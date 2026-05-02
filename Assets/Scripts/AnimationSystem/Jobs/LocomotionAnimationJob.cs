using UnityEngine;
using UnityEngine.Animations;

namespace AnimationSystem.Jobs
{
    public struct LocomotionAnimationJob : IAnimationJob
    {
        public LocomotionAnimationJobConfig Config;
            
        public float StepWeight;
        
        public Quaternion FootRotation;
        public Vector3 FootStartPosition;
        public Vector3 FootEndPosition;
        
        public TransformStreamHandle RightFootTarget;
        
        public void ProcessAnimation(AnimationStream stream)
        {
            var sinWeight = Mathf.Sin(StepWeight * Mathf.PI);
            var additionHeight = new Vector3(0, sinWeight * Config.StepHeight, 0);
            var position = Vector3.Lerp(FootStartPosition, FootEndPosition, StepWeight);
            var finalPosition = position +  additionHeight;
            
            RightFootTarget.SetGlobalTR(
                stream,
                finalPosition,
                FootRotation,
                // Vector3.one,
                false);
        }

        public void ProcessRootMotion(AnimationStream stream)
        {
            // ignored
        }
    }
}