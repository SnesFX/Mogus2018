using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroCutscene : MonoBehaviour
{
	public TextRenderer Title;

	public TextRenderer ImpostorText;

	public SpriteRenderer PlayerPrefab;

	public MeshRenderer BackgroundBar;

	public MeshRenderer Foreground;

	public FloatRange ForegroundRadius;

	public SpriteRenderer FrontMost;

	public AudioClip IntroStinger;

	public float BaseY = -0.25f;

	public IEnumerator CoBegin(List<PlayerControl> yourTeam, bool isImpostor)
	{
		SoundManager.Instance.PlaySound(IntroStinger, false);
		if ((bool)PlayerControl.LocalPlayer)
		{
			PlayerControl.LocalPlayer.canMove = false;
		}
		if (!isImpostor)
		{
			BeginCrewmate(yourTeam);
		}
		else
		{
			BeginImpostor(yourTeam);
		}
		Color c = Title.Color;
		Color fade = Color.black;
		Color impColor = Color.white;
		Vector3 titlePos = Title.transform.localPosition;
		float timer2 = 0f;
		while (timer2 < 3f)
		{
			timer2 += Time.deltaTime;
			float t = Mathf.Min(1f, timer2 / 3f);
			Foreground.material.SetFloat("_Rad", ForegroundRadius.ExpOutLerp(t * 2f));
			fade.a = Mathf.Lerp(1f, 0f, t * 3f);
			FrontMost.color = fade;
			c.a = Mathf.Clamp(FloatRange.ExpOutLerp(t, 0f, 1f), 0f, 1f);
			Title.Color = c;
			impColor.a = Mathf.Lerp(0f, 1f, (t - 0.3f) * 3f);
			ImpostorText.Color = impColor;
			titlePos.y = 2.7f - t * 0.3f;
			Title.transform.localPosition = titlePos;
			yield return null;
		}
		timer2 = 0f;
		while (timer2 < 1f)
		{
			timer2 += Time.deltaTime;
			float t2 = timer2 / 1f;
			fade.a = Mathf.Lerp(0f, 1f, t2 * 3f);
			FrontMost.color = fade;
			yield return null;
		}
		if ((bool)PlayerControl.LocalPlayer)
		{
			PlayerControl.LocalPlayer.canMove = true;
		}
		Object.Destroy(base.gameObject);
	}

	private void BeginCrewmate(List<PlayerControl> yourTeam)
	{
		Vector3 position = BackgroundBar.transform.position;
		position.y -= 0.25f;
		BackgroundBar.transform.position = position;
		if (PlayerControl.GameOptions.NumImpostors == 1)
		{
			ImpostorText.Text = string.Format("There is [FF1919FF]{0} Impostor[] among us", PlayerControl.GameOptions.NumImpostors);
		}
		else
		{
			ImpostorText.Text = string.Format("There are [FF1919FF]{0} Impostors[] among us", PlayerControl.GameOptions.NumImpostors);
		}
		BackgroundBar.material.SetColor("_Color", Palette.CrewmateBlue);
		Title.Text = "Crewmate";
		Title.Color = Palette.CrewmateBlue;
		for (int i = 0; i < yourTeam.Count; i++)
		{
			PlayerControl playerControl = yourTeam[i];
			if ((bool)playerControl)
			{
				SpriteRenderer spriteRenderer = Object.Instantiate(PlayerPrefab, base.transform);
				spriteRenderer.name = playerControl.name + "Dummy";
				spriteRenderer.flipX = i % 2 == 0;
				float num = 1.5f;
				int num2 = ((i % 2 != 0) ? 1 : (-1));
				int num3 = (i + 1) / 2;
				float num4 = ((i != 0) ? 1f : 1.2f);
				float num5 = num4 - (float)num3 * 0.12f;
				float num6 = 1f - (float)num3 * 0.08f;
				float num7 = ((i != 0) ? (-1) : (-8));
				PlayerControl.SetPlayerMaterialColors(playerControl.ColorId, spriteRenderer);
				spriteRenderer.transform.localPosition = new Vector3(0.8f * (float)num2 * (float)num3 * num6, BaseY - 0.25f + (float)num3 * 0.1f, num7 + (float)num3 * 0.01f) * num;
				Vector3 localScale = new Vector3(num5, num5, num5) * num;
				spriteRenderer.transform.localScale = localScale;
				TextRenderer componentInChildren = spriteRenderer.GetComponentInChildren<TextRenderer>();
				componentInChildren.gameObject.SetActive(false);
				SpriteRenderer component = spriteRenderer.transform.Find("HatSlot").GetComponent<SpriteRenderer>();
				component.flipX = !spriteRenderer.flipX;
				if (spriteRenderer.flipX)
				{
					Vector3 localPosition = component.transform.localPosition;
					localPosition.x = 0f - localPosition.x;
					component.transform.localPosition = localPosition;
				}
				PlayerControl.SetHatImage(playerControl.HatId, component);
			}
		}
	}

	private void BeginImpostor(List<PlayerControl> yourTeam)
	{
		ImpostorText.gameObject.SetActive(false);
		Title.Text = "Impostor";
		Title.Color = Palette.ImpostorRed;
		for (int i = 0; i < yourTeam.Count; i++)
		{
			PlayerControl playerControl = yourTeam[i];
			SpriteRenderer spriteRenderer = Object.Instantiate(PlayerPrefab, base.transform);
			spriteRenderer.flipX = i % 2 == 1;
			float num = 1.5f;
			int num2 = ((i % 2 != 0) ? 1 : (-1));
			int num3 = (i + 1) / 2;
			float num4 = 1f - (float)num3 * 0.075f;
			float num5 = 1f - (float)num3 * 0.035f;
			PlayerControl.SetPlayerMaterialColors(playerControl.ColorId, spriteRenderer);
			float num6 = ((i != 0) ? (-1) : (-8));
			spriteRenderer.transform.localPosition = new Vector3((float)(num2 * num3) * num5, BaseY + (float)num3 * 0.15f, num6 + (float)num3 * 0.01f) * num;
			Vector3 vector = new Vector3(num4, num4, num4) * num;
			spriteRenderer.transform.localScale = vector;
			TextRenderer componentInChildren = spriteRenderer.GetComponentInChildren<TextRenderer>();
			componentInChildren.Text = playerControl.PlayerName;
			componentInChildren.transform.localScale = vector.Inv();
			SpriteRenderer component = spriteRenderer.transform.Find("HatSlot").GetComponent<SpriteRenderer>();
			component.flipX = !spriteRenderer.flipX;
			if (spriteRenderer.flipX)
			{
				Vector3 localPosition = component.transform.localPosition;
				localPosition.x = 0f - localPosition.x;
				component.transform.localPosition = localPosition;
			}
			PlayerControl.SetHatImage(playerControl.HatId, component);
		}
	}
}
