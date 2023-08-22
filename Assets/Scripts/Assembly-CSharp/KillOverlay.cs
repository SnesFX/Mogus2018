using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillOverlay : MonoBehaviour
{
	public SpriteRenderer background;

	public GameObject flameParent;

	public OverlayKillAnimation[] KillAnims;

	public float FadeTime = 0.6f;

	public OverlayKillAnimation EmergencyOverlay;

	public OverlayKillAnimation ReportOverlay;

	private Queue<Func<IEnumerator>> queue = new Queue<Func<IEnumerator>>();

	private Coroutine showAll;

	private Coroutine showOne;

	public IEnumerator WaitForFinish()
	{
		while (showAll != null || queue.Count > 0)
		{
			yield return null;
		}
	}

	public void ShowOne(PlayerControl killer, PlayerControl victim)
	{
		queue.Enqueue(() => ShowOne(KillAnims.Random(), killer, victim, true));
		if (showAll == null)
		{
			showAll = StartCoroutine(ShowAll());
		}
	}

	public void ShowOne(OverlayKillAnimation killAnimPrefab, PlayerControl killer, PlayerControl victim)
	{
		queue.Enqueue(() => ShowOne(killAnimPrefab, killer, victim, false));
		if (showAll == null)
		{
			showAll = StartCoroutine(ShowAll());
		}
	}

	private IEnumerator ShowAll()
	{
		while (queue.Count > 0 || showOne != null)
		{
			if (showOne == null)
			{
				showOne = StartCoroutine(queue.Dequeue()());
			}
			yield return null;
		}
		showAll = null;
	}

	private IEnumerator ShowOne(OverlayKillAnimation killAnimPrefab, PlayerControl killer, PlayerControl victim, bool canMove)
	{
		OverlayKillAnimation anim = UnityEngine.Object.Instantiate(killAnimPrefab, base.transform);
		anim.gameObject.SetActive(false);
		anim.Begin(killer, victim);
		yield return CoShowOne(anim, canMove);
	}

	private IEnumerator CoShowOne(OverlayKillAnimation anim, bool canMove)
	{
		if (Constants.ShouldPlaySfx())
		{
			AudioSource audioSource = SoundManager.Instance.PlaySound(anim.Stinger, false);
			audioSource.volume = anim.StingerVolume;
		}
		WaitForSeconds wait = new WaitForSeconds(1f / 12f);
		PlayerControl.LocalPlayer.canMove = false;
		background.enabled = true;
		yield return wait;
		background.enabled = false;
		flameParent.SetActive(true);
		flameParent.transform.localScale = new Vector3(1f, 0.3f, 1f);
		flameParent.transform.localEulerAngles = new Vector3(0f, 0f, 25f);
		yield return wait;
		flameParent.transform.localScale = new Vector3(1f, 0.5f, 1f);
		flameParent.transform.localEulerAngles = new Vector3(0f, 0f, -15f);
		yield return wait;
		flameParent.transform.localScale = new Vector3(1f, 1f, 1f);
		flameParent.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
		anim.gameObject.SetActive(true);
		yield return anim.WaitForFinish();
		UnityEngine.Object.Destroy(anim.gameObject);
		yield return new WaitForLerp(1f / 6f, delegate(float t)
		{
			flameParent.transform.localScale = new Vector3(1f, 1f - t, 1f);
		});
		PlayerControl.LocalPlayer.canMove = canMove;
		flameParent.SetActive(false);
		showOne = null;
	}
}
