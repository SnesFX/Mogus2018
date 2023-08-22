using UnityEngine;

internal class ChatBubble : PoolableBehavior
{
	public SpriteRenderer ChatFace;

	public TextRenderer NameText;

	public TextRenderer TextArea;

	public SpriteRenderer Background;

	public void SetLeft()
	{
		base.transform.localPosition = new Vector3(-3f, 0f, 0f);
		ChatFace.flipX = false;
		ChatFace.transform.localPosition = new Vector3(0f, 0.07f, 0f);
		NameText.transform.localPosition = new Vector3(0.5f, 0.34f, 0f);
		NameText.RightAligned = false;
		TextArea.transform.localPosition = new Vector3(0.5f, 0.09f, 0f);
		TextArea.RightAligned = false;
	}

	public void SetRight()
	{
		base.transform.localPosition = new Vector3(-2.35f, 0f, 0f);
		ChatFace.flipX = true;
		ChatFace.transform.localPosition = new Vector3(4.75f, 0.07f, 0f);
		NameText.transform.localPosition = new Vector3(4.35f, 0.34f, 0f);
		NameText.RightAligned = true;
		TextArea.transform.localPosition = new Vector3(4.35f, 0.09f, 0f);
		TextArea.RightAligned = true;
	}
}
