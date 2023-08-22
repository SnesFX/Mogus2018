using UnityEngine;

namespace PowerTools
{
	public class SpriteAnimNodes : MonoBehaviour
	{
		public static readonly int NUM_NODES = 10;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node0 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node1 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node2 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node3 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node4 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node5 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node6 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node7 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node8 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private Vector2 m_node9 = Vector2.zero;

		[SerializeField]
		[HideInInspector]
		private float m_ang0;

		[SerializeField]
		[HideInInspector]
		private float m_ang1;

		[SerializeField]
		[HideInInspector]
		private float m_ang2;

		[SerializeField]
		[HideInInspector]
		private float m_ang3;

		[SerializeField]
		[HideInInspector]
		private float m_ang4;

		[SerializeField]
		[HideInInspector]
		private float m_ang5;

		[SerializeField]
		[HideInInspector]
		private float m_ang6;

		[SerializeField]
		[HideInInspector]
		private float m_ang7;

		[SerializeField]
		[HideInInspector]
		private float m_ang8;

		[SerializeField]
		[HideInInspector]
		private float m_ang9;

		private SpriteRenderer m_spriteRenderer;

		public Vector3 GetPosition(int nodeId, bool ignoredPivot = false)
		{
			if (m_spriteRenderer == null)
			{
				m_spriteRenderer = GetComponent<SpriteRenderer>();
			}
			if (m_spriteRenderer == null || m_spriteRenderer.sprite == null)
			{
				return Vector2.zero;
			}
			Vector3 localPosition = GetLocalPosition(nodeId, ignoredPivot);
			localPosition = base.transform.rotation * localPosition;
			localPosition.Scale(base.transform.lossyScale);
			return localPosition + base.transform.position;
		}

		public Vector3 GetLocalPosition(int nodeId, bool ignoredPivot = false)
		{
			if (m_spriteRenderer == null)
			{
				m_spriteRenderer = GetComponent<SpriteRenderer>();
			}
			if (m_spriteRenderer == null || m_spriteRenderer.sprite == null)
			{
				return Vector2.zero;
			}
			Vector3 result = GetPositionRaw(nodeId);
			result.y = 0f - result.y;
			if (ignoredPivot)
			{
				result += (Vector3)(m_spriteRenderer.sprite.rect.size * 0.5f - m_spriteRenderer.sprite.pivot);
			}
			result *= 1f / m_spriteRenderer.sprite.pixelsPerUnit;
			if (m_spriteRenderer.flipX)
			{
				result.x = 0f - result.x;
			}
			if (m_spriteRenderer.flipY)
			{
				result.y = 0f - result.y;
			}
			return result;
		}

		public float GetAngle(int nodeId)
		{
			float angleRaw = GetAngleRaw(nodeId);
			if (m_spriteRenderer == null)
			{
				m_spriteRenderer = GetComponent<SpriteRenderer>();
			}
			if (m_spriteRenderer == null || m_spriteRenderer.sprite == null)
			{
				return 0f;
			}
			return angleRaw + base.transform.eulerAngles.z;
		}

		public Vector2 GetPositionRaw(int nodeId)
		{
			switch (nodeId)
			{
			case 0:
				return m_node0;
			case 1:
				return m_node1;
			case 2:
				return m_node2;
			case 3:
				return m_node3;
			case 4:
				return m_node4;
			case 5:
				return m_node5;
			case 6:
				return m_node6;
			case 7:
				return m_node7;
			case 8:
				return m_node8;
			case 9:
				return m_node9;
			default:
				return Vector2.zero;
			}
		}

		public float GetAngleRaw(int nodeId)
		{
			switch (nodeId)
			{
			case 0:
				return m_ang0;
			case 1:
				return m_ang1;
			case 2:
				return m_ang2;
			case 3:
				return m_ang3;
			case 4:
				return m_ang4;
			case 5:
				return m_ang5;
			case 6:
				return m_ang6;
			case 7:
				return m_ang7;
			case 8:
				return m_ang8;
			case 9:
				return m_ang9;
			default:
				return 0f;
			}
		}

		public void Reset()
		{
			m_node0 = Vector2.zero;
			m_node1 = Vector2.zero;
			m_node2 = Vector2.zero;
			m_node3 = Vector2.zero;
			m_node4 = Vector2.zero;
			m_node5 = Vector2.zero;
			m_node6 = Vector2.zero;
			m_node7 = Vector2.zero;
			m_node8 = Vector2.zero;
			m_node9 = Vector2.zero;
			m_ang0 = 0f;
			m_ang1 = 0f;
			m_ang2 = 0f;
			m_ang3 = 0f;
			m_ang4 = 0f;
			m_ang5 = 0f;
			m_ang6 = 0f;
			m_ang7 = 0f;
			m_ang8 = 0f;
			m_ang9 = 0f;
		}
	}
}
