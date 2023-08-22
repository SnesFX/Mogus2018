using System.Collections;
using InnerNet;
using PowerTools;
using UnityEngine;

public class MatchMakerGameButton : PoolableBehavior, IConnectButton
{
	public TextRenderer DescriptionText;

	public SpriteAnim connectIcon;

	public AnimationClip connectClip;

	public GameListing myListing;

	public void OnClick()
	{
		if (DestroyableSingleton<MatchMaker>.Instance.Connecting(this))
		{
			AmongUsClient.Instance.GameMode = GameModes.OnlineGame;
			AmongUsClient.Instance.OnlineScene = "OnlineGame";
			AmongUsClient.Instance.GameId = myListing.GameId;
			StartCoroutine(ConnectForFindGame());
		}
	}

	private IEnumerator ConnectForFindGame()
	{
		AmongUsClient.Instance.JoinGame();
		yield return EndGameManager.WaitWithTimeout(() => AmongUsClient.Instance.ClientId >= 0 || AmongUsClient.Instance.LastDisconnectReason != DisconnectReasons.ExitGame);
		DestroyableSingleton<MatchMaker>.Instance.NotConnecting();
		if (AmongUsClient.Instance.ClientId < 0)
		{
			DestroyableSingleton<FindAGameManager>.Instance.DisconnectPopup.Show();
			yield return AmongUsClient.Instance.CoConnect();
			if (AmongUsClient.Instance.LastDisconnectReason != 0)
			{
				DestroyableSingleton<MMOnlineManager>.Instance.DisconnectPopup.Show();
				yield break;
			}
			AmongUsClient.Instance.HostId = AmongUsClient.Instance.ClientId;
			DestroyableSingleton<FindAGameManager>.Instance.RefreshList();
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

	public void SetGame(GameListing gameListing)
	{
		myListing = gameListing;
		DescriptionText.Text = string.Format("{0} ({1}/10)", myListing.HostName, myListing.PlayerCount);
	}
}
