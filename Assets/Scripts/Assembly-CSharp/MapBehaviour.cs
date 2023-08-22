using UnityEngine;

public class MapBehaviour : MonoBehaviour
{
	public SpriteRenderer Background;

	public Sprite SabBackground;

	public Sprite OverlayBackground;

	public Sprite MapBackground;

	public SpriteRenderer HerePoint;

	public const float MapScale = 4.4f;

	public GameObject countOverlay;

	public InfectedOverlay infectedOverlay;

	public MapTaskOverlay taskOverlay;

	public void ShowInfectedMap()
	{
		if (base.gameObject.activeSelf)
		{
			Close();
		}
		else if (PlayerControl.LocalPlayer.canMove)
		{
			if (base.gameObject.activeSelf)
			{
				Close();
			}
			else if (PlayerControl.LocalPlayer.canMove)
			{
				PlayerControl.LocalPlayer.SetPlayerMaterialColors(HerePoint);
				base.gameObject.SetActive(true);
				infectedOverlay.gameObject.SetActive(true);
				Background.sprite = SabBackground;
				taskOverlay.Hide();
				DestroyableSingleton<HudManager>.Instance.SetHudActive(false);
			}
		}
	}

	public void ShowNormalMap()
	{
		if (base.gameObject.activeSelf)
		{
			Close();
		}
		else if (PlayerControl.LocalPlayer.canMove)
		{
			PlayerControl.LocalPlayer.SetPlayerMaterialColors(HerePoint);
			base.gameObject.SetActive(true);
			taskOverlay.Show();
			Background.sprite = MapBackground;
			DestroyableSingleton<HudManager>.Instance.SetHudActive(false);
		}
	}

	public void ShowCountOverlay()
	{
		PlayerControl.LocalPlayer.canMove = false;
		base.gameObject.SetActive(true);
		countOverlay.SetActive(true);
		taskOverlay.Hide();
		HerePoint.enabled = false;
		Background.sprite = OverlayBackground;
		DestroyableSingleton<HudManager>.Instance.SetHudActive(false);
	}

	public void FixedUpdate()
	{
		Vector3 position = PlayerControl.LocalPlayer.transform.position;
		position /= 4.4f;
		position.z = -1f;
		HerePoint.transform.localPosition = position;
	}

	public void Close()
	{
		PlayerControl.LocalPlayer.canMove = true;
		base.gameObject.SetActive(false);
		countOverlay.SetActive(false);
		infectedOverlay.gameObject.SetActive(false);
		taskOverlay.Hide();
		HerePoint.enabled = true;
		base.transform.localScale = Vector3.one;
		base.transform.localPosition = new Vector3(0f, 0f, -25f);
		DestroyableSingleton<HudManager>.Instance.SetHudActive(true);
	}
}
