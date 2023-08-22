using System;
using System.Collections.Generic;
using UnityEngine;

public class LightSource : MonoBehaviour
{
	private class VertInfo
	{
		public float Angle;

		public Vector3 Position;

		internal void Complete(float x, float y)
		{
			Position.x = x;
			Position.y = y;
			Angle = pseudoAngle(y, x);
		}

		internal void Complete(Vector2 point)
		{
			Position.x = point.x;
			Position.y = point.y;
			Angle = pseudoAngle(point.y, point.x);
		}
	}

	private class AngleComparer : IComparer<VertInfo>
	{
		public static readonly AngleComparer Instance = new AngleComparer();

		public int Compare(VertInfo x, VertInfo y)
		{
			return (x.Angle > y.Angle) ? 1 : ((x.Angle < y.Angle) ? (-1) : 0);
		}
	}

	private class HitDepthComparer : IComparer<RaycastHit2D>
	{
		public static readonly HitDepthComparer Instance = new HitDepthComparer();

		public int Compare(RaycastHit2D x, RaycastHit2D y)
		{
			return (x.fraction > y.fraction) ? 1 : (-1);
		}
	}

	public static Dictionary<int, NoShadowBehaviour> NoShadows = new Dictionary<int, NoShadowBehaviour>();

	[HideInInspector]
	private GameObject child;

	[HideInInspector]
	private Vector2[] requiredDels;

	[HideInInspector]
	private Mesh myMesh;

	public int MinRays = 24;

	public float LightRadius = 3f;

	public Material Material;

	[HideInInspector]
	private List<VertInfo> verts = new List<VertInfo>(256);

	[HideInInspector]
	private int vertCount;

	private RaycastHit2D[] buffer = new RaycastHit2D[25];

	private Collider2D[] hits = new Collider2D[40];

	private ContactFilter2D filter = default(ContactFilter2D);

	private Vector3[] vec;

	private Vector2[] uvs;

	public float tol = 0.05f;

	private List<RaycastHit2D> lightHits = new List<RaycastHit2D>();

	private void Start()
	{
		filter.useTriggers = true;
		filter.layerMask = Constants.ShadowMask;
		filter.useLayerMask = true;
		requiredDels = new Vector2[MinRays];
		for (int i = 0; i < requiredDels.Length; i++)
		{
			requiredDels[i] = Vector2.left.Rotate((float)i / (float)requiredDels.Length * 360f);
		}
		myMesh = new Mesh();
		myMesh.MarkDynamic();
		myMesh.name = "ShadowMesh";
		GameObject gameObject = new GameObject("LightChild");
		gameObject.layer = 10;
		MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
		meshFilter.mesh = myMesh;
		MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
		Material = new Material(Material);
		meshRenderer.sharedMaterial = Material;
		child = gameObject;
	}

