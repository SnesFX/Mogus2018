using UnityEngine;

public class ExitGameButton : MonoBehaviour
{
	public void Start()
	{
		if (!DestroyableSingleton<HudManager>.InstanceExists)
		{
			base.gameObject.SetActive(false);
		}
	}

	public void OnClick()
	{
		AmongUsClient.Instance.ExitGame();
	}
}
