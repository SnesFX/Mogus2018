using System;
using System.Collections;
using InnerNet;
using PowerTools;
using UnityEngine;

public class JoinGameButton : MonoBehaviour, IConnectButton
{
	public AudioClip IntroMusic;

	public TextBox NameText;

	public TextBox GameIdText;

	public TextRenderer gameNameText;

	public float timeRecieved;

	public SpriteRenderer FillScreen;

	public SpriteAnim connectIcon;

	public AnimationClip connectClip;

	public GameModes GameMode;

	public string netAddress;

	public void OnClick()
	{
		if (string.IsNullOrWhiteSpace(netAddress))
		{
			return;
		}
		if ((bool)NameText)
		{
			SaveManager.PlayerName = NameText.text;
			if (!IsValidName(SaveManager.PlayerName))
			{
				StartCoroutine(Effects.Shake(NameText.transform));
				return;
			}
		}
		if (!DestroyableSingleton<MatchMaker>.Instance.Connecting(this))
		{
			return;
		}
		AmongUsClient.Instance.GameMode = GameMode;
		if (GameMode == GameModes.OnlineGame)
		{
			AmongUsClient.Instance.NetworkAddress = "18.218.20.65";
			AmongUsClient.Instance.MainMenuScene = "MMOnline";
			int num = InnerNetClient.GameNameToInt(GameIdText.text);
			if (num == -1)
			{
				StartCoroutine(Effects.Shake(GameIdText.transform));
				DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
				return;
			}
			AmongUsClient.Instance.GameId = num;
		}
		else
		{
			AmongUsClient.Instance.NetworkAddress = netAddress;
			AmongUsClient.Instance.GameId = 32;
			AmongUsClient.Instance.GameMode = GameModes.LocalGame;
			AmongUsClient.Instance.MainMenuScene = "MatchMaking";
		}
		StartCoroutine(JoinGame());
	}

	private IEnumerator JoinGame()
	{
		AmongUsClient.Instance.OnlineScene = "OnlineGame";
		IEnumerator joinWaiter = AmongUsClient.Instance.CoConnect(MatchMakerModes.Client);
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
		yield return joinWaiter;
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
		DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
		DestroyableSingleton<MMOnlineManager>.Instance.DisconnectPopup.Show();
	}

	public void SetGameName(string[] gameNameParts)
	{
		gameNameText.Text = string.Format("{0} ({1}/10)", gameNameParts[0], gameNameParts[2]);
	}

	public static bool IsValidName(string text)
	{
		if (text == null || text.Length == 0)
		{
			return false;
		}
		if (text.Equals("Enter Name", StringComparison.OrdinalIgnoreCase))
		{
			return false;
		}
		bool result = false;
		foreach (char c in text)
		{
			if (c != ' ')
			{
				result = true;
			}
		}
		return result;
	}

	public void StartIcon()
	{
		connectIcon.Play(connectClip);
	}

	public void StopIcon()
	{
		connectIcon.Stop();
		connectIcon.GetComponent<SpriteRenderer>().sprite = null;
	}
}
