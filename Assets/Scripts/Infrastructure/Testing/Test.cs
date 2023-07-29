using UnityEngine;

namespace Infrastructure.Testing
{
    public class Test : MonoBehaviour
    {
        [SerializeField] private GameObject joint1;
        [SerializeField] private GameObject joint2;
        [SerializeField] private GameObject joint3;
        [SerializeField] private GameObject joint4;

        [SerializeField] private Transform targetTransform;
        [SerializeField] private Transform hintTransform;

        private void Update()
        {
            var bones = new float[] { 1, 1, 1 };
            var positions = new[]
            {
                joint1.transform.position,
                joint2.transform.position,
                joint3.transform.position,
                joint4.transform.position
            };
            var target = targetTransform.position;
            var hint = hintTransform.position;

            var angles = Calculate(bones, positions, target, hint);
            joint1.transform.rotation = angles[0];
            joint2.transform.rotation = angles[1];
            joint3.transform.rotation = angles[2];
        }
    
        private Quaternion[] Calculate(float[] bones, Vector3[] positions, Vector3 target, Vector3 hint)
        {
            Quaternion[] angles = new Quaternion[bones.Length];
            float tolerance = 0.1f;

            float leftLength = 0;
            foreach (var bone in bones)
            {
                leftLength += bone;
            }

            for (int i = 0; i < positions.Length - 1; i++)
            {
                leftLength -= bones[i];
                var targetLocal = target - positions[i];
                var distance = targetLocal.magnitude;
                var angleTarget = Quaternion.LookRotation(targetLocal);
                //var angleCurrent = Quaternion.LookRotation(positions[i + 1]);
                Quaternion angle;
                if (distance + tolerance >= bones[i] + leftLength)
                {
                    angle = angleTarget;
                }
                else if (distance < Mathf.Abs(bones[i] - leftLength))
                {
                    angle = Quaternion.LookRotation(targetLocal * -1);
                }
                else
                {
                    var radAngle = Mathf.Acos(
                        (Mathf.Pow(distance, 2) + Mathf.Pow(bones[i], 2) - Mathf.Pow(leftLength, 2)) /
                        (2 * bones[i] * distance));
                    var floatAngle = radAngle * Mathf.Rad2Deg;
                    var normal = Vector3.Cross(targetLocal + Vector3.forward, targetLocal).normalized;
                    angle = angleTarget * Quaternion.AngleAxis(floatAngle, normal);
                }

                angles[i] = angle;
            }

            return angles;
        }
    }
}
