using UnityEngine;

public class CustomPlayerMenu : MonoBehaviour
{
	public GameObject ColorTab;

	public GameObject HatsTab;

	public GameObject GameTab;

	public SpriteRenderer ColorButton;

	public SpriteRenderer HatsButton;

	public SpriteRenderer GameButton;

	public Sprite NormalColor;

	public Sprite SelectedColor;

	public void OpenPlayer()
	{
		ColorTab.SetActive(true);
		ColorButton.sprite = SelectedColor;
		HatsTab.SetActive(false);
		HatsButton.sprite = NormalColor;
		GameTab.SetActive(false);
		GameButton.sprite = NormalColor;
	}

	public void OpenHats()
	{
		ColorTab.SetActive(false);
		ColorButton.sprite = NormalColor;
		HatsTab.SetActive(true);
		HatsButton.sprite = SelectedColor;
		GameTab.SetActive(false);
		GameButton.sprite = NormalColor;
	}

	public void OpenGame()
	{
		ColorTab.SetActive(false);
		ColorButton.sprite = NormalColor;
		HatsTab.SetActive(false);
		HatsButton.sprite = NormalColor;
		GameTab.SetActive(true);
		GameButton.sprite = SelectedColor;
	}

	public void Close(bool canMove)
	{
		PlayerControl.LocalPlayer.canMove = canMove;
		Object.Destroy(base.gameObject);
	}
}
