using System;
using System.Collections.Generic;
using UnityEngine;

namespace AnimationBalanceSystem
{
    public class CenterOfMassView : MonoBehaviour
    {
        [SerializeField] private List<CenterOfMassSegmentView> segments;
        [SerializeField] private CenterOfMassWeaponView weapon;
        [SerializeField] private Transform centerOfMassGizmo;
        [SerializeField] private Transform centerOfMassProjectionGizmo;

        private void Update()
        {
            var com = GetCenterOfMass();
            centerOfMassGizmo.position = com;
            centerOfMassProjectionGizmo.position = new Vector3(com.x, centerOfMassProjectionGizmo.position.y, com.z);
        }

        public Vector3 GetCenterOfMass()
        {
            var weightedSum = Vector3.zero;
            var totalMass = 0f;
            
            foreach (var segment in segments)
            {
                var segmentCom = segment.GetCenterOfMass();
                
                weightedSum += segmentCom * segment.PercentOfMass;
                totalMass += segment.PercentOfMass;
            }
            
            weightedSum += weapon.GetCenterOfMass() * weapon.Mass;
            totalMass += weapon.Mass;
            
            return weightedSum / totalMass;
        }
    }
}