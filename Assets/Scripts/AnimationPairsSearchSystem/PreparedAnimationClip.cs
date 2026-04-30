using Infrastructure.Enums;
using UnityEngine;

namespace AnimationPairsSearchSystem
{
    [CreateAssetMenu(fileName = "PreparedAnimationClip", menuName = "Scriptables/PreparedAnimationClip")]
    public class PreparedAnimationClip : ScriptableObject
    {
        [SerializeField] private AnimationType animationType;
        [SerializeField] private AnimationClip clip;
        [SerializeField] private float blendStartTime;
        [SerializeField] private float blendEndTime;
        
        public AnimationType AnimationType => animationType;
        public AnimationClip Clip => clip;
        public float BlendStartTime => blendStartTime;
        public float BlendEndTime => blendEndTime;
    }
}