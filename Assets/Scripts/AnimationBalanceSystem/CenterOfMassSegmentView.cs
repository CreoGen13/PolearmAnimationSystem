using UnityEngine;

namespace AnimationBalanceSystem
{
    public class CenterOfMassSegmentView : MonoBehaviour
    {
		[SerializeField] private float percentOfMass;
		[Range(0, 1)]
		[SerializeField] private float comPosition;
        [SerializeField] private Transform segmentStart;
		[SerializeField] private Transform segmentEnd;
		
		public float PercentOfMass => percentOfMass;

		public Vector3 GetCenterOfMass()
		{
			return Vector3.Lerp(segmentStart.position, segmentEnd.position, comPosition);
		}
    }
}