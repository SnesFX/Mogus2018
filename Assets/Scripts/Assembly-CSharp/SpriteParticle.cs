using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SpriteParticle : MonoBehaviour
{
	private const float FrameRate = 24f;

	public Sprite[] Sprites;

	public ParticleInfo[] Particles;

	public ushort[][] TriangleCache;

	private Vector3[] verts;

	private Vector2[] uvs;

	private List<int> tris = new List<int>();

	private Mesh mesh;

	private int MaxVerts;

	public void OnDrawGizmos()
	{
		if (Particles != null && Sprites != null && Sprites.Length != 0)
		{
			Sprite sprite = Sprites[0];
			for (int i = 0; i < Particles.Length; i++)
			{
				ParticleInfo particleInfo = Particles[i];
				Vector3 center = particleInfo.Position + base.transform.position;
				Gizmos.DrawCube(center, new Vector3(sprite.bounds.size.x * particleInfo.Scale, sprite.bounds.size.y * particleInfo.Scale, sprite.bounds.size.x * particleInfo.Scale));
			}
		}
	}

	public void Start()
	{
		MeshFilter component = GetComponent<MeshFilter>();
		mesh = new Mesh();
		mesh.MarkDynamic();
		component.mesh = mesh;
		TriangleCache = new ushort[Sprites.Length][];
		for (int i = 0; i < Sprites.Length; i++)
		{
			if (MaxVerts < Sprites[i].vertices.Length)
			{
				MaxVerts = Sprites[i].vertices.Length;
			}
			TriangleCache[i] = Sprites[i].triangles;
		}
		verts = new Vector3[Particles.Length * MaxVerts];
		uvs = new Vector2[verts.Length];
		for (int j = 0; j < Particles.Length; j++)
		{
			int num = j * MaxVerts;
			int num2 = (int)Particles[j].Timer;
			Sprite sprite = Sprites[num2];
			Vector2[] vertices = sprite.vertices;
			Vector2[] uv = sprite.uv;
			for (int k = 0; k < sprite.vertices.Length; k++)
			{
				int num3 = num + k;
				verts[num3].x = vertices[k].x * Particles[j].Scale + Particles[j].Position.x;
				verts[num3].y = vertices[k].y * Particles[j].Scale + Particles[j].Position.y;
				uvs[num3] = uv[k];
			}
			ushort[] triangles = sprite.triangles;
			for (int l = 0; l < triangles.Length; l++)
			{
				tris.Add(triangles[l]);
			}
		}
		mesh.vertices = verts;
		mesh.uv = uvs;
		mesh.SetTriangles(tris, 0);
	}

	public void Update()
	{
		float num = Time.deltaTime * 24f;
		tris.Clear();
		for (int i = 0; i < Particles.Length; i++)
		{
			float num2 = Particles[i].Timer + num;
			if (num2 > (float)Sprites.Length)
			{
				num2 %= 24f;
			}
			Particles[i].Timer = num2;
			int num3 = i * MaxVerts;
			int num4 = (int)Particles[i].Timer;
			Sprite sprite = Sprites[num4];
			Vector2[] vertices = sprite.vertices;
			Vector2[] uv = sprite.uv;
			for (int j = 0; j < vertices.Length; j++)
			{
				int num5 = num3 + j;
				verts[num5].x = vertices[j].x * Particles[i].Scale + Particles[i].Position.x;
				verts[num5].y = vertices[j].y * Particles[i].Scale + Particles[i].Position.y;
				uvs[num5] = uv[j];
			}
			ushort[] array = TriangleCache[num4];
			for (int k = 0; k < array.Length; k++)
			{
				tris.Add(array[k] + num3);
			}
		}
		mesh.vertices = verts;
		mesh.uv = uvs;
		mesh.SetTriangles(tris, 0);
	}
}
