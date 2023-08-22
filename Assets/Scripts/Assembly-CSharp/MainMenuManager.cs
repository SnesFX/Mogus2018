using System.Collections;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
	public DataCollectScreen DataPolicy;

	public AdDataCollectScreen AdsPolicy;

	public AnnouncementPopUp Announcement;

	public void Start()
	{
		Announcement.Init();
		StartCoroutine(RunStartUp());
	}

	private IEnumerator RunStartUp()
	{
		yield return DataPolicy.Show();
		yield return Announcement.Show();
	}

	private void Update()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			Application.Quit();
		}
	}
}
