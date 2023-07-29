using UnityEngine;

namespace AnimationSystem.Jobs
{
    public struct CalculateAnimationTransformsJobConfig
    {
        public Quaternion WeaponRotationOffset;
        
        public Vector3 LeftHandLocalPositionOffset;
        public Vector3 RightHandLocalPositionOffset;
    }
}