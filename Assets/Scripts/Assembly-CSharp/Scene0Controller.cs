using System;
using System.Collections;
using UnityEngine;

public class Scene0Controller : SceneController
{
	public float Duration = 3f;

	public SpriteRenderer[] ExtraBoys;

	public AnimationCurve PopInCurve;

	public AnimationCurve PopOutCurve;

	public float OutDuration = 0.2f;

	public void OnEnable()
	{
		StartCoroutine(Run());
	}

	public void OnDisable()
	{
		for (int i = 0; i < ExtraBoys.Length; i++)
		{
			ExtraBoys[i].enabled = false;
		}
	}

	private IEnumerator Run()
	{
		int lastBoy = 0;
		float start = Time.time;
		while (true)
		{
			float t = (Time.time - start) / Duration;
			float round = (Mathf.Cos((float)Math.PI * t + (float)Math.PI) + 1f) / 2f;
			int boy = Mathf.RoundToInt(round * (float)ExtraBoys.Length);
			if (lastBoy < boy)
			{
				StartCoroutine(PopIn(ExtraBoys[lastBoy]));
				lastBoy = boy;
			}
			else if (lastBoy > boy)
			{
				lastBoy = boy;
				StartCoroutine(PopOut(ExtraBoys[lastBoy]));
			}
			yield return null;
		}
	}

	private IEnumerator PopIn(SpriteRenderer boy)
	{
		boy.enabled = true;
		for (float timer = 0f; timer < 0.2f; timer += Time.deltaTime)
		{
			float s = PopInCurve.Evaluate(timer / 0.2f);
			boy.transform.localScale = new Vector3(s, s, s);
			yield return null;
		}
		boy.transform.localScale = Vector3.one;
	}

	private IEnumerator PopOut(SpriteRenderer boy)
	{
		boy.enabled = true;
		for (float timer = 0f; timer < OutDuration; timer += Time.deltaTime)
		{
			float s = PopOutCurve.Evaluate(timer / OutDuration);
			boy.transform.localScale = new Vector3(s, s, s);
			yield return null;
		}
		boy.transform.localScale = Vector3.one;
		boy.enabled = false;
	}
}
