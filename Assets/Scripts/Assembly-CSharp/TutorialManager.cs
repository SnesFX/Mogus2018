using System.Collections;
using UnityEngine;

public class TutorialManager : DestroyableSingleton<TutorialManager>
{
	public PlayerControl PlayerPrefab;

	public Transform[] DummyLocations;

	public override void Start()
	{
		base.Start();
		StartCoroutine(RunTutorial());
	}

	private IEnumerator RunTutorial()
	{
		while (!ShipStatus.Instance)
		{
			yield return null;
		}
		ShipStatus.Instance.enabled = false;
		while (!PlayerControl.LocalPlayer)
		{
			yield return null;
		}
		PlayerControl.GameOptions = new GameOptionsData
		{
			NumImpostors = 0
		};
		PlayerControl.LocalPlayer.RpcSetInfected(new PlayerControl[0]);
		for (int i = 0; i < DummyLocations.Length; i++)
		{
			PlayerControl playerControl = Object.Instantiate(PlayerPrefab);
			playerControl.PlayerId = GameData.Instance.GetAvailableId();
			playerControl.transform.position = DummyLocations[i].position;
			playerControl.GetComponent<DummyBehaviour>().enabled = true;
			playerControl.SetName("Dummy " + (i + 1));
			AmongUsClient.Instance.Spawn(playerControl);
			playerControl.SetColor((byte)((i >= SaveManager.BodyColor) ? (i + 1) : i));
			GameData.Instance.AddPlayer(playerControl);
			GameData.Instance.RpcSetTasks(playerControl.PlayerId, new byte[0]);
		}
		ShipStatus.Instance.Begin();
	}
}
