using System.Collections.Generic;
using InnerNet;
using UnityEngine;

public class BanMenu : MonoBehaviour
{
	public BanButton BanButtonPrefab;

	public SpriteRenderer Background;

	public SpriteRenderer BanButton;

	public SpriteRenderer KickButton;

	public GameObject ContentParent;

	public int selected = -1;

	[HideInInspector]
	public List<BanButton> allButtons = new List<BanButton>();

	public void ShowButton(bool show)
	{
		show = show && AmongUsClient.Instance.AmHost;
		GetComponent<SpriteRenderer>().enabled = show;
	}

	public void SetButtonActive(bool show)
	{
		show = show && AmongUsClient.Instance.AmHost;
		GetComponent<PassiveButton>().enabled = show;
	}

	public void Show()
	{
		if (ContentParent.activeSelf)
		{
			Hide();
			return;
		}
		selected = -1;
		KickButton.color = Color.gray;
		BanButton.color = Color.gray;
		ContentParent.SetActive(true);
		int num = 0;
		if ((bool)AmongUsClient.Instance)
		{
			List<ClientData> allClients = AmongUsClient.Instance.allClients;
			for (int i = 0; i < allClients.Count; i++)
			{
				ClientData clientData = allClients[i];
				if (clientData.Id != AmongUsClient.Instance.ClientId && (bool)clientData.Character)
				{
					PlayerControl character = clientData.Character;
					if (!string.IsNullOrWhiteSpace(character.PlayerName))
					{
						BanButton banButton = Object.Instantiate(BanButtonPrefab, ContentParent.transform);
						banButton.transform.localPosition = new Vector3(-0.2f, -0.15f - 0.4f * (float)num, -1f);
						banButton.Parent = this;
						banButton.NameText.Text = character.PlayerName;
						banButton.TargetClientId = clientData.Id;
						banButton.Unselect();
						allButtons.Add(banButton);
						num++;
					}
				}
			}
		}
		KickButton.transform.localPosition = new Vector3(-0.8f, -0.15f - 0.4f * (float)num - 0.1f, -1f);
		BanButton.transform.localPosition = new Vector3(0.3f, -0.15f - 0.4f * (float)num - 0.1f, -1f);
		float num2 = 0.3f + (float)(num + 1) * 0.4f;
		Background.size = new Vector2(3f, num2);
		Background.GetComponent<BoxCollider2D>().size = new Vector2(3f, num2);
		Background.transform.localPosition = new Vector3(0f, (0f - num2) / 2f + 0.15f, 0.1f);
	}

	public void Hide()
	{
		selected = -1;
		ContentParent.SetActive(false);
		for (int i = 0; i < allButtons.Count; i++)
		{
			Object.Destroy(allButtons[i].gameObject);
		}
		allButtons.Clear();
	}

	public void Select(int client)
	{
		selected = client;
		for (int i = 0; i < allButtons.Count; i++)
		{
			if (allButtons[i].TargetClientId != client)
			{
				allButtons[i].Unselect();
			}
		}
		KickButton.color = Color.white;
		BanButton.color = Color.white;
	}

	public void Kick(bool ban)
	{
		if (selected >= 0)
		{
			AmongUsClient.Instance.KickPlayer(selected, ban);
			Hide();
		}
	}
}
