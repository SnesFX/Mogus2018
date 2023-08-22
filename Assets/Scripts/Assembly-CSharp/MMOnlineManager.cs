using UnityEngine;

public class MMOnlineManager : DestroyableSingleton<MMOnlineManager>
{
	public DisconnectPopup DisconnectPopup;

	public GameObject HelpMenu;

	public override void Start()
	{
		base.Start();
		DisconnectPopup.Show();
		if ((bool)HelpMenu)
		{
			if (SaveManager.ShowOnlineHelp)
			{
				SaveManager.ShowOnlineHelp = false;
			}
			else
			{
				HelpMenu.gameObject.SetActive(false);
			}
		}
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			SceneChanger.ChangeScene("MainMenu");
		}
	}
}
