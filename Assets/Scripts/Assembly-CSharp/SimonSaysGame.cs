using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimonSaysGame : Minigame
{
	private Queue<int> operations = new Queue<int>();

	private const int FlashOp = 256;

	private const int AnimateOp = 128;

	private const int FailOp = 64;

	private Color gray = new Color32(141, 141, 141, byte.MaxValue);

	private Color blue = new Color32(68, 168, byte.MaxValue, byte.MaxValue);

	private Color red = new Color32(byte.MaxValue, 58, 0, byte.MaxValue);

	private Color green = Color.green;

	public SpriteRenderer[] LeftSide;

	public SpriteRenderer[] Buttons;

	public SpriteRenderer[] LeftLights;

	public SpriteRenderer[] RightLights;

	private List<int> idxes = new List<int>();

	private float flashTime = 0.25f;

	private float userButtonFlashTime = 0.175f;

	public AudioClip ButtonPressSound;

	public AudioClip FailSound;

	public override void Begin(PlayerTask task)
	{
		for (int i = 0; i < LeftSide.Length; i++)
		{
			LeftSide[i].color = Color.clear;
		}
		base.Begin(task);
		operations.Enqueue(128);
		StartCoroutine(CoRun());
	}

	public void HitButton(int bIdx)
	{
		if (MyNormTask.IsComplete || MyNormTask.taskStep >= idxes.Count)
		{
			return;
		}
		if (idxes[MyNormTask.taskStep] == bIdx)
		{
			MyNormTask.NextStep();
			SetLights(RightLights, MyNormTask.taskStep);
			if (MyNormTask.IsComplete)
			{
				SetLights(LeftLights, LeftLights.Length);
				for (int i = 0; i < Buttons.Length; i++)
				{
					SpriteRenderer spriteRenderer = Buttons[i];
					spriteRenderer.color = gray;
					StartCoroutine(FlashButton(-1, spriteRenderer, flashTime));
				}
				StartCoroutine(CoStartClose());
			}
			else
			{
				operations.Enqueue(0x100 | bIdx);
				if (MyNormTask.taskStep >= idxes.Count)
				{
					operations.Enqueue(128);
				}
			}
		}
		else
		{
			idxes.Clear();
			operations.Enqueue(64);
			operations.Enqueue(128);
		}
	}

	private IEnumerator CoRun()
	{
		while (true)
		{
			if (operations.Count > 0)
			{
				int op = operations.Dequeue();
				if (op.HasAnyBit(256))
				{
					int bIdx = op & -257;
					yield return FlashButton(bIdx, Buttons[bIdx], userButtonFlashTime);
				}
				else if (op.HasAnyBit(128))
				{
					yield return CoAnimateNewLeftSide();
				}
				else if (op.HasAnyBit(64))
				{
					if (Constants.ShouldPlaySfx())
					{
						SoundManager.Instance.PlaySound(FailSound, false);
					}
					SetAllColor(red);
					yield return new WaitForSeconds(flashTime);
					SetAllColor(Color.white);
					yield return new WaitForSeconds(flashTime);
					SetAllColor(red);
					yield return new WaitForSeconds(flashTime);
					SetAllColor(Color.white);
					yield return new WaitForSeconds(flashTime / 2f);
				}
			}
			else
			{
				yield return null;
			}
		}
	}

	private IEnumerator CoAnimateNewLeftSide()
	{
		SetLights(RightLights, 0);
		for (int j = 0; j < Buttons.Length; j++)
		{
			Buttons[j].color = gray;
		}
		yield return new WaitForSeconds(1f);
		idxes.Add(Buttons.RandomIdx());
		SetLights(LeftLights, idxes.Count);
		for (int i = 0; i < idxes.Count; i++)
		{
			int idx = idxes[i];
			yield return FlashButton(idx, LeftSide[idx], flashTime);
			yield return new WaitForSeconds(0.1f);
		}
		MyNormTask.taskStep = 0;
		for (int k = 0; k < Buttons.Length; k++)
		{
			Buttons[k].color = Color.white;
		}
	}

	private IEnumerator FlashButton(int id, SpriteRenderer butt, float flashTime)
	{
		if (id > -1 && Constants.ShouldPlaySfx())
		{
			AudioSource audioSource = SoundManager.Instance.PlaySound(ButtonPressSound, false);
			audioSource.pitch = Mathf.Lerp(0.5f, 1.5f, (float)id / 9f);
		}
		Color c = butt.color;
		butt.color = blue;
		yield return new WaitForSeconds(flashTime);
		butt.color = c;
	}

	private void SetLights(SpriteRenderer[] lights, int num)
	{
		for (int i = 0; i < lights.Length; i++)
		{
			if (i < num)
			{
				lights[i].color = green;
			}
			else
			{
				lights[i].color = gray;
			}
		}
	}

	private void SetAllColor(Color color)
	{
		for (int i = 0; i < Buttons.Length; i++)
		{
			Buttons[i].color = color;
		}
		for (int j = 0; j < RightLights.Length; j++)
		{
			RightLights[j].color = color;
		}
	}
}
