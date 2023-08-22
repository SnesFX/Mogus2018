using UnityEngine;

public class GameOptionsMenu : MonoBehaviour
{
	private GameOptionsData cachedData;

	public GameObject ResetButton;

	private OptionBehaviour[] Children;

	public void Start()
	{
		Children = GetComponentsInChildren<OptionBehaviour>();
		for (int i = 0; i < Children.Length; i++)
		{
			OptionBehaviour optionBehaviour = Children[i];
			optionBehaviour.OnValueChanged = ValueChanged;
			if ((bool)AmongUsClient.Instance && !AmongUsClient.Instance.AmHost)
			{
				optionBehaviour.SetAsPlayer();
			}
		}
		cachedData = PlayerControl.GameOptions;
	}

	public void Update()
	{
		if (cachedData != PlayerControl.GameOptions)
		{
			cachedData = PlayerControl.GameOptions;
			RefreshChildren();
		}
		if ((bool)AmongUsClient.Instance && AmongUsClient.Instance.AmHost)
		{
			if (!ResetButton.activeSelf)
			{
				ResetButton.SetActive(true);
			}
		}
		else if (ResetButton.activeSelf)
		{
			ResetButton.SetActive(false);
		}
	}

	private void RefreshChildren()
	{
		for (int i = 0; i < Children.Length; i++)
		{
			OptionBehaviour optionBehaviour = Children[i];
			optionBehaviour.enabled = false;
			optionBehaviour.enabled = true;
		}
	}

	public void ValueChanged(OptionBehaviour option)
	{
		if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost)
		{
			return;
		}
		if (option.Title == "Recommended Settings")
		{
			if (cachedData.isDefaults)
			{
				cachedData.isDefaults = false;
			}
			else
			{
				cachedData.SetRecommendations(GameData.Instance.AllPlayers.Count);
			}
			RefreshChildren();
		}
		else
		{
			GameOptionsData gameOptions = PlayerControl.GameOptions;
			switch (option.Title)
			{
			case "Player Speed":
				gameOptions.PlayerSpeedMod = option.GetFloat();
				break;
			case "Crewmate Vision":
				gameOptions.CrewLightMod = option.GetFloat();
				break;
			case "Impostor Vision":
				gameOptions.ImpostorLightMod = option.GetFloat();
				break;
			case "Kill Cooldown":
				gameOptions.KillCooldown = option.GetFloat();
				break;
			case "Kill Distance":
				gameOptions.KillDistance = option.GetInt();
				break;
			case "# Common Tasks":
				gameOptions.NumCommonTasks = option.GetInt();
				break;
			case "# Long Tasks":
				gameOptions.NumLongTasks = option.GetInt();
				break;
			case "# Short Tasks":
				gameOptions.NumShortTasks = option.GetInt();
				break;
			case "# Impostors":
				gameOptions.NumImpostors = option.GetInt();
				break;
			case "# Emergency Meetings":
				gameOptions.NumEmergencyMeetings = option.GetInt();
				break;
			case "Discussion Time":
				gameOptions.DiscussionTime = option.GetInt();
				break;
			case "Voting Time":
				gameOptions.VotingTime = option.GetInt();
				break;
			default:
				Debug.Log("Ono, unrecognized setting: " + option.Title);
				break;
			}
			if (gameOptions.isDefaults)
			{
				gameOptions.isDefaults = false;
				RefreshChildren();
			}
		}
		if ((object)PlayerControl.LocalPlayer != null)
		{
			PlayerControl.LocalPlayer.RpcSyncSettings(PlayerControl.GameOptions);
		}
	}
}
