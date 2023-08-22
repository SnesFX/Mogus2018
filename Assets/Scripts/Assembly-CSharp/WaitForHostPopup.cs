using UnityEngine;

public class WaitForHostPopup : DestroyableSingleton<WaitForHostPopup>
{
	public void Show()
	{
		if ((bool)AmongUsClient.Instance && AmongUsClient.Instance.ClientId > 0)
		{
			base.transform.position = new Vector3(0f, 0f, -300f);
		}
	}

	public void ExitGame()
	{
		AmongUsClient.Instance.ExitGame();
	}

	public void Hide()
	{
		base.transform.position = new Vector3(300f, 0f, 0f);
	}
}
