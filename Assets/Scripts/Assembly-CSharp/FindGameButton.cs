using System.Collections;
using InnerNet;
using PowerTools;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FindGameButton : MonoBehaviour, IConnectButton
{
	public TextBox NameText;

	public SpriteAnim connectIcon;

	public AnimationClip connectClip;

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
		if (DestroyableSingleton<MatchMaker>.Instance.Connecting(this))
		{
			AmongUsClient.Instance.GameMode = GameModes.OnlineGame;
			AmongUsClient.Instance.MainMenuScene = "MMOnline";
			StartCoroutine(ConnectForFindGame());
		}
	}

	private IEnumerator ConnectForFindGame()
	{
		AmongUsClient.Instance.NetworkAddress = "18.218.20.65";
		AmongUsClient.Instance.OnlineScene = "OnlineGame";
		AmongUsClient.Instance.mode = MatchMakerModes.Client;
		yield return AmongUsClient.Instance.CoConnect();
		if (AmongUsClient.Instance.LastDisconnectReason != 0)
		{
			DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
			DestroyableSingleton<MMOnlineManager>.Instance.DisconnectPopup.Show();
		}
		else
		{
			AmongUsClient.Instance.HostId = AmongUsClient.Instance.ClientId;
			SceneManager.LoadScene("FindAGame");
		}
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
