using UnityEngine;

public class NameTextBehaviour : MonoBehaviour
{
	public void Start()
	{
		TextBox component = GetComponent<TextBox>();
		component.text = SaveManager.PlayerName;
		component.outputText.Text = SaveManager.PlayerName;
	}
}
