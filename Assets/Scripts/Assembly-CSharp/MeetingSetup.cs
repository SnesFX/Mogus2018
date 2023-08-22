using UnityEngine;

public class MeetingSetup : MonoBehaviour
{
	public TextRenderer WhyHereText;

	public SpriteRenderer reporter;

	public TextRenderer WhoDiedText;

	public Transform bodyParent;

	public SpriteRenderer DeadBodyPrefab;

	internal void Setup(PlayerControl reporter, bool emergency, PlayerControl[] deadBodies)
	{
		reporter.SetPlayerMaterialColors(this.reporter);
		if (emergency)
		{
			WhyHereText.Text = reporter.PlayerName + " called an emergency meeting!";
		}
		else
		{
			WhyHereText.Text = reporter.PlayerName + " found a dead body!";
		}
		float num = Mathf.Min(4, deadBodies.Length);
		int num2 = deadBodies.Length / 4 + 1;
		WhoDiedText.Text = string.Format("Roll call shows {0} new dead bodies", deadBodies.Length);
		for (int i = 0; i < deadBodies.Length; i++)
		{
			int num3 = i % 4;
			int num4 = i / 4;
			float x = Mathf.Lerp(0f - num, num, ((float)num3 + 0.5f) / num) / 2f;
			float y = Mathf.Lerp(2f, -2f, ((float)num4 + 0.5f) / (float)num2) / 2f;
			SpriteRenderer spriteRenderer = Object.Instantiate(DeadBodyPrefab, bodyParent);
			spriteRenderer.transform.localPosition = new Vector3(x, y, 0f);
			spriteRenderer.transform.localScale = Vector3.one * 0.6f;
			deadBodies[i].SetPlayerMaterialColors(spriteRenderer);
		}
	}
}
