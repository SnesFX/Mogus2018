using System.Collections;
using PowerTools;
using UnityEngine;

public class OverlayKillAnimation : MonoBehaviour
{
	public Renderer[] killerParts;

	public Renderer[] victimParts;

	private uint victimHat;

	public AudioClip Stinger;

	public AudioClip Sfx;

	public float StingerVolume = 0.6f;

	public void Begin(PlayerControl killer, PlayerControl victim)
	{
		if (killerParts != null)
		{
			for (int i = 0; i < killerParts.Length; i++)
			{
				Renderer renderer = killerParts[i];
				killer.SetPlayerMaterialColors(renderer);
				if (renderer.name == "HatSlot")
				{
					PlayerControl.SetHatImage(killer.HatId, (SpriteRenderer)renderer);
				}
			}
		}
		if (!victim || victimParts == null)
		{
			return;
		}
		victimHat = victim.HatId;
		for (int j = 0; j < victimParts.Length; j++)
		{
			Renderer renderer2 = victimParts[j];
			victim.SetPlayerMaterialColors(renderer2);
			if (renderer2.name == "HatSlot")
			{
				PlayerControl.SetHatImage(victim.HatId, (SpriteRenderer)renderer2);
			}
		}
	}

	public void SetHatFloor()
	{
		for (int i = 0; i < victimParts.Length; i++)
		{
			Renderer renderer = victimParts[i];
			if (renderer.name == "HatSlot")
			{
				((SpriteRenderer)renderer).sprite = DestroyableSingleton<HatManager>.Instance.GetHatById(victimHat).FloorImage;
			}
		}
	}

	public void PlayKillSound()
	{
		if (Constants.ShouldPlaySfx())
		{
			AudioSource audioSource = SoundManager.Instance.PlaySound(Sfx, false);
			audioSource.volume = 0.8f;
		}
	}

	public IEnumerator WaitForFinish()
	{
		SpriteAnim[] anims = GetComponentsInChildren<SpriteAnim>();
		if (anims.Length == 0)
		{
			yield return new WaitForSeconds(1f);
			yield break;
		}
		while (true)
		{
			bool found = false;
			for (int i = 0; i < anims.Length; i++)
			{
				if (anims[i].IsPlaying())
				{
					found = true;
					break;
				}
			}
			if (!found)
			{
				break;
			}
			yield return null;
		}
	}
}
