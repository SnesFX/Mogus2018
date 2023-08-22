using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EndGameManager : DestroyableSingleton<EndGameManager>
{
	public TextRenderer WinText;

	public MeshRenderer BackgroundBar;

	public MeshRenderer Foreground;

	public FloatRange ForegroundRadius;

	public SpriteRenderer FrontMost;

	public ButtonBehavior ProceedButton;

	public SpriteRenderer PlayerPrefab;

	public Sprite GhostSprite;

	public SpriteRenderer PlayAgainButton;

	public AudioClip DisconnectStinger;

	public AudioClip CrewStinger;

	public AudioClip ImpostorStinger;

	public float BaseY = -0.25f;

	private float stingerTime;

	public override void Start()
	{
		base.Start();
		if (TempData.showAd)
		{
			AdPlayer.RequestInterstitial();
		}
		SetEverythingUp();
		StartCoroutine(CoBegin());
	}

	private void SetEverythingUp()
	{
		if (TempData.EndReason == GameOverReason.ImpostorDisconnect)
		{
			WinText.Text = "Impostor\r\nDisconnected";
			SoundManager.Instance.PlaySound(DisconnectStinger, false);
			return;
		}
		if (TempData.EndReason == GameOverReason.HumansDisconnect)
		{
			WinText.Text = "Most Crewmates\r\nDisconnected";
			SoundManager.Instance.PlaySound(DisconnectStinger, false);
			return;
		}
		if (TempData.winners.Any((WinningPlayerData h) => h.IsYou))
		{
			WinText.Text = "Victory";
			BackgroundBar.material.SetColor("_Color", Palette.CrewmateBlue);
		}
		else
		{
			WinText.Text = "Defeat";
			WinText.Color = Color.red;
		}
		bool flag = TempData.DidHumansWin(TempData.EndReason);
		if (flag)
		{
			SoundManager.Instance.PlayDynamicSound("Stinger", CrewStinger, false, GetStingerVol);
		}
		else
		{
			SoundManager.Instance.PlayDynamicSound("Stinger", ImpostorStinger, false, GetStingerVol);
		}
		List<WinningPlayerData> list = TempData.winners.OrderBy((WinningPlayerData b) => b.IsYou ? (-1) : 0).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			WinningPlayerData winningPlayerData = list[i];
			SpriteRenderer spriteRenderer = UnityEngine.Object.Instantiate(PlayerPrefab, base.transform);
			spriteRenderer.flipX = i % 2 == 0;
			if (winningPlayerData.IsDead)
			{
				spriteRenderer.sprite = GhostSprite;
				spriteRenderer.flipX = !spriteRenderer.flipX;
			}
			float num = 1.25f;
			int num2 = ((i % 2 != 0) ? 1 : (-1));
			int num3 = (i + 1) / 2;
			float num4 = 1f - (float)num3 * 0.075f;
			float num5 = 1f - (float)num3 * 0.035f;
			float num6 = ((i != 0) ? (-1) : (-8));
			PlayerControl.SetPlayerMaterialColors(winningPlayerData.ColorId, spriteRenderer);
			spriteRenderer.transform.localPosition = new Vector3(0.8f * (float)num2 * (float)num3 * num5, BaseY - 0.25f + (float)num3 * 0.1f, num6 + (float)num3 * 0.01f) * num;
			Vector3 vector = new Vector3(num4, num4, num4) * num;
			spriteRenderer.transform.localScale = vector;
			TextRenderer componentInChildren = spriteRenderer.GetComponentInChildren<TextRenderer>();
			if (flag)
			{
				componentInChildren.gameObject.SetActive(false);
			}
			else
			{
				componentInChildren.Text = winningPlayerData.Name;
				componentInChildren.transform.localScale = vector.Inv();
			}
			SpriteRenderer component = spriteRenderer.transform.Find("HatSlot").GetComponent<SpriteRenderer>();
			Vector3 localPosition = component.transform.localPosition;
			if (winningPlayerData.IsDead)
			{
				component.flipX = spriteRenderer.flipX;
				component.color = new Color(1f, 1f, 1f, 0.5f);
				localPosition.y = 0.725f;
			}
			else
			{
				component.flipX = !spriteRenderer.flipX;
			}
			if (spriteRenderer.flipX)
			{
				localPosition.x = 0f - localPosition.x;
			}
			component.transform.localPosition = localPosition;
			PlayerControl.SetHatImage(winningPlayerData.HatId, component);
		}
	}

	private void GetStingerVol(AudioSource source, float dt)
	{
		stingerTime += dt * 0.75f;
		source.volume = Mathf.Clamp(1f / stingerTime, 0f, 1f);
	}

	public IEnumerator CoBegin()
	{
		Color c = WinText.Color;
		Color fade = Color.black;
		Color impColor = Color.white;
		Vector3 titlePos = WinText.transform.localPosition;
		float timer = 0f;
		while (timer < 3f)
		{
			timer += Time.deltaTime;
			float t = Mathf.Min(1f, timer / 3f);
			Foreground.material.SetFloat("_Rad", ForegroundRadius.ExpOutLerp(t * 2f));
			fade.a = Mathf.Lerp(1f, 0f, t * 3f);
			FrontMost.color = fade;
			c.a = Mathf.Clamp(FloatRange.ExpOutLerp(t, 0f, 1f), 0f, 1f);
			WinText.Color = c;
			titlePos.y = 2.7f - t * 0.3f;
			WinText.transform.localPosition = titlePos;
			yield return null;
		}
	}

	public void NextGame()
	{
		ProceedButton.gameObject.SetActive(false);
		PlayAgainButton.gameObject.SetActive(false);
		if (TempData.showAd && !SaveManager.BoughtNoAds)
		{
			TempData.showAd = false;
			TempData.playAgain = true;
			AdPlayer.ShowInterstitial(this);
		}
		else
		{
			StartCoroutine(CoJoinGame());
		}
	}

	public IEnumerator CoJoinGame()
	{
		AmongUsClient.Instance.JoinGame();
		yield return WaitWithTimeout(() => AmongUsClient.Instance.ClientId >= 0);
		if (AmongUsClient.Instance.ClientId < 0)
		{
			AmongUsClient.Instance.ExitGame();
		}
	}

	public void Exit()
	{
		ProceedButton.gameObject.SetActive(false);
		PlayAgainButton.gameObject.SetActive(false);
		if (TempData.showAd && !SaveManager.BoughtNoAds)
		{
			TempData.showAd = false;
			TempData.playAgain = false;
			AdPlayer.ShowInterstitial(this);
		}
		else
		{
			AmongUsClient.Instance.ExitGame();
		}
	}

	public static IEnumerator WaitWithTimeout(Func<bool> success)
	{
		for (float timer = 0f; timer < 5f; timer += Time.deltaTime)
		{
			if (success())
			{
				break;
			}
			yield return null;
		}
	}
}
