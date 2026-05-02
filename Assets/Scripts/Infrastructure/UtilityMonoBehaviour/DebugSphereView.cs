using UnityEngine;

namespace Infrastructure.UtilityMonoBehaviour
{
    [ExecuteInEditMode]
    public class DebugSphereView : MonoBehaviour
    {
        [Range(0f, 3f)]
        [SerializeField] private float radius = 0.05f;
        [SerializeField] private Color color;

        private void OnDrawGizmos()
        {
            Gizmos.color = color;
            Gizmos.DrawSphere(transform.position, radius);
        }
    }
}