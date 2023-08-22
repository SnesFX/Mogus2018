using Hazel;
using InnerNet;
using UnityEngine;

[DisallowMultipleComponent]
public class CustomNetworkTransform : InnerNetObject
{
	private enum RpcCalls
	{
		SnapTo = 0
	}

	private const float LocalMovementThreshold = 0.0001f;

	private const float LocalVelocityThreshold = 0.0001f;

	private const float MoveAheadRatio = 0.1f;

	private readonly FloatRange XRange = new FloatRange(-29f, 24f);

	private readonly FloatRange YRange = new FloatRange(-22f, 10f);

	[SerializeField]
	private float sendInterval = 0.1f;

	[SerializeField]
	private float snapThreshold = 5f;

	[SerializeField]
	private float interpolateMovement = 1f;

	private Rigidbody2D body;

	private Vector2 targetSyncPosition;

	private Vector2 targetSyncVelocity;

	private ushort lastSequenceId;

	private Vector2 prevPosSent;

	private Vector2 prevVelSent;

	private void Awake()
	{
		body = GetComponent<Rigidbody2D>();
		targetSyncPosition = (prevPosSent = base.transform.position);
		targetSyncVelocity = (prevVelSent = Vector2.zero);
	}

	public void OnEnable()
	{
		SetDirtyBit(1u);
	}

	public void Halt()
	{
		ushort minSid = (ushort)(lastSequenceId + 1);
		SnapTo(base.transform.position, minSid);
	}

	public void RpcSnapTo(Vector2 position)
	{
		ushort minSid = (ushort)(lastSequenceId + 50);
		if (AmongUsClient.Instance.AmClient)
		{
			SnapTo(position, minSid);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(NetId, 0);
		WriteVector2(position, messageWriter);
		messageWriter.Write(lastSequenceId);
		messageWriter.EndMessage();
	}

	public void SnapTo(Vector2 position)
	{
		ushort minSid = (ushort)(lastSequenceId + 50);
		SnapTo(position, minSid);
	}

	private void SnapTo(Vector2 position, ushort minSid)
	{
		if (SidGreaterThan(minSid, lastSequenceId))
		{
			lastSequenceId = minSid;
			Transform obj = base.transform;
			Vector2 position2 = position;
			body.position = position2;
			obj.position = (targetSyncPosition = position2);
			position2 = Vector2.zero;
			body.velocity = position2;
			targetSyncVelocity = position2;
			prevPosSent = position;
			prevVelSent = Vector2.zero;
		}
	}

	private void FixedUpdate()
	{
		if (base.AmOwner)
		{
			if (HasMoved())
			{
				SetDirtyBit(1u);
			}
			return;
		}
		if (interpolateMovement != 0f)
		{
			Vector2 vector = targetSyncPosition - body.position;
			if (vector.sqrMagnitude >= 0.0001f)
			{
				body.velocity = vector * interpolateMovement / sendInterval;
			}
			else
			{
				body.velocity = Vector2.zero;
			}
		}
		targetSyncPosition += targetSyncVelocity * Time.fixedDeltaTime * 0.1f;
	}

	private bool HasMoved()
	{
		float num = 0f;
		num = ((!(body != null)) ? Vector2.Distance(base.transform.position, prevPosSent) : Vector2.Distance(body.position, prevPosSent));
		if (num > 0.0001f)
		{
			return true;
		}
		if (body != null)
		{
			num = Vector2.Distance(body.velocity, prevVelSent);
		}
		if (num > 0.0001f)
		{
			return true;
		}
		return false;
	}

	public override void HandleRpc(byte callId, MessageReader reader)
	{
		if (base.isActiveAndEnabled && callId == 0)
		{
			Vector2 position = ReadVector2(reader);
			ushort minSid = reader.ReadUInt16();
			SnapTo(position, minSid);
		}
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		if (initialState)
		{
			writer.Write(lastSequenceId);
			WriteVector2(body.position, writer);
			WriteVector2(body.velocity, writer);
			return true;
		}
		if (DirtyBits == 0)
		{
			return false;
		}
		if (!base.isActiveAndEnabled)
		{
			return false;
		}
		lastSequenceId++;
		writer.Write(lastSequenceId);
		WriteVector2(body.position, writer);
		WriteVector2(body.velocity, writer);
		prevPosSent = body.position;
		prevVelSent = body.velocity;
		DirtyBits = 0u;
		return true;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
		if (initialState)
		{
			lastSequenceId = reader.ReadUInt16();
			Vector3 vector = ReadVector2(reader);
			base.transform.position = vector;
			targetSyncPosition = vector;
			targetSyncVelocity = ReadVector2(reader);
			return;
		}
		ushort newSid = reader.ReadUInt16();
		if (!SidGreaterThan(newSid, lastSequenceId))
		{
			return;
		}
		lastSequenceId = newSid;
		targetSyncPosition = ReadVector2(reader);
		targetSyncVelocity = ReadVector2(reader);
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		float num = Vector2.Distance(body.position, targetSyncPosition);
		if (num > snapThreshold)
		{
			if ((bool)body)
			{
				Debug.Log("Snapped");
				body.position = targetSyncPosition;
				body.velocity = targetSyncVelocity;
			}
			else
			{
				base.transform.position = targetSyncPosition;
			}
		}
		if (interpolateMovement == 0f && (bool)body)
		{
			body.position = targetSyncPosition;
		}
	}

	private static bool SidGreaterThan(ushort newSid, ushort prevSid)
	{
		ushort num = (ushort)(prevSid + 32767);
		if (prevSid < num)
		{
			return newSid > prevSid && newSid <= num;
		}
		return newSid > prevSid || newSid <= num;
	}

	private void WriteVector2(Vector2 vec, MessageWriter writer)
	{
		ushort value = (ushort)(XRange.ReverseLerp(vec.x) * 65535f);
		ushort value2 = (ushort)(YRange.ReverseLerp(vec.y) * 65535f);
		writer.Write(value);
		writer.Write(value2);
	}

	private Vector2 ReadVector2(MessageReader reader)
	{
		float v = (float)(int)reader.ReadUInt16() / 65535f;
		float v2 = (float)(int)reader.ReadUInt16() / 65535f;
		return new Vector2(XRange.Lerp(v), YRange.Lerp(v2));
	}
}
