using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace UnityEngine.Animations.Rigging
{
    public static class ChainTrianglesUtils
    {
        public static bool SolveTriangles(
            ref NativeArray<Vector3> linkPositions,
            ref NativeArray<float> linkLengths,
            Vector3 target,
            float tolerance,
            float maxReach
        )
        {
            var rootToTargetDir = target - linkPositions[0];
            if (rootToTargetDir.sqrMagnitude > Square(maxReach))
            {
                var dir = rootToTargetDir.normalized;
                for (int i = 1; i < linkPositions.Length; ++i)
                    linkPositions[i] = linkPositions[i - 1] + dir * linkLengths[i - 1];

                return true;
            }
            else
            {
                int tipIndex = linkPositions.Length - 1;
                float sqrTolerance = Square(tolerance);
                if (SqrDistance(linkPositions[tipIndex], target) > sqrTolerance)
                {
                    float leftLength = maxReach;

                    for (int i = 0; i < linkPositions.Length - 1; i++)
                    {
                        leftLength -= linkLengths[i];
                        var targetLocal = target - linkPositions[i];
                        var distance = targetLocal.magnitude;
                        var angleTarget = Quaternion.LookRotation(targetLocal);
                        Quaternion angle;
                        if (distance + tolerance >= linkLengths[i] + leftLength)
                        {
                            angle = angleTarget;
                        }
                        else if (distance < Mathf.Abs(linkLengths[i] - leftLength))
                        {
                            angle = Quaternion.LookRotation(targetLocal * -1);
                        }
                        else
                        {
                            var radAngle = Mathf.Acos(
                                (Mathf.Pow(distance, 2) + Mathf.Pow(linkLengths[i], 2) - Mathf.Pow(leftLength, 2)) /
                                (2 * linkLengths[i] * distance));
                            var floatAngle = radAngle * Mathf.Rad2Deg;
                            var normal = Vector3.Cross(targetLocal + Vector3.forward, targetLocal).normalized;
                            angle = angleTarget * Quaternion.AngleAxis(floatAngle, normal);
                        }

                        linkPositions[i + 1] = angle * (Vector3.forward * linkLengths[i]) + linkPositions[i];
                    }

                    return true;
                }

                return false;
            }
        }
        private static float SqrDistance(Vector3 lhs, Vector3 rhs)
        {
            return (rhs - lhs).sqrMagnitude;
        }
        private static float Square(float value)
        {
            return value * value;
        }
    }
}
