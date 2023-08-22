using System.Collections;
using UnityEngine;

public abstract class Minigame : MonoBehaviour
{
	protected enum CloseState
	{
		None = 0,
		Waiting = 1,
		Closing = 2
	}

	public static Minigame Instance;

	public const float Depth = -50f;

	public TransitionType TransType;

	protected PlayerTask MyTask;

	protected NormalPlayerTask MyNormTask;

	protected CloseState amClosing;

	public AudioClip OpenSound;

	public AudioClip CloseSound;

	public Console Console { get; set; }

	protected int ConsoleId
	{
		get
		{
			return Console ? Console.ConsoleId : 0;
		}
	}

	public virtual void Begin(PlayerTask task)
	{
		Instance = this;
		MyTask = task;
		MyNormTask = task as NormalPlayerTask;
		if ((bool)PlayerControl.LocalPlayer)
		{
			DestroyableSingleton<HudManager>.Instance.Map.Close();
			PlayerControl.LocalPlayer.canMove = false;
			PlayerControl.LocalPlayer.NetTransform.Halt();
			Camera.main.GetComponent<FollowerCamera>().Locked = true;
		}
		StartCoroutine(CoAnimateOpen());
	}

	protected IEnumerator CoStartClose(float duration = 0.75f)
	{
		if (amClosing == CloseState.None)
		{
			amClosing = CloseState.Waiting;
			yield return new WaitForSeconds(duration);
			Close(true);
		}
	}

	public virtual void Close(bool allowMovement)
	{
		if (amClosing != CloseState.Closing)
		{
			if ((bool)CloseSound && Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(CloseSound, false);
			}
			amClosing = CloseState.Closing;
			StartCoroutine(CoDestroySelf());
			if (allowMovement)
			{
				PlayerControl.LocalPlayer.canMove = true;
				Camera.main.GetComponent<FollowerCamera>().Locked = false;
			}
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	protected virtual IEnumerator CoAnimateOpen()
	{
		if ((bool)OpenSound && Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(OpenSound, false);
		}
		switch (TransType)
		{
		case TransitionType.SlideBottom:
		{
			for (float timer = 0f; timer < 0.25f; timer += Time.deltaTime)
			{
				float t = timer / 0.25f;
				base.transform.localPosition = new Vector3(0f, Mathf.SmoothStep(-8f, 0f, t), -50f);
				yield return null;
			}
			base.transform.localPosition = new Vector3(0f, 0f, -50f);
			break;
		}
		case TransitionType.Alpha:
		{
			SpriteRenderer[] rends = GetComponentsInChildren<SpriteRenderer>();
			for (float timer2 = 0f; timer2 < 0.25f; timer2 += Time.deltaTime)
			{
				float t2 = timer2 / 0.25f;
				for (int i = 0; i < rends.Length; i++)
				{
					rends[i].color = Color.Lerp(Palette.ClearWhite, Color.white, t2);
				}
				yield return null;
			}
			for (int j = 0; j < rends.Length; j++)
			{
				rends[j].color = Color.white;
			}
			break;
		}
		}
	}

	protected virtual IEnumerator CoDestroySelf()
	{
		switch (TransType)
		{
		case TransitionType.SlideBottom:
		{
			for (float timer = 0f; timer < 0.25f; timer += Time.deltaTime)
			{
				float t = timer / 0.25f;
				base.transform.localPosition = new Vector3(0f, Mathf.SmoothStep(0f, -8f, t), -50f);
				yield return null;
			}
			break;
		}
		case TransitionType.Alpha:
		{
			SpriteRenderer[] rends = GetComponentsInChildren<SpriteRenderer>();
			for (float timer2 = 0f; timer2 < 0.25f; timer2 += Time.deltaTime)
			{
				float t2 = timer2 / 0.25f;
				for (int i = 0; i < rends.Length; i++)
				{
					rends[i].color = Color.Lerp(Color.white, Palette.ClearWhite, t2);
				}
				yield return null;
			}
			break;
		}
		}
		Object.Destroy(base.gameObject);
	}
}
