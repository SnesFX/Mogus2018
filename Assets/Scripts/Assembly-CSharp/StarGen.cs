using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class StarGen : MonoBehaviour
{
	[Serializable]
	private struct Stars
	{
		public float Size;

		public float Rate;

		public float PositionX;

		public float PositionY;
	}

	public static StarGen instance;

	public int NumStars = 500;

	public float Length = 25f;

	public float Width = 25f;

	public Vector2 Direction = new Vector2(1f, 0f);

	private Vector2 NormDir = new Vector2(1f, 0f);

	private Vector2 Tangent = new Vector2(0f, 1f);

	private float tanLen;

	public FloatRange Sizes = new FloatRange(0.01f, 0.1f);

	public FloatRange Rates = new FloatRange(0.25f, 1f);

	[HideInInspector]
	private Stars[] stars;

	[HideInInspector]
	private Vector3[] verts;

	[HideInInspector]
	private Mesh mesh;

	public void Awake()
	{
		instance = this;
	}

	public void Start()
	{
		this.stars = new Stars[NumStars];
		verts = new Vector3[NumStars * 4];
		Vector2[] array = new Vector2[NumStars * 4];
		Color32[] array2 = new Color32[NumStars * 4];
		int[] array3 = new int[NumStars * 6];
		SetDirection(Direction);
		MeshFilter component = GetComponent<MeshFilter>();
		mesh = new Mesh();
		mesh.MarkDynamic();
		component.mesh = mesh;
		for (int i = 0; i < this.stars.Length; i++)
		{
			Stars stars = this.stars[i];
			float num = FloatRange.Next(-1f, 1f) * Length;
			float num2 = FloatRange.Next(-1f, 1f) * Width;
			float num3 = (stars.PositionX = num * NormDir.x + num2 * Tangent.x);
			float num4 = (stars.PositionY = num * NormDir.y + num2 * Tangent.y);
			float num5 = (stars.Size = Sizes.Next());
			stars.Rate = Rates.Next();
			this.stars[i] = stars;
			int num6 = i * 4;
			verts[num6].Set(num3 - num5, num4 + num5, 0f);
			verts[num6 + 1].Set(num3 - num5, num4 - num5, 0f);
			verts[num6 + 2].Set(num3 + num5, num4 - num5, 0f);
			verts[num6 + 3].Set(num3 + num5, num4 + num5, 0f);
			array[num6].Set(-1f, 1f);
			array[num6 + 1].Set(-1f, -1f);
			array[num6 + 2].Set(1f, -1f);
			array[num6 + 3].Set(1f, 1f);
			int num7 = i * 6;
			array3[num7] = num6;
			array3[num7 + 1] = num6 + 1;
			array3[num7 + 2] = num6 + 2;
			array3[num7 + 3] = num6 + 2;
			array3[num7 + 4] = num6;
			array3[num7 + 5] = num6 + 3;
			Color32 color = Palette.PlayerColors.Random();
			array2[num6] = color;
			array2[num6 + 1] = color;
			array2[num6 + 2] = color;
			array2[num6 + 3] = color;
		}
		mesh.vertices = verts;
		mesh.uv = array;
		mesh.colors32 = array2;
		mesh.SetIndices(array3, MeshTopology.Triangles, 0);
	}

	private void FixedUpdate()
	{
		float num = -0.99f * Length;
		Vector2 vector = Direction * Time.fixedDeltaTime;
		for (int i = 0; i < this.stars.Length; i++)
		{
			Stars stars = this.stars[i];
			float size = stars.Size;
			float positionX = stars.PositionX;
			float positionY = stars.PositionY;
			float num2 = stars.Rate * (size / Sizes.max);
			positionX += num2 * vector.x;
			positionY += num2 * vector.y;
			if (OrthoDistance(positionX, positionY) > Length)
			{
				float num3 = FloatRange.Next(-1f, 1f) * Width;
				positionX = num * NormDir.x + num3 * Tangent.x;
				positionY = num * NormDir.y + num3 * Tangent.y;
				this.stars[i].Rate = Rates.Next();
			}
			stars.PositionX = positionX;
			stars.PositionY = positionY;
			this.stars[i] = stars;
			int num4 = i * 4;
			float x = positionX - size;
			float x2 = positionX + size;
			float y = positionY + size;
			float y2 = positionY - size;
			verts[num4].x = x;
			verts[num4].y = y;
			verts[num4 + 1].x = x;
			verts[num4 + 1].y = y2;
			verts[num4 + 2].x = x2;
			verts[num4 + 2].y = y2;
			verts[num4 + 3].x = x2;
			verts[num4 + 3].y = y;
		}
		mesh.vertices = verts;
	}

	public void SetDirection(Vector2 dir)
	{
		Direction = dir;
		NormDir = Direction.normalized;
		Tangent = new Vector2(0f - NormDir.y, NormDir.x);
		tanLen = Mathf.Sqrt(Tangent.y * Tangent.y + Tangent.x * Tangent.x);
	}

	public void RegenPositions()
	{
		if (stars != null)
		{
			for (int i = 0; i < stars.Length; i++)
			{
				float num = FloatRange.Next(-1f, 1f) * Length;
				float num2 = FloatRange.Next(-1f, 1f) * Width;
				stars[i].PositionX = num * NormDir.x + num2 * Tangent.x;
				stars[i].PositionY = num * NormDir.y + num2 * Tangent.y;
			}
		}
	}

	private float OrthoDistance(float pointx, float pointy)
	{
		return (Tangent.y * pointx - Tangent.x * pointy) / tanLen;
	}
}
