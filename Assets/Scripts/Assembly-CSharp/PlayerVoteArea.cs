using System.Collections;
using Hazel;
using UnityEngine;

public class PlayerVoteArea : MonoBehaviour
{
	public sbyte TargetPlayerId;

	public const byte DeadBit = 128;

	public const byte VotedBit = 64;

	public const byte ReportedBit = 32;

	public const byte VoteMask = 15;

	public GameObject Buttons;

	public SpriteRenderer PlayerIcon;

	public SpriteRenderer Flag;

	public SpriteRenderer Megaphone;

	public SpriteRenderer Overlay;

	public TextRenderer NameText;

	public bool isDead;

	public bool didVote;

	public bool didReport;

	public sbyte votedFor;

	public bool voteComplete;

	public bool resultsShowing;

	public MeetingHud Parent { get; set; }

	public void SetDead(bool isMe, bool didReport, bool isDead)
	{
		this.isDead = isDead;
		this.didReport = didReport;
		Megaphone.enabled = didReport;
		Overlay.gameObject.SetActive(false);
	}

	public void SetDisabled()
	{
		if (!isDead)
		{
			if ((bool)Overlay)
			{
				Overlay.gameObject.SetActive(true);
				Transform child = Overlay.transform.GetChild(0);
				child.gameObject.SetActive(false);
			}
			else
			{
				GetComponent<SpriteRenderer>().enabled = false;
			}
		}
	}

	public void SetEnabled()
	{
		if (!isDead)
		{
			if ((bool)Overlay)
			{
				Overlay.gameObject.SetActive(false);
			}
			else
			{
				GetComponent<SpriteRenderer>().enabled = true;
			}
		}
	}

	public IEnumerator CoAnimateOverlay()
	{
		Overlay.gameObject.SetActive(isDead);
		if (isDead)
		{
			Transform xMark = Overlay.transform.GetChild(0);
			Overlay.color = Palette.ClearWhite;
			xMark.localScale = Vector3.zero;
			float fadeDuration = 0.5f;
			for (float t3 = 0f; t3 < fadeDuration; t3 += Time.deltaTime)
			{
				Overlay.color = Color.Lerp(Palette.ClearWhite, Color.white, t3 / fadeDuration);
				yield return null;
			}
			Overlay.color = Color.white;
			float scaleDuration = 0.15f;
			for (float t2 = 0f; t2 < scaleDuration; t2 += Time.deltaTime)
			{
				float v3 = Mathf.Lerp(3f, 1f, t2 / scaleDuration);
				xMark.transform.localScale = new Vector3(v3, v3, v3);
				yield return null;
			}
			xMark.transform.localScale = Vector3.one;
		}
		else if (didReport)
		{
			float duration = 1f;
			for (float time = 0f; time < duration; time += Time.deltaTime)
			{
				float t = time / duration;
				float v2 = TriangleWave(t * 3f) * 2f - 1f;
				Megaphone.transform.localEulerAngles = new Vector3(0f, 0f, v2 * 30f);
				v2 = Mathf.Lerp(0.7f, 1.2f, TriangleWave(t * 2f));
				Megaphone.transform.localScale = new Vector3(v2, v2, v2);
				yield return null;
			}
			Megaphone.transform.localEulerAngles = Vector3.zero;
			Megaphone.transform.localScale = Vector3.one;
		}
	}

	private static float TriangleWave(float t)
	{
		t -= (float)(int)t;
		if (t < 0.5f)
		{
			return t * 2f;
		}
		return 1f - (t - 0.5f) * 2f;
	}

	internal void SetVote(sbyte suspectIdx)
	{
		didVote = true;
		votedFor = suspectIdx;
		Flag.enabled = true;
	}

	public void UnsetVote()
	{
		Flag.enabled = false;
		votedFor = 0;
		didVote = false;
	}

	public void ClearButtons()
	{
		Buttons.SetActive(false);
	}

	public void ClearForResults()
	{
		resultsShowing = true;
		Flag.enabled = false;
	}

	public void VoteForMe()
	{
		if (!voteComplete)
		{
			Parent.Confirm(TargetPlayerId);
		}
	}

	public void Select()
	{
		if (!PlayerControl.LocalPlayer.IsDead && !isDead && !voteComplete && Parent.Select(TargetPlayerId))
		{
			Buttons.SetActive(true);
		}
	}

	public void Cancel()
	{
		Buttons.SetActive(false);
	}

	public void Serialize(MessageWriter writer)
	{
		byte state = GetState();
		writer.Write(state);
	}

	public void Deserialize(MessageReader reader)
	{
		byte b = reader.ReadByte();
		votedFor = (sbyte)((b & 0xF) - 1);
		isDead = (b & 0x80) != 0;
		didVote = (b & 0x40) != 0;
		didReport = (b & 0x20) != 0;
		Flag.enabled = didVote && !resultsShowing;
		Overlay.gameObject.SetActive(isDead);
		Megaphone.enabled = didReport;
	}

	public byte GetState()
	{
		return (byte)(((uint)(votedFor + 1) & 0xFu) | (isDead ? 128u : 0u) | (didVote ? 64u : 0u) | (didReport ? 32u : 0u));
	}
}
