using TMPro;
using UnityEngine;

namespace Infrastructure.UtilityMonoBehaviour
{
	public class FpsView : MonoBehaviour
	{
		[SerializeField] private TextMeshProUGUI fpsText;
		[SerializeField] private float hudRefreshRate = 1f;

		private float _timer;

		private void Awake () {
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = 75;
		}
	
		private void Update()
		{
			if (!(Time.unscaledTime > _timer))
			{
				return;
			}

			var fps = (int)(1f / Time.unscaledDeltaTime);
			fpsText.text = "FPS: " + fps;
			_timer = Time.unscaledTime + hudRefreshRate;
		}
	}
}