	private void Update()
	{
		vertCount = 0;
		child.transform.position = base.transform.position - new Vector3(0f, 0f, 7f);
		Vector2 myPos = child.transform.position;
		Material.SetFloat("_LightRadius", LightRadius);
		Vector2 vector = default(Vector2);
		Vector2 vector2 = default(Vector2);
		Vector2 del = default(Vector2);
		int num = Physics2D.OverlapCircleNonAlloc(myPos, LightRadius, hits, Constants.ShadowMask);
		for (int i = 0; i < num; i++)
		{
			Collider2D collider2D = hits[i];
			if (!collider2D.isTrigger)
			{
				EdgeCollider2D edgeCollider2D = collider2D as EdgeCollider2D;
				if ((bool)edgeCollider2D)
				{
					Vector2[] points = edgeCollider2D.points;
					for (int j = 0; j < points.Length; j++)
					{
						Vector2 vector3 = edgeCollider2D.transform.TransformPoint(points[j]);
						vector.x = vector3.x - myPos.x;
						vector.y = vector3.y - myPos.y;
						float num2 = length(vector.x, vector.x);
						vector2.x = (0f - vector.y) / num2 * tol;
						vector2.y = vector.x / num2 * tol;
						del.x = vector.x + vector2.x;
						del.y = vector.y + vector2.y;
						CreateVert(ref myPos, ref del);
						del.x = vector.x - vector2.x;
						del.y = vector.y - vector2.y;
						CreateVert(ref myPos, ref del);
					}
					continue;
				}
				PolygonCollider2D polygonCollider2D = collider2D as PolygonCollider2D;
				if ((bool)polygonCollider2D)
				{
					Vector2[] points2 = polygonCollider2D.points;
					for (int k = 0; k < points2.Length; k++)
					{
						Vector2 vector4 = polygonCollider2D.transform.TransformPoint(points2[k]);
						vector.x = vector4.x - myPos.x;
						vector.y = vector4.y - myPos.y;
						float num3 = length(vector.x, vector.x);
						vector2.x = (0f - vector.y) / num3 * tol;
						vector2.y = vector.x / num3 * tol;
						del.x = vector.x + vector2.x;
						del.y = vector.y + vector2.y;
						CreateVert(ref myPos, ref del);
						del.x = vector.x - vector2.x;
						del.y = vector.y - vector2.y;
						CreateVert(ref myPos, ref del);
					}
					continue;
				}
			}
			BoxCollider2D boxCollider2D = collider2D as BoxCollider2D;
			if ((bool)boxCollider2D)
			{
				Vector2 del2 = boxCollider2D.transform.TransformPoint(boxCollider2D.offset);
				Vector2 size = boxCollider2D.size;
				CreateVert(ref myPos, ref del2);
				del.x = del2.x + size.x;
				del.y = del2.y + size.y;
				CreateVert(ref myPos, ref del);
				del.y = del2.y - size.y;
				CreateVert(ref myPos, ref del);
				del.x = del2.x - size.x;
				CreateVert(ref myPos, ref del);
				del.y = del2.y + size.y;
				CreateVert(ref myPos, ref del);
			}
		}
		float num4 = LightRadius * 1.05f;
		for (int l = 0; l < requiredDels.Length; l++)
		{
			Vector2 del3 = num4 * requiredDels[l];
			CreateVert(ref myPos, ref del3);
		}
		verts.Sort(0, vertCount, AngleComparer.Instance);
		myMesh.Clear();
		if (vec == null || vec.Length < vertCount + 1)
		{
			vec = new Vector3[vertCount + 1];
			uvs = new Vector2[vec.Length];
		}
		vec[0] = Vector3.zero;
		for (int m = 0; m < vertCount; m++)
		{
			vec[m + 1] = verts[m].Position;
		}
		for (int n = 0; n < vec.Length; n++)
		{
			uvs[n] = new Vector2(vec[n].x, vec[n].y);
		}
		int num5 = 0;
		int[] array = new int[vertCount * 3];
		for (int num6 = 0; num6 < array.Length; num6 += 3)
		{
			array[num6] = 0;
			array[num6 + 1] = num5 + 1;
			if (num6 == array.Length - 3)
			{
				array[num6 + 2] = 1;
			}
			else
			{
				array[num6 + 2] = num5 + 2;
			}
			num5++;
		}
		myMesh.vertices = vec;
		myMesh.uv = uvs;
		myMesh.triangles = array;
	}

	private void CreateVert(ref Vector2 myPos, ref Vector2 del)
	{
		int num = Physics2D.Raycast(myPos, del, filter, buffer, LightRadius * 1.05f);
		if (num > 0)
		{
			Array.Sort(buffer, 0, num, HitDepthComparer.Instance);
			lightHits.Clear();
			RaycastHit2D raycastHit2D = buffer[0];
			for (int i = 0; i < num; i++)
			{
				RaycastHit2D raycastHit2D2 = buffer[i];
				lightHits.Add(raycastHit2D2);
				if (!raycastHit2D2.collider.isTrigger)
				{
					raycastHit2D = raycastHit2D2;
					break;
				}
			}
			for (int j = 0; j < lightHits.Count; j++)
			{
				int instanceID = lightHits[j].collider.gameObject.GetInstanceID();
				NoShadowBehaviour value;
				if (NoShadows.TryGetValue(instanceID, out value))
				{
					value.didHit = true;
				}
			}
			if (!raycastHit2D.collider.isTrigger)
			{
				Vector2 point = raycastHit2D.point;
				GetEmptyVert().Complete(point.x - myPos.x, point.y - myPos.y);
				return;
			}
		}
		Vector2 normalized = del.normalized;
		GetEmptyVert().Complete(normalized.x * LightRadius, normalized.y * LightRadius);
	}

	private VertInfo GetEmptyVert()
	{
		if (vertCount < verts.Count)
		{
			return verts[vertCount++];
		}
		VertInfo vertInfo = new VertInfo();
		verts.Add(vertInfo);
		vertCount = verts.Count;
		return vertInfo;
	}

	private static float length(float x, float y)
	{
		return Mathf.Sqrt(x * x + y * y);
	}

	public static float pseudoAngle(float dx, float dy)
	{
		if (dx < 0f)
		{
			float num = 0f - dx;
			float num2 = ((!(dy > 0f)) ? (0f - dy) : dy);
			return 2f - dy / (num + num2);
		}
		float num3 = ((!(dy > 0f)) ? (0f - dy) : dy);
		return dy / (dx + num3);
	}
}
