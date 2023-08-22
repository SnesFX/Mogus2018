using UnityEngine;

public class DialogueBox : MonoBehaviour
{
	public TextRenderer target;

	public void Show(string dialogue)
	{
		target.Text = dialogue;
		Minigame minigame = Object.FindObjectOfType<Minigame>();
		if ((bool)minigame)
		{
			minigame.Close(false);
		}
		PlayerControl.LocalPlayer.canMove = false;
		PlayerControl.LocalPlayer.NetTransform.Halt();
		base.gameObject.SetActive(true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(false);
		PlayerControl.LocalPlayer.canMove = true;
		Camera.main.GetComponent<FollowerCamera>().Locked = false;
	}
}
