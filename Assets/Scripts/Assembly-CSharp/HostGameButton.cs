using System.Collections;
using InnerNet;
using UnityEngine;

public class HostGameButton : MonoBehaviour
{
	public AudioClip IntroMusic;

	public TextBox NameText;

	public string targetScene;

	public SpriteRenderer FillScreen;

	public GameModes GameMode;

	public void OnClick()
	{
		if ((bool)NameText)
		{
			SaveManager.PlayerName = NameText.text;
			if (!JoinGameButton.IsValidName(SaveManager.PlayerName))
			{
				StartCoroutine(Effects.Shake(NameText.transform));
				return;
			}
		}
		if (!TempData.GameStarting)
		{
			TempData.GameStarting = true;
			StartCoroutine(CoStartGame());
		}
	}

	private IEnumerator CoStartGame()
	{
		try
		{
			SoundManager.Instance.StopAllSound();
			AmongUsClient.Instance.GameMode = GameMode;
			switch (GameMode)
			{
			case GameModes.LocalGame:
				InnerNetServer.Instance.StartAsServer();
				AmongUsClient.Instance.NetworkAddress = "127.0.0.1";
				AmongUsClient.Instance.MainMenuScene = "MatchMaking";
				break;
			case GameModes.OnlineGame:
				AmongUsClient.Instance.NetworkAddress = "18.218.20.65";
				AmongUsClient.Instance.MainMenuScene = "MMOnline";
				break;
			case GameModes.FreePlay:
				InnerNetServer.Instance.StartAsServer();
				AmongUsClient.Instance.NetworkAddress = "127.0.0.1";
				AmongUsClient.Instance.MainMenuScene = "MainMenu";
				break;
			}
			yield return new WaitForSeconds(0.1f);
			AmongUsClient.Instance.OnlineScene = targetScene;
			IEnumerator connectWaiter = AmongUsClient.Instance.CoConnect(MatchMakerModes.Both);
			if ((bool)FillScreen)
			{
				SoundManager.Instance.CrossFadeSound("MainBG", null, 0.5f);
				FillScreen.gameObject.SetActive(true);
				for (float time2 = 0f; time2 < 0.25f; time2 += Time.deltaTime)
				{
					FillScreen.color = Color.Lerp(Color.clear, Color.black, time2 / 0.25f);
					yield return null;
				}
				FillScreen.color = Color.black;
			}
			yield return connectWaiter;
			if (AmongUsClient.Instance.mode != 0)
			{
				yield break;
			}
			if ((bool)FillScreen)
			{
				SoundManager.Instance.CrossFadeSound("MainBG", IntroMusic, 0.5f);
				for (float time = 0f; time < 0.25f; time += Time.deltaTime)
				{
					FillScreen.color = Color.Lerp(Color.black, Color.clear, time / 0.25f);
					yield return null;
				}
				FillScreen.color = Color.clear;
			}
			if (DestroyableSingleton<MMOnlineManager>.InstanceExists && (bool)DestroyableSingleton<MMOnlineManager>.Instance.DisconnectPopup)
			{
				DestroyableSingleton<MMOnlineManager>.Instance.DisconnectPopup.Show();
			}
		}
		finally
		{
			TempData.GameStarting = false;
		}
	}
}
