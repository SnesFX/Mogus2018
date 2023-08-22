using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.CoreScripts
{
	public class Telemetry : DestroyableSingleton<Telemetry>
	{
		public const byte Version = 8;

		public const int HeaderSize = 39;

		public const byte InvalidOpCode = 0;

		public const byte StartGameOpCode = 1;

		public const byte EndGameOpCode = 2;

		public const byte PositionOpCode = 3;

		public const byte MurderOpCode = 4;

		public const byte UseOpCode = 5;

		public const byte DisconnectCode = 6;

		public const byte VotingStartCode = 7;

		public const byte VotingEndCode = 8;

		public const byte UpdateTaskOpCode = 9;

		private const int MaxSize = 1048576;

		private SequenceReaderWriter writer;

		private ushort submissionNum;

		public Vector2Range PosRange = new Vector2Range(new Vector2(-25f, -19f), new Vector2(20f, 6f));

		private bool gameStarted;

		public bool IsInitialized;

		public Guid CurrentGuid { get; private set; }

		public void Initialize()
		{
			Initialize(Guid.NewGuid());
		}

		public void Initialize(Guid gameGuid)
		{
			IsInitialized = true;
			writer = new SequenceReaderWriter(1048576);
			CurrentGuid = gameGuid;
			WriteHeader();
			gameStarted = false;
		}

		public void StartGame(bool sendName, bool isHost, bool isMurderer, byte playerCount, byte gameMode, float percImpostor)
		{
			writer.Write(1);
			if (sendName)
			{
				writer.Write(PlayerControl.LocalPlayer.PlayerName);
			}
			else
			{
				writer.Write(0);
			}
			writer.Write(PlayerControl.LocalPlayer.PlayerId);
			writer.Write(isHost);
			writer.Write(isMurderer);
			writer.Write(playerCount);
			WriteTime();
			writer.Write(gameMode);
			writer.Write(percImpostor);
			writer.Flush();
			gameStarted = true;
		}

		public void WriteMeetingStarted(bool isEmergency)
		{
			if (gameStarted && writer != null)
			{
				writer.Write(7);
				WriteTime();
				writer.Write(isEmergency);
				SubmitIfFull();
				writer.Flush();
			}
		}

		public void WriteMeetingEnded(byte[] results, float duration)
		{
			if (gameStarted && writer != null)
			{
				writer.Write(8);
				WriteTime();
				writer.Write((byte)results.Length);
				writer.Write(results);
				writer.Write(duration);
				SubmitIfFull();
				writer.Flush();
			}
		}

		public void WritePosition(byte playerNum, Vector2 worldPos)
		{
			if (gameStarted && writer != null)
			{
				worldPos.y -= 0.3636f;
				writer.Write(3);
				writer.Write(playerNum);
				WritePosition(worldPos);
				WriteTime();
				SubmitIfFull();
				writer.Flush();
			}
		}

		public void WriteMurder(byte sourcePlayerNum, byte targetPlayerNum, Vector3 worldPos)
		{
			if (gameStarted && writer != null)
			{
				worldPos.y -= 0.3636f;
				writer.Write(4);
				writer.Write(sourcePlayerNum);
				writer.Write(targetPlayerNum);
				WriteTime();
				WritePosition(worldPos);
				SubmitIfFull();
				writer.Flush();
			}
		}

		public void WriteUse(byte playerNum, byte taskNum, Vector3 worldPos)
		{
			if (gameStarted && writer != null)
			{
				worldPos.y -= 0.3636f;
				writer.Write(5);
				writer.Write(playerNum);
				writer.Write(taskNum);
				WriteTime();
				WritePosition(worldPos);
				SubmitIfFull();
				writer.Flush();
			}
		}

		public void WriteUpdateTask(byte playerNum, byte taskNum, byte taskStep)
		{
			if (gameStarted && writer != null)
			{
				writer.Write(9);
				writer.Write(playerNum);
				writer.Write(taskNum);
				writer.Write(taskStep);
				WriteTime();
				SubmitIfFull();
				writer.Flush();
			}
		}

		internal void WriteDisconnect(byte playerNum)
		{
			if (gameStarted && writer != null)
			{
				writer.Write(6);
				writer.Write(playerNum);
				WriteTime();
				SubmitIfFull();
				writer.Flush();
			}
		}

		public void EndGame(GameOverReason endReason)
		{
			if (gameStarted && writer != null)
			{
				writer.Write(2);
				writer.Write((byte)endReason);
				WriteTime();
				writer.Flush();
				AmongUsClient.Instance.StartCoroutine(Submit());
			}
		}

		private void SubmitIfFull()
		{
			if (writer.IsFull())
			{
				AmongUsClient.Instance.StartCoroutine(Submit());
			}
		}

		private IEnumerator Submit()
		{
			if (writer.Position > 39)
			{
				UnityWebRequest www = UnityWebRequest.Post("http://www.innersloth.com/amongus/telemetry.php", new Dictionary<string, string>
				{
					{
						"id",
						CurrentGuid.ToString("N")
					},
					{
						"data",
						writer.GetDataString()
					}
				});
				writer.Reset();
				submissionNum++;
				WriteHeader();
				yield return www.SendWebRequest();
				if (www.isNetworkError || www.isHttpError)
				{
					Debug.Log(www.error);
				}
				else
				{
					Debug.Log("Sent telemetry!");
				}
			}
		}

		private void WriteHeader()
		{
			writer.Write(8);
			writer.Write(VersionToUint(Application.version));
			writer.Write(CurrentGuid);
			writer.Write(submissionNum);
			writer.Write(PosRange.min.x);
			writer.Write(PosRange.min.y);
			writer.Write(PosRange.max.x);
			writer.Write(PosRange.max.y);
			writer.Flush();
		}

		private static uint VersionToUint(string version)
		{
			string[] array = version.Split('.');
			if (version.Length != 3)
			{
				return uint.MaxValue;
			}
			return uint.Parse(array[0]) * 366 + uint.Parse(array[1]) * 32 + uint.Parse(array[0]);
		}

		private void WriteTime()
		{
			writer.Write((uint)(Time.time * 100f));
		}

		private void WritePosition(Vector2 vec)
		{
			ushort input = (ushort)((vec.x - PosRange.min.x) / PosRange.Width * 65535f);
			ushort input2 = (ushort)((vec.y - PosRange.min.y) / PosRange.Height * 65535f);
			writer.Write(input);
			writer.Write(input2);
		}

		public override void OnDestroy()
		{
			if (writer != null)
			{
				writer.Dispose();
				writer = null;
			}
			base.OnDestroy();
		}
	}
}
