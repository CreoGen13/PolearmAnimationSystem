using System.Collections.Generic;
using Infrastructure.Utility;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace AnimationPairsSearchSystem
{
    [ExecuteInEditMode]
    public class AnimationPairsSearchView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator samplerAnimator;

        [Space]
        [SerializeField] private Transform samplerHipsBone;
        [SerializeField] private Transform samplerSpineBone;
        [SerializeField] private Transform samplerChestBone;
        [SerializeField] private Transform samplerUpperChestBone;
        [SerializeField] private Transform samplerNeckBone;
        [SerializeField] private Transform samplerHeadBone;
        [SerializeField] private Transform samplerWeaponBone;

        [Header("Settings")]
        [SerializeField] private AvatarMask avatarMask;
        
        [Space]
        [SerializeField] private float stepSwing;
        [SerializeField] private float stepStrike;
        [SerializeField] private float maximumScore;
        [SerializeField] private float distanceToAngleWeightMultiplier;
        
        [Space]
        [SerializeField] private float hipsWeight;
        [SerializeField] private float spineWeight;
        [SerializeField] private float chestWeight;
        [SerializeField] private float upperChestWeight;
        [SerializeField] private float neckWeight;
        [SerializeField] private float headWeight;
        [SerializeField] private float weaponRotationWeight;
        [SerializeField] private float weaponPositionWeight;
        
        [Header("Paths")]
        [SerializeField] private string swingClipsPath;
        [SerializeField] private string strikeClipsPath;
        [SerializeField] private string savePairsPath;

        [Button]
        private void SearchAndCreateAnimationPairs()
        {
            if (string.IsNullOrEmpty(swingClipsPath))
            {
                return;
            }
            
            if (string.IsNullOrEmpty(strikeClipsPath))
            {
                return;
            }
            
            if (string.IsNullOrEmpty(savePairsPath))
            {
                return;
            }

            var swingClips = AssetsDatabaseUtility.GetAssetsAtPath<PreparedAnimationClip>(swingClipsPath);
            var strikeClips = AssetsDatabaseUtility.GetAssetsAtPath<PreparedAnimationClip>(strikeClipsPath);

            for (var i = 0; i < swingClips.Length; i++)
            {
                for (var j = 0; j < strikeClips.Length; j++)
                {
                    var swingClip = swingClips[i];
                    var strikeClip = strikeClips[j];
                    var result = GetSwingAndStrikeBlendScore(swingClip, strikeClip);

                    if (result.score > maximumScore)
                    {
                        continue;
                    }

                    var asset = ScriptableObject.CreateInstance<AnimationSwingStrikePair>();
                    asset.Initialize(swingClip.Clip, strikeClip.Clip, result.swingTime, result.strikeTime, result.blendTime);

                    AssetsDatabaseUtility.CreateAssetAtPath(asset, savePairsPath + $"Pair{i}{j}.asset");
                }
            }
        }

        private (float score, float swingTime, float strikeTime, float blendTime) GetSwingAndStrikeBlendScore(PreparedAnimationClip swingClip, PreparedAnimationClip strikeClip)
        {
            var graph = PlayableGraph.Create("Animation");
            graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            var mixer = AnimationLayerMixerPlayable.Create(graph, 2);
            var output = AnimationPlayableOutput.Create(graph, "Animation Output", samplerAnimator);
            
            var swingClipPlayable = AnimationClipPlayable.Create(graph, swingClip.Clip);
            var strikeClipPlayable = AnimationClipPlayable.Create(graph, strikeClip.Clip);

            graph.Connect(swingClipPlayable, 0, mixer, 0);
            graph.Connect(strikeClipPlayable, 0, mixer, 1);

            mixer.SetLayerMaskFromAvatarMask(0, avatarMask);
            mixer.SetLayerMaskFromAvatarMask(1, avatarMask);
            mixer.SetInputWeight(0, 1f);
            mixer.SetInputWeight(1, 0f);

            output.SetSourcePlayable(mixer);

            graph.Play();
            graph.Evaluate(0f);

            var bestScore = 100f;
            var swingTime = 0f;
            var strikeTime = 0f;

            PoseFeature strikeStartPose = null;
            PoseFeature strikeEndPose = null;

            foreach (var tSwing in SampleTimes(swingClip.BlendStartTime, swingClip.BlendEndTime, stepSwing))
            {
                foreach (var tStrike in SampleTimes(strikeClip.BlendStartTime, strikeClip.BlendEndTime, stepStrike))
                {
                    swingClipPlayable.SetTime(tSwing);
                    mixer.SetInputWeight(0, 1f);
                    mixer.SetInputWeight(1, 0f);
                    graph.Evaluate(0f);
                    var swingPose = GetPoseFeature();
                    
                    strikeClipPlayable.SetTime(tStrike);
                    mixer.SetInputWeight(0, 0f);
                    mixer.SetInputWeight(1, 1f);
                    graph.Evaluate(0f);
                    var strikePose = GetPoseFeature();

                    if (tStrike == strikeClip.BlendStartTime)
                    {
                        strikeStartPose = strikePose;
                    }

                    if (tStrike == strikeClip.BlendEndTime)
                    {
                        strikeEndPose = strikePose;
                    }

                    var score = ComparePose(swingPose, strikePose);

                    if (score > bestScore)
                    {
                        continue;
                    }
                    
                    bestScore = score;
                    swingTime = tSwing;
                    strikeTime = tStrike;
                }
            }
            
            var strikeFullBlendDuration = strikeClip.BlendEndTime - strikeClip.BlendStartTime;
            var strikeFullBlendScore = ComparePose(strikeStartPose, strikeEndPose);
            var strikeBlendSpeed = strikeFullBlendScore / strikeFullBlendDuration;
            var blendTime = NumbersUtility.RoundFloat(bestScore / strikeBlendSpeed, 3);
            
            return (bestScore, swingTime, strikeTime, blendTime);
        }
        
        private IEnumerable<float> SampleTimes(float start, float end, float step)
        {
            if (step <= 0f)
            {
                yield break;
            }

            if (end < start)
            {
                yield break;
            }

            var duration = end - start;
            var fullSteps = Mathf.FloorToInt(duration / step);

            for (var i = 0; i <= fullSteps; i++)
            {
                yield return start + i * step;
            }

            var lastRegularTime = start + fullSteps * step;
            
            if (!Mathf.Approximately(lastRegularTime, end))
            {
                yield return end;
            }
        }
        
        private float ComparePose(PoseFeature a, PoseFeature b)
        {
            var score = 0f;

            score += Quaternion.Angle(a.HipsRotation, b.HipsRotation) * hipsWeight;
            score += Quaternion.Angle(a.SpineRotation, b.SpineRotation) * spineWeight;
            score += Quaternion.Angle(a.ChestRotation, b.ChestRotation) * chestWeight;
            score += Quaternion.Angle(a.UpperChestRotation, b.UpperChestRotation) * upperChestWeight;
            score += Quaternion.Angle(a.NeckRotation, b.NeckRotation) * neckWeight;
            score += Quaternion.Angle(a.HeadRotation, b.HeadRotation) * headWeight;

            score += Quaternion.Angle(a.WeaponRotation, b.WeaponRotation) * weaponRotationWeight;
            score += Vector3.Distance(a.WeaponPosition, b.WeaponPosition) * distanceToAngleWeightMultiplier * weaponPositionWeight;

            return score;
        }

        private PoseFeature GetPoseFeature()
        {
            return new PoseFeature
            {
                HipsRotation = samplerHipsBone.rotation,
                SpineRotation = samplerSpineBone.rotation,
                ChestRotation = samplerChestBone.rotation,
                UpperChestRotation = samplerUpperChestBone.rotation,
                NeckRotation = samplerNeckBone.rotation,
                HeadRotation = samplerHeadBone.rotation,
                WeaponRotation = samplerWeaponBone.rotation,
                WeaponPosition = samplerWeaponBone.position
            };
        }
    }
}