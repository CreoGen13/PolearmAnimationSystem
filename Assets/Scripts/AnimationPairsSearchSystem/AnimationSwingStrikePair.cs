using Sirenix.OdinInspector;
using UnityEngine;

namespace AnimationPairsSearchSystem
{
    [CreateAssetMenu(fileName = "AnimationPair", menuName = "Scriptables/AnimationPair")]
    public class AnimationSwingStrikePair : ScriptableObject
    {
        [ReadOnly][ShowInInspector] public AnimationClip SwingAnimation { get; private set; }
        [ReadOnly][ShowInInspector] public AnimationClip StrikeAnimation { get; private set; }
        
        [ReadOnly][ShowInInspector] public float SwingEndTime { get; private set; }
        [ShowInInspector] public float SwingLength => SwingEndTime;
        [ReadOnly][ShowInInspector] public float StrikeStartTime { get; private set; }
        [ShowInInspector] public float StrikeLength => StrikeAnimation?.length - StrikeStartTime ?? 0f;
        [ReadOnly][ShowInInspector] public float BlendTime { get; private set; }

        public void Initialize(
            AnimationClip swingAnimation,
            AnimationClip strikeAnimation,
            float swingEndTime,
            float strikeStartTime,
            float blendTime)
        {
            SwingAnimation = swingAnimation;
            StrikeAnimation = strikeAnimation;
            SwingEndTime = swingEndTime;
            StrikeStartTime = strikeStartTime;
            BlendTime = blendTime;
        }
    }
}