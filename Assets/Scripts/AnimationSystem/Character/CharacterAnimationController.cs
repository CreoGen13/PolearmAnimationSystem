using AnimationSystem.Jobs;
using Infrastructure.Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace AnimationSystem.Character
{
    [ExecuteInEditMode]
    public class CharacterAnimationController : MonoBehaviour,
        IInitializable,
        IUpdatable,
        IDestroyable
    {
        private const int IDLE_INDEX = 0;
        private const int SWING_INDEX = 1;
        private const int STRIKE_INDEX = 2;
        
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private Transform hitTarget;
        
        [Header("Clips")]
        [SerializeField] private AnimationClip idleClip;
        [SerializeField] private AnimationClip swingClip;
        [SerializeField] private AnimationClip strikeClip;

        [Header("Settings")]
        [Range(0f, 1f)]
        [SerializeField] private float swingBlendTime;
        [Range(0f, 1f)]
        [SerializeField] private float strikeBlendTime;
        [Range(0f, 1f)]
        [SerializeField] private float idleBlendTime;
        [Range(0f, 1f)]
        [SerializeField] private float weaponMovingTime;
        [Range(0f, 1f)]
        [SerializeField] private float handsMovingTime;
        
        [Header("Weapon Settings")]
        [SerializeField] private Transform weaponBone;
        [SerializeField] private Transform weapon;
        [SerializeField] private Transform weaponObject;
        [SerializeField] private MeshFilter weaponMeshFilter;
        
        [SerializeField] private Vector3 weaponRotationOffset;
        [SerializeField] private float startWeaponOffset;
        
        [Header("Hands Settings")]
        [SerializeField] private Transform leftHandRotationTransform;
        [SerializeField] private Transform rightHandRotationTransform;

        [SerializeField] private Transform leftHandTarget;
        [SerializeField] private Transform rightHandTarget;
        
        [SerializeField] private Vector3 leftHandOffset;
        [SerializeField] private Vector3 rightHandOffset;
        
        [Header("Sampler")]
        [SerializeField] private Transform samplerWeaponBone;
        [SerializeField] private Transform samplerStrikePivotBone;
        [SerializeField] private Transform samplerShouldersBone;
        [SerializeField] private Animator samplerAnimator;
        
        private PlayableGraph _graph;
        private PlayableGraph _samplerGraph;
        private AnimationMixerPlayable _mixer;
        private AnimationClipPlayable _idleClipPlayable;
        private AnimationClipPlayable _swingClipPlayable;
        private AnimationClipPlayable _strikeClipPlayable;
        private AnimationClipPlayable _sampleStrikeClipPlayable;
        private AnimationScriptPlayable _animationPlayable;

        private CharacterAnimationState _animationState;
        private float _blendPassedTime;
        private float _strikePassedTime;
        private float _moveHandsPassedTime;

        [Button]
        private void PlayStrike()
        {
            _moveHandsPassedTime = 0;
            _swingClipPlayable.SetTime(0);
            SetJobMovingHandsWeight(0, 0);
            
            Sample(out var bakedWeaponEndPos, out var bakedWeaponEndRot);
            
            var strikePivotPos = samplerStrikePivotBone.position;
            var weaponLocalOffset = new Vector3(0, (bakedWeaponEndPos - samplerStrikePivotBone.position).magnitude, 0);
            var targetPos = hitTarget.position;
            var targetDir = (targetPos - strikePivotPos).normalized;
            var deltaRot = Quaternion.FromToRotation(bakedWeaponEndRot * Vector3.up, targetDir);
            var targetWeaponRot = deltaRot * bakedWeaponEndRot;
            var targetWeaponPos = strikePivotPos + targetWeaponRot * weaponLocalOffset;
            var deltaPos = targetWeaponPos - bakedWeaponEndPos;
            var currentDist = weaponObject.localPosition.y;
            var targetDist = (targetWeaponPos - targetPos).magnitude - 1;
            var useMoveHands = !Mathf.Approximately(currentDist, targetDist);
            
            var job = _animationPlayable.GetJobData<CalculateAnimationTransformsJob>();
            job.WeaponCorrectionDeltaRotation = deltaRot;
            job.WeaponCorrectionDeltaPosition = deltaPos;
            job.WeaponObjectLocalStartPosition = new Vector3(0, currentDist, 0);
            job.WeaponObjectLocalEndPosition = new Vector3(0, targetDist, 0);
            _animationPlayable.SetJobData(job);
            
            ChangeState(useMoveHands ?
                CharacterAnimationState.MoveHands :
                CharacterAnimationState.IdleToSwing);
        }
        
        public void Initialize()
        {
            ChangeState(CharacterAnimationState.Idle);
            CreateSamplerGraph();
            CreateGraph();
        }
        
        public void ManualUpdate()
        {
            switch (_animationState)
            {
                case CharacterAnimationState.Idle:
                {
                    return;
                }
                case CharacterAnimationState.MoveHands:
                {
                    var maxTime = weaponMovingTime + handsMovingTime;
                    var handsPassedTime = _moveHandsPassedTime - weaponMovingTime;
                    var deltaWeaponWeight = Mathf.Clamp01(_moveHandsPassedTime / weaponMovingTime);
                    var deltaHandsWeight = Mathf.Clamp01(_moveHandsPassedTime < weaponMovingTime ?
                        1 :
                        (handsMovingTime - handsPassedTime) / handsMovingTime);

                    SetJobMovingHandsWeight(deltaWeaponWeight, deltaHandsWeight);

                    _moveHandsPassedTime += Time.deltaTime;
                    
                    if (_moveHandsPassedTime >= maxTime)
                    {
                        _swingClipPlayable.SetTime(0);
                        
                        SetJobMovingHandsWeight(1, 0);
                        ChangeState(CharacterAnimationState.IdleToSwing);
                    }
                    
                    break;
                }
                case CharacterAnimationState.IdleToSwing:
                {
                    var deltaWeight = Mathf.Clamp01(_blendPassedTime / swingBlendTime);

                    SetAnimationWeights(1f - deltaWeight, deltaWeight, 0);

                    if (deltaWeight >= 1f)
                    {
                        ChangeState(CharacterAnimationState.Swing);
                    }
                    
                    break;
                }
                case CharacterAnimationState.Swing:
                {
                    if (_swingClipPlayable.GetTime() >= swingClip.length)
                    {
                        _strikePassedTime = 0;
                        _strikeClipPlayable.SetTime(0);
                        
                        SetJobHitTargetWeight(0);
                        ChangeState(CharacterAnimationState.SwingToStrike);
                    }
                    
                    break;
                }
                case CharacterAnimationState.SwingToStrike:
                {
                    var deltaWeight = Mathf.Clamp01(_blendPassedTime / strikeBlendTime);

                    SetAnimationWeights(0, 1f - deltaWeight, deltaWeight);
                    SetJobHitTargetWeight(_strikePassedTime / strikeClip.length);
                    
                    _strikePassedTime += Time.deltaTime;
                    
                    if (deltaWeight >= 1f)
                    {
                        ChangeState(CharacterAnimationState.Strike);
                    }
                    
                    break;
                }
                case CharacterAnimationState.Strike:
                {
                    SetJobHitTargetWeight(_strikePassedTime / strikeClip.length);
                    
                    _strikePassedTime += Time.deltaTime;
                    
                    if (_strikeClipPlayable.GetTime() >= strikeClip.length)
                    {
                        _strikePassedTime = idleBlendTime;
                        _idleClipPlayable.SetTime(0);
                        
                        ChangeState(CharacterAnimationState.StrikeToIdle);
                    }
                    
                    break;
                }
                case CharacterAnimationState.StrikeToIdle:
                {
                    var deltaWeight = Mathf.Clamp01(_blendPassedTime / idleBlendTime);

                    SetAnimationWeights(deltaWeight, 0, 1f - deltaWeight);
                    SetJobHitTargetWeight(_strikePassedTime / idleBlendTime);
                    
                    _strikePassedTime -= Time.deltaTime;
                    
                    if (deltaWeight >= 1f)
                    {
                        SetJobHitTargetWeight(0);
                        ChangeState(CharacterAnimationState.Idle);
                    }
                    
                    break;
                }
            }

            _blendPassedTime += Time.deltaTime;
        }

        private void CreateSamplerGraph()
        {
            _samplerGraph = PlayableGraph.Create("Sampler Animation");
            _samplerGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            
            _sampleStrikeClipPlayable = AnimationClipPlayable.Create(_samplerGraph, strikeClip);
            _sampleStrikeClipPlayable.SetApplyFootIK(false);
            _sampleStrikeClipPlayable.SetApplyPlayableIK(false);
            _sampleStrikeClipPlayable.SetTime(strikeClip.length);
            _sampleStrikeClipPlayable.SetSpeed(0);
            
            var output = AnimationPlayableOutput.Create(
                _samplerGraph,
                "Sampler Animation Output",
                samplerAnimator
            );
            
            output.SetSourcePlayable(_sampleStrikeClipPlayable);

            _samplerGraph.Play();
        }
        private void Sample(out Vector3 weaponPos, out Quaternion weaponRot)
        {
            _samplerGraph.Evaluate(0);

            weaponPos = samplerWeaponBone.position;
            weaponRot = samplerWeaponBone.rotation;
        }

        private void CreateGraph()
        {
            _graph = PlayableGraph.Create("Animation");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            _mixer = AnimationMixerPlayable.Create(_graph, 3);
            var output = AnimationPlayableOutput.Create(_graph, "Animation Output", animator);
            
            _idleClipPlayable = AnimationClipPlayable.Create(_graph, idleClip);
            _swingClipPlayable = AnimationClipPlayable.Create(_graph, swingClip);
            _strikeClipPlayable = AnimationClipPlayable.Create(_graph, strikeClip);

            _graph.Connect(_idleClipPlayable, 0, _mixer, IDLE_INDEX);
            _graph.Connect(_swingClipPlayable, 0, _mixer, SWING_INDEX);
            _graph.Connect(_strikeClipPlayable, 0, _mixer, STRIKE_INDEX);

            _mixer.SetInputWeight(IDLE_INDEX, 1f);
            _mixer.SetInputWeight(SWING_INDEX, 0f);
            _mixer.SetInputWeight(STRIKE_INDEX, 0f);

            var job = CreateAnimationJob();
            
            _animationPlayable = AnimationScriptPlayable.Create(_graph, job, 1);
            _graph.Connect(_mixer, 0, _animationPlayable, 0);
            _animationPlayable.SetInputWeight(0, 1f);

            output.SetSourcePlayable(_animationPlayable);

            _graph.Play();
        }
        private CalculateAnimationTransformsJob CreateAnimationJob()
        {
            var startWeaponObjectPositionOffset = new Vector3(0, startWeaponOffset, 0);
            var weaponRotOffset = Quaternion.Euler(weaponRotationOffset);
            var localShouldersPoint = samplerWeaponBone.InverseTransformPoint(samplerShouldersBone.position);
            var handsLocalOffset = new Vector3(0, localShouldersPoint.y, 0);
            var leftHandLocalOffset = handsLocalOffset + leftHandOffset;
            var rightHandLocalOffset = handsLocalOffset + rightHandOffset;
            
            var config = new CalculateAnimationTransformsJobConfig
            {
                WeaponRotationOffset = weaponRotOffset,
                
                LeftHandLocalPositionOffset = leftHandLocalOffset,
                RightHandLocalPositionOffset = rightHandLocalOffset,
            };
            
            return new CalculateAnimationTransformsJob
            {
                Config = config,
                WeaponObjectLocalStartPosition = startWeaponObjectPositionOffset,
                
                WeaponBone = animator.BindStreamTransform(weaponBone),
                Weapon = animator.BindStreamTransform(weapon),
                WeaponObject = animator.BindStreamTransform(weaponObject),
                LeftHandRotationTransform = animator.BindSceneTransform(leftHandRotationTransform),
                RightHandRotationTransform = animator.BindSceneTransform(rightHandRotationTransform),
                LeftHandTarget = animator.BindStreamTransform(leftHandTarget),
                RightHandTarget = animator.BindStreamTransform(rightHandTarget)
            };
        }

        private void ChangeState(CharacterAnimationState newState)
        {
            _blendPassedTime = 0;
            _animationState = newState;
        }
        private void SetAnimationWeights(float idle, float swing, float strike)
        {
            _mixer.SetInputWeight(IDLE_INDEX, idle);
            _mixer.SetInputWeight(SWING_INDEX, swing);
            _mixer.SetInputWeight(STRIKE_INDEX, strike);
        }
        private void SetJobHitTargetWeight(float hitTargetWeight)
        {
            var job = _animationPlayable.GetJobData<CalculateAnimationTransformsJob>();
            job.HitTargetWeight = Mathf.Clamp01(hitTargetWeight);
            _animationPlayable.SetJobData(job);
        }
        private void SetJobMovingHandsWeight(float weaponWeight, float handWeight)
        {
            var job = _animationPlayable.GetJobData<CalculateAnimationTransformsJob>();
            job.WeaponLocalPositionEndWeight = Mathf.Clamp01(weaponWeight);
            job.HandWeaponWeight = Mathf.Clamp01(handWeight);
            _animationPlayable.SetJobData(job);
        }

        public void ManualDestroy()
        {
            if (_graph.IsValid())
            {
                _graph.Destroy();
            }
            
            if (_samplerGraph.IsValid())
            {
                _samplerGraph.Destroy();
            }
        }
    }
}