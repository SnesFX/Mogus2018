using System.Collections;
using Hazel;
using InnerNet;
using PowerTools;
using UnityEngine;

public class LobbyBehaviour : InnerNetObject
{
	public static LobbyBehaviour Instance;

	public AudioClip SpawnSound;

	public AnimationClip SpawnInClip;

	public Vector2[] SpawnPositions;

	public AudioClip DropShipSound;

	public ShipRoom[] AllRooms;

	public ChatController MyChat;

	private float timer;

	public void Start()
	{
		Instance = this;
		SoundManager.Instance.StopAllSound();
		AudioSource audioSource = SoundManager.Instance.PlaySound(DropShipSound, true);
		audioSource.pitch = 1.2f;
		if ((bool)Camera.main)
		{
			FollowerCamera component = Camera.main.GetComponent<FollowerCamera>();
			if ((bool)component)
			{
				component.shakeAmount = 0.03f;
				component.shakePeriod = 400f;
			}
		}
		ChatController.Instance = MyChat;
		MyChat.transform.parent.SetParent(DestroyableSingleton<HudManager>.Instance.transform, false);
	}

	public IEnumerator CoSpawnPlayer(PlayerControl target)
	{
		CustomNetworkTransform trans = target.NetTransform;
		PlayerPhysics phys = target.GetComponent<PlayerPhysics>();
		SpriteAnim anim = target.GetComponent<SpriteAnim>();
		Collider2D col = target.GetComponent<Collider2D>();
		Vector3 spawnPos = phys.Vec2ToPosition(SpawnPositions[target.PlayerId]);
		target.nameText.gameObject.SetActive(false);
		col.enabled = false;
		target.canMove = false;
		phys.enabled = false;
		trans.Halt();
		trans.enabled = false;
		bool amFlipped = target.PlayerId > 4;
		target.GetComponent<SpriteRenderer>().flipX = amFlipped;
		target.transform.position = spawnPos;
		AudioSource sfx = SoundManager.Instance.PlaySound(SpawnSound, false);
		sfx.volume = 0.8f;
		yield return new WaitForAnimationFinish(anim, SpawnInClip);
		phys.enabled = true;
		phys.Start();
		target.transform.position = spawnPos + new Vector3((!amFlipped) ? 0.3f : (-0.3f), -0.24f);
		phys.ResetAnim();
		Vector2 targetPos = (-spawnPos).normalized;
		yield return phys.WalkPlayerTo((Vector2)spawnPos + targetPos);
		col.enabled = true;
		target.canMove = true;
		trans.enabled = true;
		target.nameText.gameObject.SetActive(true);
		trans.Halt();
	}

	public void FixedUpdate()
	{
		timer += Time.deltaTime;
		if (!(timer < 0.25f))
		{
			timer = 0f;
			if (PlayerControl.GameOptions != null)
			{
				int numPlayers = ((!GameData.Instance) ? 10 : GameData.Instance.AllPlayers.Count);
				DestroyableSingleton<HudManager>.Instance.GameSettings.Text = PlayerControl.GameOptions.ToHudString(numPlayers);
				DestroyableSingleton<HudManager>.Instance.GameSettings.gameObject.SetActive(true);
			}
		}
	}

	public override void OnDestroy()
	{
		if ((bool)MyChat)
		{
			Object.Destroy(MyChat.transform.parent.gameObject);
		}
		if (DestroyableSingleton<HudManager>.InstanceExists)
		{
			DestroyableSingleton<HudManager>.Instance.GameSettings.gameObject.SetActive(false);
		}
		if ((bool)Camera.main)
		{
			FollowerCamera component = Camera.main.GetComponent<FollowerCamera>();
			if ((bool)component)
			{
				component.shakeAmount = 0.02f;
				component.shakePeriod = 0.3f;
			}
		}
		base.OnDestroy();
	}

	public override void HandleRpc(byte callId, MessageReader reader)
	{
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		return false;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
	}
}
