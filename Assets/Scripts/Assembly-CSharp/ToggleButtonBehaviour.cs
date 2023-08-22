using UnityEngine;

public class ToggleButtonBehaviour : MonoBehaviour
{
	public string BaseText;

	public TextRenderer Text;

	public SpriteRenderer Background;

	public ButtonRolloverHandler Rollover;

	public void UpdateText(bool on)
	{
		Color color = ((!on) ? Color.white : new Color(0f, 1f, 14f / 85f, 1f));
		Background.color = color;
		Text.Text = BaseText + ": " + ((!on) ? "Off" : "On");
		if ((bool)Rollover)
		{
			Rollover.OutColor = color;
		}
	}
}
