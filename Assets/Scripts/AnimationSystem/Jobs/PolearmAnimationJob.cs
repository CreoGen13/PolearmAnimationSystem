using UnityEngine;
using UnityEngine.Animations;

namespace AnimationSystem.Jobs
{
    public struct PolearmAnimationJob : IAnimationJob
    {
        public PolearmAnimationJobConfig Config;

        public float HitTargetWeight;
        public float WeaponLocalPositionEndWeight;
        public float HandWeaponWeight;
        
        public Quaternion WeaponCorrectionDeltaRotation;
        public Vector3 WeaponCorrectionDeltaPosition;
        
        public Vector3 WeaponObjectLocalStartPosition;
        public Vector3 WeaponObjectLocalEndPosition;
        
        public TransformSceneHandle WeaponBone;
        public TransformStreamHandle Weapon;
        public TransformStreamHandle WeaponObject;
        
        public TransformSceneHandle LeftHandRotationTransform;
        public TransformSceneHandle RightHandRotationTransform;
        public TransformSceneHandle LeftHandPositionTransform;
        public TransformSceneHandle RightHandPositionTransform;
        
        public TransformStreamHandle LeftHandTarget;
        public TransformStreamHandle RightHandTarget;
        
        public void ProcessAnimation(AnimationStream stream)
        {
            var weaponBonePosition = WeaponBone.GetPosition(stream);
            var weaponBoneRotation = WeaponBone.GetRotation(stream);

            var weaponPosition = Vector3.Lerp(
                Vector3.zero,
                WeaponCorrectionDeltaPosition,
                HitTargetWeight
            ) + weaponBonePosition;
            var weaponRotation = Quaternion.Slerp(
                Quaternion.identity,
                WeaponCorrectionDeltaRotation,
                HitTargetWeight
            ) * weaponBoneRotation;
            var weaponLocalPosition = Vector3.Lerp(
                WeaponObjectLocalStartPosition,
                WeaponObjectLocalEndPosition,
                WeaponLocalPositionEndWeight);
            
            WeaponObject.SetLocalPosition(
                stream,
                weaponLocalPosition);
            Weapon.SetGlobalTR(
                stream,
                weaponPosition,
                weaponRotation,
                false);

            var weaponLocalPositionDelta = weaponLocalPosition - WeaponObjectLocalStartPosition;
            var handLocalPositionDelta = weaponLocalPositionDelta * HandWeaponWeight;
            var isGrowing = WeaponObjectLocalEndPosition.y > WeaponObjectLocalStartPosition.y;
            var leftHandPosition = weaponPosition + weaponRotation * (
                Config.HandsLocalOffset +
                LeftHandPositionTransform.GetLocalPosition(stream) +
                (isGrowing ? handLocalPositionDelta : Vector3.zero));
            var leftHandRotation = LeftHandRotationTransform.GetRotation(stream);
            var rightHandPosition = weaponPosition + weaponRotation * (
                Config.HandsLocalOffset +
                RightHandPositionTransform.GetLocalPosition(stream) +
                (isGrowing ? Vector3.zero : handLocalPositionDelta));
            var rightHandRotation = RightHandRotationTransform.GetRotation(stream);
            LeftHandTarget.SetGlobalTR(
                stream,
                leftHandPosition,
                leftHandRotation,
                false);
            RightHandTarget.SetGlobalTR(
                stream,
                rightHandPosition,
                rightHandRotation,
                false);
        }

        public void ProcessRootMotion(AnimationStream stream)
        {
            // ignored
        }
    }
}