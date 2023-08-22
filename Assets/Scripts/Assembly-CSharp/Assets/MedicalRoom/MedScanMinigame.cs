using System.Collections;
using System.Text;
using UnityEngine;

namespace Assets.MedicalRoom
{
	internal class MedScanMinigame : Minigame
	{
		private static readonly string[] ColorNames = new string[10] { "Red", "Blue", "Green", "Pink", "Orange", "Yellow", "Black", "White", "Purple", "Brown" };

		private static readonly string[] BloodTypes = new string[8] { "O-", "A-", "B-", "AB-", "O+", "A+", "B+", "AB+" };

		public TextRenderer text;

		public TextRenderer charStats;

		public HorizontalGauge gauge;

		private MedScanSystem medscan;

		public float ScanDuration = 10f;

		public float ScanTimer;

		private bool requested;

		private string completeString;

		private Coroutine walking;

		private bool atPad;

		public override void Begin(PlayerTask task)
		{
			base.Begin(task);
			medscan = ShipStatus.Instance.Systems[SystemTypes.MedBay] as MedScanSystem;
			gauge.Value = 0f;
			base.transform.position = new Vector3(100f, 0f, 0f);
			int playerId = PlayerControl.LocalPlayer.PlayerId;
			int colorId = PlayerControl.LocalPlayer.ColorId;
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("ID: ");
			stringBuilder.Append(ColorNames[colorId].Substring(0, 3).ToUpperInvariant());
			stringBuilder.Append("P" + playerId);
			stringBuilder.Append(new string(' ', 8));
			stringBuilder.Append("HT: 3' 6\"");
			stringBuilder.Append(new string(' ', 8));
			stringBuilder.Append("WT: 92lb");
			stringBuilder.AppendLine();
			stringBuilder.Append("C: ");
			stringBuilder.Append(ColorNames[colorId].PadRight(17));
			stringBuilder.Append("BT: ");
			stringBuilder.Append(BloodTypes[playerId * 3 % BloodTypes.Length]);
			completeString = stringBuilder.ToString();
			charStats.Text = string.Empty;
			if (medscan.CurrentUser == -1)
			{
				walking = StartCoroutine(WalkToPad());
			}
			else
			{
				walking = StartCoroutine(WalkToOffset());
			}
		}

		private IEnumerator WalkToOffset()
		{
			PlayerPhysics phys = PlayerControl.LocalPlayer.GetComponent<PlayerPhysics>();
			Vector2 padCenter = ShipStatus.Instance.MedScanner.transform.position;
			Vector2 offset = Vector2.left.Rotate(PlayerControl.LocalPlayer.PlayerId * 36);
			padCenter += offset / 2f;
			Camera.main.GetComponent<FollowerCamera>().Locked = false;
			yield return phys.WalkPlayerTo(padCenter, 0.001f);
			yield return new WaitForSeconds(0.1f);
			Camera.main.GetComponent<FollowerCamera>().Locked = true;
			atPad = false;
			walking = null;
		}

		private IEnumerator WalkToPad()
		{
			PlayerPhysics phys = PlayerControl.LocalPlayer.GetComponent<PlayerPhysics>();
			Vector2 padCenter = ShipStatus.Instance.MedScanner.transform.position;
			padCenter.x += 0.14f;
			padCenter.y += 0.1f;
			Camera.main.GetComponent<FollowerCamera>().Locked = false;
			yield return phys.WalkPlayerTo(padCenter, 0.001f);
			yield return new WaitForSeconds(0.1f);
			Camera.main.GetComponent<FollowerCamera>().Locked = true;
			atPad = true;
			walking = null;
		}

		private void FixedUpdate()
		{
			if (MyNormTask.IsComplete)
			{
				return;
			}
			int playerIndex = GameData.Instance.GetPlayerIndex(PlayerControl.LocalPlayer);
			if (medscan.CurrentUser != playerIndex)
			{
				if (medscan.CurrentUser == -1)
				{
					text.Text = "Scan requested";
					if (requested)
					{
						return;
					}
					if (!atPad)
					{
						if (walking == null)
						{
							walking = StartCoroutine(WalkToPad());
						}
					}
					else
					{
						requested = true;
						ShipStatus.Instance.RpcRepairSystem(SystemTypes.MedBay, playerIndex | 0x80);
					}
				}
				else
				{
					requested = false;
					GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[medscan.CurrentUser];
					text.Text = "Waiting for " + playerInfo.Name;
					if (atPad && walking == null)
					{
						walking = StartCoroutine(WalkToOffset());
					}
				}
			}
			else
			{
				ScanTimer += Time.fixedDeltaTime;
				gauge.Value = ScanTimer / ScanDuration;
				float num = Mathf.Min(1f, ScanTimer / ScanDuration * 1.25f);
				int num2 = (int)(num * (float)completeString.Length);
				if (num2 > charStats.Text.Length)
				{
					charStats.Text = completeString.Substring(0, num2);
				}
				if (ScanTimer >= ScanDuration)
				{
					text.Text = "Scan complete";
					MyNormTask.NextStep();
					ShipStatus.Instance.RpcRepairSystem(SystemTypes.MedBay, playerIndex | 0x40);
					StartCoroutine(CoStartClose());
				}
				else
				{
					text.Text = "Scan complete in: " + (int)(ScanDuration - ScanTimer);
				}
			}
		}

		public override void Close(bool allowMovement)
		{
			int playerIndex = GameData.Instance.GetPlayerIndex(PlayerControl.LocalPlayer);
			ShipStatus.Instance.RpcRepairSystem(SystemTypes.MedBay, playerIndex | 0x40);
			base.Close(allowMovement);
		}
	}
}
