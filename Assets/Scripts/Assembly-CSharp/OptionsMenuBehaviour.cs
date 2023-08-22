using UnityEngine;

public class OptionsMenuBehaviour : MonoBehaviour
{
	public SpriteRenderer Background;

	public SpriteRenderer CloseButton;

	public SpriteRenderer JoystickButton;

	public SpriteRenderer TouchButton;

	public SlideBar JoystickSizeSlider;

	public FloatRange JoystickSizes = new FloatRange(0.5f, 1.5f);

	public SlideBar SoundSlider;

	public SlideBar MusicSlider;

	public ToggleButtonBehaviour SendNameButton;

	public ToggleButtonBehaviour SendTelemButton;

	public ToggleButtonBehaviour PersonalizedAdsButton;

	public bool Toggle = true;

	private bool couldMove;

	public TabGroup[] Tabs;

	public void OpenTabGroup(TabGroup selected)
	{
		selected.Open();
		for (int i = 0; i < Tabs.Length; i++)
		{
			TabGroup tabGroup = Tabs[i];
			if (!(tabGroup == selected))
			{
				tabGroup.Close();
			}
		}
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			Close();
		}
	}

	public void Open()
	{
		JoystickButton.transform.parent.GetComponentInChildren<TextRenderer>().Text = "Mouse";
		TouchButton.transform.parent.GetComponentInChildren<TextRenderer>().Text = "Keyboard+Mouse";
		JoystickSizeSlider.gameObject.SetActive(false);
		if (base.gameObject.activeSelf)
		{
			if (Toggle)
			{
				GetComponent<TransitionOpen>().Close();
			}
			return;
		}
		if ((bool)PlayerControl.LocalPlayer)
		{
			couldMove = PlayerControl.LocalPlayer.canMove;
			PlayerControl.LocalPlayer.canMove = false;
		}
		OpenTabGroup(Tabs[0]);
		UpdateButtons();
		base.gameObject.SetActive(true);
	}

	public void SetControlType(int i)
	{
		SaveManager.TouchConfig = i;
		UpdateButtons();
		if (DestroyableSingleton<HudManager>.InstanceExists)
		{
			DestroyableSingleton<HudManager>.Instance.SetTouchType(i);
		}
	}

	public void UpdateJoystickSize(SlideBar slider)
	{
		float joystickSize = JoystickSizes.Lerp(slider.Value);
		SaveManager.JoystickSize = joystickSize;
		if (DestroyableSingleton<HudManager>.InstanceExists)
		{
			DestroyableSingleton<HudManager>.Instance.SetJoystickSize(SaveManager.JoystickSize);
		}
	}

	public void ToggleSendTelemetry()
	{
		SaveManager.SendTelemetry = !SaveManager.SendTelemetry;
		UpdateButtons();
	}

	public void ToggleSendName()
	{
		SaveManager.SendName = !SaveManager.SendName;
		UpdateButtons();
	}

	public void UpdateSfxVolume(SlideBar button)
	{
		SaveManager.SfxVolume = button.Value;
		SoundManager.Instance.ChangeSfxVolume(button.Value);
	}

	public void UpdateMusicVolume(SlideBar button)
	{
		SaveManager.MusicVolume = button.Value;
		SoundManager.Instance.ChangeMusicVolume(button.Value);
	}

	public void TogglePersonalizedAd()
	{
		switch (SaveManager.ShowAdsScreen & (ShowAdsState)127)
		{
		case ShowAdsState.NonPersonalized:
			SaveManager.ShowAdsScreen = ShowAdsState.Accepted;
			break;
		default:
			SaveManager.ShowAdsScreen = (ShowAdsState)129;
			break;
		case ShowAdsState.Purchased:
			break;
		}
		UpdateButtons();
	}

	public void UpdateButtons()
	{
		if (SaveManager.TouchConfig == 0)
		{
			JoystickButton.color = new Color32(0, byte.MaxValue, 42, byte.MaxValue);
			TouchButton.color = Color.white;
			JoystickSizeSlider.enabled = true;
			JoystickSizeSlider.OnEnable();
		}
		else
		{
			JoystickButton.color = Color.white;
			TouchButton.color = new Color32(0, byte.MaxValue, 42, byte.MaxValue);
			JoystickSizeSlider.enabled = false;
			JoystickSizeSlider.OnDisable();
		}
		JoystickSizeSlider.Value = JoystickSizes.ReverseLerp(SaveManager.JoystickSize);
		SoundSlider.Value = SaveManager.SfxVolume;
		MusicSlider.Value = SaveManager.MusicVolume;
		SendNameButton.UpdateText(SaveManager.SendName);
		SendTelemButton.UpdateText(SaveManager.SendTelemetry);
		if ((bool)PersonalizedAdsButton)
		{
			if (SaveManager.ShowAdsScreen.HasFlag(ShowAdsState.Purchased) || SaveManager.BoughtNoAds)
			{
				PersonalizedAdsButton.transform.parent.gameObject.SetActive(false);
			}
			else
			{
				PersonalizedAdsButton.UpdateText(!SaveManager.ShowAdsScreen.HasFlag(ShowAdsState.NonPersonalized));
			}
		}
	}

	public void Close()
	{
		if ((bool)PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.canMove)
		{
			PlayerControl.LocalPlayer.canMove = couldMove;
		}
		base.gameObject.SetActive(false);
	}
}
