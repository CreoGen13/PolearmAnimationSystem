using Infrastructure.UtilityMonoBehaviour;
using UnityEngine;

namespace AnimationBalanceSystem
{
    public class SupportAreaView : MonoBehaviour
    {
        [SerializeField] private Transform leftFootStart;
        [SerializeField] private Transform leftFootEnd;
        [SerializeField] private Transform rightFootStart;
        [SerializeField] private Transform rightFootEnd;
        [SerializeField] private DebugCircleView debugCircleView;

        private void Update()
        {
            var leftFootPosition = Vector3.Lerp(leftFootStart.position, leftFootEnd.position, 0.5f);
            var rightFootPosition = Vector3.Lerp(rightFootStart.position, rightFootEnd.position, 0.5f);
            var position = Vector3.Lerp(leftFootPosition, rightFootPosition, 0.5f);
            transform.position = new Vector3(position.x, transform.position.y, position.z);
            
            debugCircleView.ChangeRadius((leftFootPosition - rightFootPosition).magnitude / 2);
        }
    }
}