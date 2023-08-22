using UnityEngine;
using UnityEngine.SceneManagement;

public class HowToPlayController : MonoBehaviour
{
	public Transform DotParent;

	public SpriteRenderer leftButton;

	public SpriteRenderer rightButton;

	public SceneController PCMove;

	public SceneController[] Scenes;

	public int SceneNum;

	public void Start()
	{
		Scenes[2] = PCMove;
		PCMove.gameObject.SetActive(false);
		for (int i = 1; i < Scenes.Length; i++)
		{
			Scenes[i].gameObject.SetActive(false);
		}
		for (int j = 0; j < DotParent.childCount; j++)
		{
			Transform child = DotParent.GetChild(j);
			child.localScale = Vector3.one;
		}
		ChangeScene(0);
	}

	public void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			Close();
		}
	}

	public void NextScene()
	{
		ChangeScene(1);
	}

	public void PreviousScene()
	{
		ChangeScene(-1);
	}

	public void Close()
	{
		SceneManager.LoadScene("MainMenu");
	}

	private void ChangeScene(int del)
	{
		Scenes[SceneNum].gameObject.SetActive(false);
		DotParent.GetChild(SceneNum).localScale = Vector3.one;
		SceneNum = Mathf.Clamp(SceneNum + del, 0, Scenes.Length - 1);
		Scenes[SceneNum].gameObject.SetActive(true);
		DotParent.GetChild(SceneNum).localScale = new Vector3(1.5f, 1.5f, 1.5f);
		leftButton.gameObject.SetActive(SceneNum > 0);
		rightButton.gameObject.SetActive(SceneNum < Scenes.Length - 1);
	}
}
