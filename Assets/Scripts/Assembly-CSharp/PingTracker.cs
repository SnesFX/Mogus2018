using UnityEngine;

public class PingTracker : MonoBehaviour
{
	public TextRenderer text;

	private void Update()
	{
		if ((bool)AmongUsClient.Instance)
		{
			if (AmongUsClient.Instance.GameMode == GameModes.FreePlay)
			{
				base.gameObject.SetActive(false);
			}
			text.Text = string.Format("Ping: {0} ms", AmongUsClient.Instance.Ping);
		}
	}
}
