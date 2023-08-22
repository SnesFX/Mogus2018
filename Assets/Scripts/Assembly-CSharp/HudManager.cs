using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class HudManager : DestroyableSingleton<HudManager>
{
	public MeetingHud MeetingPrefab;

	public KillButtonManager KillButton;

	public UseButtonManager UseButton;

	public ReportButtonManager ReportButton;

	public TextRenderer GameSettings;

	public GameObject TaskStuff;

	public DialogueBox Dialogue;

	public TextRenderer TaskText;

	public Transform TaskCompleteOverlay;

	private float taskDirtyTimer;

	public MeshRenderer ShadowQuad;

	public SpriteRenderer FullScreen;

	public SpriteRenderer MapButton;

	public MapBehaviour Map;

	public KillOverlay KillOverlay;

	public IVirtualJoystick joystick;

	public MonoBehaviour[] Joysticks;

	public DiscussBehaviour discussEmblem;

	public ShhhBehaviour shhhEmblem;

	public IntroCutscene IntroPrefab;

	public OptionsMenuBehaviour GameMenu;

	public NotificationPopper Notifier;

	public RoomTracker roomTracker;

	public AudioClip SabotageSound;

	public AudioClip TaskCompleteSound;

	public AudioClip TaskUpdateSound;

	private StringBuilder tasksString = new StringBuilder();

	public Coroutine reactorFlash { get; set; }

	public Coroutine oxyFlash { get; set; }

	public override void Start()
	{
		base.Start();
		SetTouchType(SaveManager.TouchConfig);
	}

	public void ShowTaskComplete()
	{
		StartCoroutine(CoTaskComplete());
	}

	private IEnumerator CoTaskComplete()
	{
		if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(TaskCompleteSound, false);
		}
		TaskCompleteOverlay.gameObject.SetActive(true);
		yield return Effects.Slide2D(TaskCompleteOverlay, new Vector2(0f, -8f), Vector2.zero, 0.25f);
		for (float time = 0f; time < 0.75f; time += Time.deltaTime)
		{
			yield return null;
		}
		yield return Effects.Slide2D(TaskCompleteOverlay, Vector2.zero, new Vector2(0f, 8f), 0.25f);
		TaskCompleteOverlay.gameObject.SetActive(false);
	}

	public void SetJoystickSize(float size)
	{
		if (joystick != null && joystick is VirtualJoystick)
		{
			VirtualJoystick virtualJoystick = (VirtualJoystick)joystick;
			virtualJoystick.transform.localScale = new Vector3(size, size, 1f);
			AspectPosition component = virtualJoystick.GetComponent<AspectPosition>();
			float num = Mathf.Lerp(0.65f, 1.1f, FloatRange.ReverseLerp(size, 0.5f, 1.5f));
			component.DistanceFromEdge = new Vector3(num, num, -10f);
			component.AdjustPosition();
		}
	}

	public void SetTouchType(int type)
	{
		if (joystick != null && !(joystick is KeyboardJoystick))
		{
			MonoBehaviour monoBehaviour = joystick as MonoBehaviour;
			Object.Destroy(monoBehaviour.gameObject);
		}
		MonoBehaviour monoBehaviour2 = Object.Instantiate(Joysticks[type + 1]);
		monoBehaviour2.transform.SetParent(base.transform, false);
		joystick = monoBehaviour2.GetComponent<IVirtualJoystick>();
	}

	public void SetHudActive(bool isActive)
	{
		DestroyableSingleton<HudManager>.Instance.UseButton.gameObject.SetActive(isActive);
		DestroyableSingleton<HudManager>.Instance.UseButton.Refresh();
		DestroyableSingleton<HudManager>.Instance.ReportButton.gameObject.SetActive(isActive);
		DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(isActive && PlayerControl.LocalPlayer.IsImpostor && !PlayerControl.LocalPlayer.IsDead);
		DestroyableSingleton<HudManager>.Instance.TaskText.transform.parent.gameObject.SetActive(isActive);
	}

	public void FixedUpdate()
	{
		Map.infectedOverlay.OOBUpdate(Time.fixedDeltaTime);
		taskDirtyTimer += Time.fixedDeltaTime;
		if (!(taskDirtyTimer > 0.25f))
		{
			return;
		}
		taskDirtyTimer = 0f;
		tasksString.Length = 0;
		if ((bool)PlayerControl.LocalPlayer)
		{
			PlayerTask playerTask;
			if (PlayerControl.LocalPlayer.myTasks.Count == 0)
			{
				tasksString.AppendLine("None");
			}
			else if ((bool)(playerTask = PlayerControl.LocalPlayer.myTasks.FirstOrDefault((PlayerTask p) => p.TaskType == TaskTypes.FixComms)))
			{
				playerTask.AppendTaskText(tasksString);
			}
			else
			{
				for (int i = 0; i < PlayerControl.LocalPlayer.myTasks.Count; i++)
				{
					PlayerTask playerTask2 = PlayerControl.LocalPlayer.myTasks[i];
					playerTask2.AppendTaskText(tasksString);
				}
			}
		}
		while (tasksString.Length > 0 && char.IsWhiteSpace(tasksString[tasksString.Length - 1]))
		{
			tasksString.Length--;
		}
		TaskText.Text = tasksString.ToString();
	}

	public IEnumerator ShowEmblem(bool shhh)
	{
		if (shhh)
		{
			shhhEmblem.gameObject.SetActive(true);
			yield return shhhEmblem.PlayAnimation();
			shhhEmblem.gameObject.SetActive(false);
		}
		else
		{
			discussEmblem.gameObject.SetActive(true);
			yield return discussEmblem.PlayAnimation();
			discussEmblem.gameObject.SetActive(false);
		}
	}

	public void StartReactorFlash()
	{
		if (reactorFlash == null)
		{
			reactorFlash = StartCoroutine(CoReactorFlash());
		}
	}

	public void StartOxyFlash()
	{
		if (oxyFlash == null)
		{
			oxyFlash = StartCoroutine(CoReactorFlash());
		}
	}

	public void ShowPopUp(string text)
	{
		Dialogue.Show(text);
	}

	public void StopReactorFlash()
	{
		if (reactorFlash != null)
		{
			StopCoroutine(reactorFlash);
			FullScreen.enabled = false;
			reactorFlash = null;
		}
	}

	public void StopOxyFlash()
	{
		if (oxyFlash != null)
		{
			StopCoroutine(oxyFlash);
			FullScreen.enabled = false;
			oxyFlash = null;
		}
	}

	public IEnumerator CoFadeFullScreen(Color source, Color target, float duration = 0.2f)
	{
		if (!FullScreen.enabled || !(FullScreen.color == target))
		{
			FullScreen.enabled = true;
			for (float t = 0f; t < duration; t += Time.deltaTime)
			{
				FullScreen.color = Color.Lerp(source, target, t / duration);
				yield return null;
			}
			FullScreen.color = target;
			if (target.a < 0.05f)
			{
				FullScreen.enabled = false;
			}
		}
	}

	private IEnumerator CoReactorFlash()
	{
		WaitForSeconds wait = new WaitForSeconds(1f);
		FullScreen.color = new Color(1f, 0f, 0f, 19f / 51f);
		while (true)
		{
			FullScreen.enabled = !FullScreen.enabled;
			SoundManager.Instance.PlaySound(SabotageSound, false);
			yield return wait;
		}
	}

	public IEnumerator CoShowIntro(List<PlayerControl> yourTeam)
	{
		DestroyableSingleton<HudManager>.Instance.FullScreen.transform.localPosition = new Vector3(0f, 0f, -250f);
		yield return DestroyableSingleton<HudManager>.Instance.ShowEmblem(true);
		IntroCutscene intro = Object.Instantiate(IntroPrefab, base.transform);
		yield return intro.CoBegin(yourTeam, PlayerControl.LocalPlayer.IsImpostor);
		yield return CoFadeFullScreen(Color.black, Color.clear);
		DestroyableSingleton<HudManager>.Instance.FullScreen.transform.localPosition = new Vector3(0f, 0f, -500f);
	}

	public void OpenMeetingRoom(PlayerControl reporter)
	{
		if (!MeetingHud.Instance)
		{
			Debug.Log(("Opening meeting room: " + reporter) ?? "No reporter");
			ShipStatus.Instance.RepairSystem(SystemTypes.Reactor, PlayerControl.LocalPlayer, 16);
			ShipStatus.Instance.RepairSystem(SystemTypes.LifeSupp, PlayerControl.LocalPlayer, 16);
			PlayerControl[] players = (from p in GameData.Instance.AllPlayers
				select p.Object into p
				where p
				select p).ToArray();
			MeetingHud.Instance = Object.Instantiate(MeetingPrefab);
			MeetingHud.Instance.ServerStart(players, reporter.PlayerId);
			AmongUsClient.Instance.Spawn(MeetingHud.Instance);
		}
	}
}
