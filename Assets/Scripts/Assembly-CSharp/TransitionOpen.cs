using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TransitionOpen : MonoBehaviour
{
	public float duration = 0.2f;

	public Button.ButtonClickedEvent OnClose = new Button.ButtonClickedEvent();

	public void OnEnable()
	{
		StartCoroutine(AnimateOpen());
	}

	public void Close()
	{
		StartCoroutine(AnimateClose());
	}

	private IEnumerator AnimateClose()
	{
		Vector3 vec = default(Vector3);
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			float p = t / duration;
			float s = Mathf.SmoothStep(1f, 0f, p);
			vec.Set(s, s, s);
			base.transform.localScale = vec;
			yield return null;
		}
		vec.Set(0f, 0f, 0f);
		base.transform.localScale = vec;
		OnClose.Invoke();
	}

	private IEnumerator AnimateOpen()
	{
		Vector3 vec = default(Vector3);
		for (float t = 0f; t < duration; t += Time.deltaTime)
		{
			float p = t / duration;
			float s = Mathf.SmoothStep(0f, 1f, p);
			vec.Set(s, s, s);
			base.transform.localScale = vec;
			yield return null;
		}
		vec.Set(1f, 1f, 1f);
		base.transform.localScale = vec;
	}
}
