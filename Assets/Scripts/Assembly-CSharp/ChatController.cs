using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatController : MonoBehaviour
{
	public static ChatController Instance;

	public ObjectPoolBehavior chatBubPool;

	public Transform TypingArea;

	public SpriteRenderer TextBubble;

	public TextBox TextArea;

	public TextRenderer CharCount;

	public int MaxChat = 15;

	public Scroller scroller;

	public SpriteRenderer ChatNotifyDot;

	public TextRenderer SendRateMessage;

	private List<ChatBubble> bubbles = new List<ChatBubble>();

	public Vector3 SourcePos = new Vector3(0f, 0f, -10f);

	public Vector3 TargetPos = new Vector3(-0.35f, 0.02f, -10f);

	public float MaxChatSendRate = 5f;

	public DateTime LastSendTime;

	public AudioClip MessageSound;

	public bool controlCanMove;

	private bool animating;

	private Coroutine notificationRoutine;

	public BanMenu BanButton;

	public void Toggle()
	{
		if (animating)
		{
			return;
		}
		animating = true;
		if (!base.isActiveAndEnabled)
		{
			if (controlCanMove && !PlayerControl.LocalPlayer.canMove)
			{
				animating = false;
				return;
			}
			base.gameObject.SetActive(true);
			StartCoroutine(CoOpen());
		}
		else
		{
			StartCoroutine(CoClose());
		}
	}

	public void Start()
	{
		Instance = this;
	}

	public IEnumerator CoOpen()
	{
		if (controlCanMove)
		{
			PlayerControl.LocalPlayer.canMove = false;
			PlayerControl.LocalPlayer.NetTransform.Halt();
		}
		Vector3 scale = Vector3.one;
		BanButton.Hide();
		BanButton.ShowButton(true);
		float timer = 0f;
		while (timer < 0.15f)
		{
			timer += Time.deltaTime;
			float t = Mathf.SmoothStep(0f, 1f, timer / 0.15f);
			scale.y = (scale.x = Mathf.Lerp(0.1f, 1f, t));
			base.transform.localScale = scale;
			base.transform.localPosition = Vector3.Lerp(SourcePos, TargetPos, t);
			BanButton.transform.localPosition = new Vector3(0f, (0f - t) * 0.75f, -20f);
			yield return null;
		}
		BanButton.SetButtonActive(true);
		ChatNotifyDot.enabled = false;
		animating = false;
		if (!PlayerControl.LocalPlayer.IsDead)
		{
			TextArea.GiveFocus();
		}
	}

	public void UpdateCharCount()
	{
		Vector2 size = TextBubble.size;
		size.y = Math.Max(0.62f, TextArea.outputText.Height + 0.2f);
		TextBubble.size = size;
		Vector3 localPosition = TextBubble.transform.localPosition;
		localPosition.y = (0.62f - size.y) / 2f;
		TextBubble.transform.localPosition = localPosition;
		Vector3 localPosition2 = TypingArea.localPosition;
		localPosition2.y = -2.08f - localPosition.y * 2f;
		TypingArea.localPosition = localPosition2;
		int length = TextArea.text.Length;
		CharCount.Text = length + "/100";
		if (length < 75)
		{
			CharCount.Color = Color.black;
		}
		else if (length < 100)
		{
			CharCount.Color = new Color(1f, 1f, 0f, 1f);
		}
		else
		{
			CharCount.Color = Color.red;
		}
	}

	public IEnumerator CoClose()
	{
		if (controlCanMove)
		{
			PlayerControl.LocalPlayer.canMove = true;
		}
		BanButton.Hide();
		BanButton.SetButtonActive(false);
		Vector3 scale = Vector3.one;
		float timer = 0f;
		while (timer < 0.15f)
		{
			timer += Time.deltaTime;
			float t = 1f - Mathf.SmoothStep(0f, 1f, timer / 0.15f);
			scale.y = (scale.x = Mathf.Lerp(0.1f, 1f, t));
			base.transform.localScale = scale;
			base.transform.localPosition = Vector3.Lerp(SourcePos, TargetPos, t);
			BanButton.transform.localPosition = new Vector3(0f, (0f - t) * 0.75f, -20f);
			yield return null;
		}
		BanButton.ShowButton(false);
		animating = false;
		base.gameObject.SetActive(false);
	}

	private void Update()
	{
		if (SendRateMessage.isActiveAndEnabled)
		{
			float num = MaxChatSendRate - (float)(DateTime.Now - LastSendTime).TotalSeconds;
			if (num < 0f)
			{
				SendRateMessage.gameObject.SetActive(false);
			}
			else
			{
				SendRateMessage.Text = string.Format("Too fast. Wait {0} seconds", Mathf.CeilToInt(num));
			}
		}
	}

	public void SendChat()
	{
		float num = MaxChatSendRate - (float)(DateTime.Now - LastSendTime).TotalSeconds;
		if (num > 0f || !PlayerControl.LocalPlayer.RpcSendChat(TextArea.text))
		{
			if (num > 0f)
			{
				SendRateMessage.gameObject.SetActive(true);
				SendRateMessage.Text = string.Format("Too fast. Wait {0} seconds", Mathf.CeilToInt(num));
			}
		}
		else
		{
			TextArea.Clear();
		}
	}

	public void AddChat(PlayerControl player, string chatText)
	{
		if (bubbles.Count == MaxChat)
		{
			ChatBubble chatBubble = bubbles[0];
			bubbles.RemoveAt(0);
			chatBubble.OwnerPool.Reclaim(chatBubble);
		}
		bool flag = player == PlayerControl.LocalPlayer;
		ChatBubble chatBubble2 = chatBubPool.Get<ChatBubble>();
		chatBubble2.transform.SetParent(scroller.Inner);
		chatBubble2.transform.localScale = Vector3.one;
		if (flag)
		{
			chatBubble2.SetRight();
		}
		else
		{
			chatBubble2.SetLeft();
		}
		player.SetPlayerMaterialColors(chatBubble2.ChatFace);
		chatBubble2.NameText.Text = player.PlayerName;
		chatBubble2.NameText.RefreshMesh();
		chatBubble2.TextArea.Text = chatText;
		chatBubble2.TextArea.RefreshMesh();
		chatBubble2.Background.size = new Vector2(5.52f, 0.2f + chatBubble2.NameText.Height + chatBubble2.TextArea.Height);
		Vector3 localPosition = chatBubble2.Background.transform.localPosition;
		localPosition.y = chatBubble2.NameText.transform.localPosition.y - chatBubble2.Background.size.y / 2f + 0.05f;
		chatBubble2.Background.transform.localPosition = localPosition;
		bubbles.Add(chatBubble2);
		float num = 0f;
		for (int num2 = bubbles.Count - 1; num2 >= 0; num2--)
		{
			ChatBubble chatBubble3 = bubbles[num2];
			num += chatBubble3.Background.size.y;
			localPosition = chatBubble3.transform.localPosition;
			localPosition.y = -1.85f + num;
			chatBubble3.transform.localPosition = localPosition;
			num += 0.1f;
		}
		scroller.YBounds.min = Mathf.Min(0f, 0f - num + scroller.HitBox.bounds.size.y);
		if (!base.isActiveAndEnabled && notificationRoutine == null)
		{
			notificationRoutine = DestroyableSingleton<HudManager>.Instance.StartCoroutine(BounceDot());
		}
		if (player != PlayerControl.LocalPlayer)
		{
			AudioSource audioSource = SoundManager.Instance.PlaySound(MessageSound, false);
			audioSource.pitch = 0.5f + (float)(int)player.PlayerId / 10f;
		}
	}

	private IEnumerator BounceDot()
	{
		ChatNotifyDot.enabled = true;
		yield return Effects.Bounce(ChatNotifyDot.transform);
		notificationRoutine = null;
	}

	public void GiveFocus()
	{
		if (!PlayerControl.LocalPlayer.IsDead)
		{
			TextArea.GiveFocus();
		}
	}
}
