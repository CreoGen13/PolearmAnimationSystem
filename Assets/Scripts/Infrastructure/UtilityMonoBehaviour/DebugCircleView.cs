using UnityEngine;

namespace Infrastructure.UtilityMonoBehaviour
{
    public class DebugCircleView : MonoBehaviour
    {
        [SerializeField] private float radius = 0.05f;
        [Range(0f, 100f)]
        [SerializeField] private int segments = 16;
        [SerializeField] private Color color;

        public void ChangeRadius(float newRadius)
        {
            radius = newRadius;
        }

        private void OnDrawGizmos()
        {
            var angleStep = 360.0f / segments;
            
            angleStep *= Mathf.Deg2Rad;
            
            var lineStart = Vector3.zero;
            var lineEnd = Vector3.zero;
 
            for (var i = 0; i < segments; i++)
            {
                lineStart.x = Mathf.Cos(angleStep * i) ;
                lineStart.z = Mathf.Sin(angleStep * i);
 
                lineEnd.x = Mathf.Cos(angleStep * (i + 1));
                lineEnd.z = Mathf.Sin(angleStep * (i + 1));
 
                lineStart *= radius;
                lineEnd *= radius;
 
                lineStart += transform.position;
                lineEnd += transform.position;
 
                Debug.DrawLine(lineStart, lineEnd, color);
            }
        }
    }
}