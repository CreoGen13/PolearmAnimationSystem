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
            // Set the color with custom alpha.
            Gizmos.color = color; // Red with custom alpha

            // Draw the sphere.
            Gizmos.DrawSphere(transform.position, radius);

            // Draw wire sphere outline.
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}