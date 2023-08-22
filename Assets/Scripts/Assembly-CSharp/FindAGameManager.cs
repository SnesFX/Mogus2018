using System.Collections.Generic;
using InnerNet;
using PowerTools;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FindAGameManager : DestroyableSingleton<FindAGameManager>, IGameListHandler
{
	private class GameSorter : IComparer<GameListing>
	{
		public static readonly GameSorter Instance = new GameSorter();

		public int Compare(GameListing x, GameListing y)
		{
			return -x.PlayerCount.CompareTo(y.PlayerCount);
		}
	}

	public float RefreshTime = 5f;

	private float timer;

	public ObjectPoolBehavior buttonPool;

	public SpriteAnim RefreshSpinner;

	public AnimationClip RefreshAnim;

	public Transform TargetArea;

	public float ButtonStart = 1.75f;

	public float ButtonHeight = 0.6f;

	public DisconnectPopup DisconnectPopup;

	public const bool showPrivate = false;

	public override void Start()
	{
		if (!AmongUsClient.Instance)
		{
			AmongUsClient.Instance = Object.FindObjectOfType<AmongUsClient>();
			if (!AmongUsClient.Instance)
			{
				SceneManager.LoadScene("MMOnline");
				return;
			}
		}
		AmongUsClient.Instance.GameListHandlers.Add(this);
		AmongUsClient.Instance.RequestGameList(false);
		base.Start();
	}

	public void Update()
	{
		timer += Time.deltaTime;
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			ExitGame();
		}
	}

	public void RefreshList()
	{
		if (timer > RefreshTime)
		{
			timer = 0f;
			RefreshSpinner.Play(RefreshAnim);
			AmongUsClient.Instance.RequestGameList(false);
		}
	}

	public override void OnDestroy()
	{
		if ((bool)AmongUsClient.Instance)
		{
			AmongUsClient.Instance.GameListHandlers.Remove(this);
		}
		base.OnDestroy();
	}

	public void HandleList(int totalGames, List<GameListing> availableGames)
	{
		RefreshSpinner.Stop();
		RefreshSpinner.GetComponent<SpriteRenderer>().sprite = null;
		availableGames.Sort(GameSorter.Instance);
		while (buttonPool.activeChildren.Count > availableGames.Count)
		{
			PoolableBehavior poolableBehavior = buttonPool.activeChildren[buttonPool.activeChildren.Count - 1];
			poolableBehavior.OwnerPool.Reclaim(poolableBehavior);
		}
		while (buttonPool.activeChildren.Count < availableGames.Count)
		{
			buttonPool.Get<PoolableBehavior>().transform.SetParent(TargetArea);
		}
		Vector3 localPosition = default(Vector3);
		for (int i = 0; i < buttonPool.activeChildren.Count; i++)
		{
			MatchMakerGameButton matchMakerGameButton = (MatchMakerGameButton)buttonPool.activeChildren[i];
			matchMakerGameButton.SetGame(availableGames[i]);
			localPosition.y = ButtonStart - (float)i * ButtonHeight;
			matchMakerGameButton.transform.localPosition = localPosition;
		}
	}

	public void ExitGame()
	{
		AmongUsClient.Instance.ExitGame();
	}
}
