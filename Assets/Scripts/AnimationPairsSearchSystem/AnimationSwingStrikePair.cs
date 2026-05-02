using Sirenix.OdinInspector;
using UnityEngine;

namespace AnimationPairsSearchSystem
{
    [CreateAssetMenu(fileName = "AnimationPair", menuName = "Scriptables/AnimationPair")]
    public class AnimationSwingStrikePair : ScriptableObject
    {
        [HideInInspector][SerializeField] private AnimationClip swingAnimation;
        [HideInInspector] [SerializeField] private AnimationClip strikeAnimation;

        [HideInInspector] [SerializeField] private float swingEndTime;
        [HideInInspector] [SerializeField] private float strikeStartTime;
        [HideInInspector] [SerializeField] private float blendTime;
        
        [ShowInInspector] public float SwingLength => SwingEndTime;
        [ShowInInspector] public float StrikeLength => StrikeAnimation?.length - StrikeStartTime ?? 0f;
        [ShowInInspector] public AnimationClip SwingAnimation => swingAnimation;
        [ShowInInspector] public AnimationClip StrikeAnimation => strikeAnimation;

        [ShowInInspector] public float SwingEndTime => swingEndTime;
        [ShowInInspector] public float StrikeStartTime => strikeStartTime;
        [ShowInInspector] public float BlendTime => blendTime;

        public void Initialize(
            AnimationClip newSwingAnimation,
            AnimationClip newStrikeAnimation,
            float newSwingEndTime,
            float newStrikeStartTime,
            float newBlendTime)
        {
            swingAnimation = newSwingAnimation;
            strikeAnimation = newStrikeAnimation;
            swingEndTime = newSwingEndTime;
            strikeStartTime = newStrikeStartTime;
            blendTime = newBlendTime;
        }
    }
}