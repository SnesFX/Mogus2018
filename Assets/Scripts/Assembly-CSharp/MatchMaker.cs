using InnerNet;
using UnityEngine;

public class MatchMaker : DestroyableSingleton<MatchMaker>
{
	public TextBox NameText;

	public TextBox GameIdText;

	public IConnectButton Connecter;

	public override void Start()
	{
		base.Start();
		if ((bool)GameIdText && (bool)AmongUsClient.Instance)
		{
			GameIdText.SetText(InnerNetClient.IntToGameName(AmongUsClient.Instance.GameId) ?? string.Empty);
		}
	}

	public bool Connecting(IConnectButton button)
	{
		if (Connecter == null)
		{
			Connecter = button;
			Connecter.StartIcon();
			return true;
		}
		StartCoroutine(Effects.Shake(((MonoBehaviour)Connecter).transform));
		return false;
	}

	public void NotConnecting()
	{
		if (Connecter != null)
		{
			Connecter.StopIcon();
			Connecter = null;
		}
	}
}
