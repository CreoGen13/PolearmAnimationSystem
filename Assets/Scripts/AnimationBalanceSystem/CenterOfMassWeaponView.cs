using UnityEngine;

namespace AnimationBalanceSystem
{
    public class CenterOfMassWeaponView : MonoBehaviour
    {
        [SerializeField] private float mass;
        [SerializeField] private Transform weaponTransform;
		
        public float Mass => mass;

        public Vector3 GetCenterOfMass()
        {
            return weaponTransform.position;
        }
    }
}