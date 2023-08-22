using InnerNet;
using UnityEngine;

public class DisconnectPopup : MonoBehaviour
{
	public TextRenderer TextArea;

	public void Show()
	{
		base.gameObject.SetActive(true);
		DoShow();
	}

	private void DoShow()
	{
		if (DestroyableSingleton<WaitForHostPopup>.InstanceExists)
		{
			DestroyableSingleton<WaitForHostPopup>.Instance.Hide();
		}
		if (!AmongUsClient.Instance)
		{
			base.gameObject.SetActive(false);
			return;
		}
		string text = InnerNetClient.IntToGameName(AmongUsClient.Instance.GameId);
		string arg = ((text == null) ? string.Empty : (" of " + text));
		switch (AmongUsClient.Instance.LastDisconnectReason)
		{
		case DisconnectReasons.Banned:
			TextArea.Text = string.Format("You were banned by the host{0}.\n\nYou cannot rejoin that game.", arg);
			break;
		case DisconnectReasons.Kicked:
			TextArea.Text = string.Format("You were kicked by the host{0}.\n\nYou can rejoin if the game hasn't started, but try to understand why you were kicked.", arg);
			break;
		case DisconnectReasons.GameFull:
			TextArea.Text = "The game you tried to join is full.\r\n\r\nCheck with the host to see if you can join next round.";
			break;
		case DisconnectReasons.GameStarted:
			TextArea.Text = "The game you tried to join already started.\r\n\r\nCheck with the host to see if you can join next round.";
			break;
		case DisconnectReasons.GameNotFound:
		case DisconnectReasons.IncorrectGame:
			TextArea.Text = "Could not find the game you're looking for.\r\n\r\nCheck the game code and try again.";
			break;
		case DisconnectReasons.ServerRequest:
			TextArea.Text = "You have been forcefully removed from the game.\r\n\r\nCheck with the host for any problems.";
			break;
		case DisconnectReasons.Error:
			TextArea.Text = "You disconnected from the host.\r\n\r\nIf this happens often, check your WiFi strength.";
			break;
		case DisconnectReasons.IncorrectVersion:
			TextArea.Text = "You are running an older version of the game.\r\n\r\nPlease update to play with others.";
			break;
		case DisconnectReasons.ExitGame:
		case DisconnectReasons.Destroy:
			base.gameObject.SetActive(false);
			break;
		}
		AmongUsClient.Instance.LastDisconnectReason = DisconnectReasons.ExitGame;
	}
}
