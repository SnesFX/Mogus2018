using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Effects
{
	private static HashSet<Transform> activeShakes = new HashSet<Transform>();

	public static IEnumerator Slide2D(Transform target, Vector2 source, Vector2 dest, float duration = 0.75f)
	{
		Vector3 temp = new Vector3
		{
			z = target.localPosition.z
		};
		for (float time = 0f; time < duration; time += Time.deltaTime)
		{
			float t = time / duration;
			temp.x = Mathf.SmoothStep(source.x, dest.x, t);
			temp.y = Mathf.SmoothStep(source.y, dest.y, t);
			target.localPosition = temp;
			yield return null;
		}
		temp.x = dest.x;
		temp.y = dest.y;
		target.localPosition = temp;
	}

	public static IEnumerator Bounce(Transform target, float duration = 0.3f, float height = 0.15f)
	{
		if (!target)
		{
			yield break;
		}
		Vector3 origin = target.localPosition;
		Vector3 temp = origin;
		for (float timer = 0f; timer < duration; timer += Time.deltaTime)
		{
			float perc = timer / duration;
			float decay = 1f - perc;
			temp.y = origin.y + height * Mathf.Abs(Mathf.Sin(perc * (float)Math.PI * 3f)) * decay;
			if (!target)
			{
				yield break;
			}
			target.localPosition = temp;
			yield return null;
		}
		if ((bool)target)
		{
			target.transform.localPosition = origin;
		}
	}

	public static IEnumerator Shake(Transform target, float duration = 0.75f, float halfWidth = 0.25f)
	{
		if (activeShakes.Add(target))
		{
			Vector3 origin = target.localPosition;
			for (float timer = 0f; timer < duration; timer += Time.deltaTime)
			{
				float perc = timer / duration;
				target.localPosition = origin + Vector3.right * (halfWidth * Mathf.Sin(perc * 30f) * (1f - perc));
				yield return null;
			}
			target.transform.localPosition = origin;
			activeShakes.Remove(target);
		}
	}

	public static IEnumerator Bloop(float delay, Transform target, float duration = 0.5f)
	{
		for (float t = 0f; t < delay; t += Time.deltaTime)
		{
			yield return null;
		}
		Vector3 temp = default(Vector3);
		for (float t2 = 0f; t2 < duration; t2 += Time.deltaTime)
		{
			float v = ElasticOut(t2, duration);
			temp.x = (temp.y = (temp.z = v));
			target.localScale = temp;
			yield return null;
		}
		temp.x = (temp.y = (temp.z = 1f));
		target.localScale = temp;
	}

	private static float ElasticOut(float time, float duration)
	{
		time /= duration;
		float num = time * time;
		float num2 = num * time;
		return 33f * num2 * num + -106f * num * num + 126f * num2 + -67f * num + 15f * time;
	}
}
