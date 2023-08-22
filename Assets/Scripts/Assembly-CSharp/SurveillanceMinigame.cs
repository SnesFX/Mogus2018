using System.Collections;
using System.Linq;
using UnityEngine;

public class SurveillanceMinigame : Minigame
{
	public Camera CameraPrefab;

	public GameObject Viewables;

	public MeshRenderer[] ViewPorts;

	private ShipRoom[] FilteredRooms;

	private RenderTexture[] textures;

	public MeshRenderer FillQuad;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		FilteredRooms = (from i in Object.FindObjectsOfType<ShipRoom>()
			where i.survCamera
			select i).ToArray();
		ShipStatus.Instance.RpcRepairSystem(SystemTypes.Security, 1);
		textures = new RenderTexture[FilteredRooms.Length];
		for (int j = 0; j < FilteredRooms.Length; j++)
		{
			ShipRoom shipRoom = FilteredRooms[j];
			Camera camera = Object.Instantiate(CameraPrefab);
			camera.transform.SetParent(base.transform);
			camera.transform.position = shipRoom.transform.position + shipRoom.survCamera.Offset;
			camera.orthographicSize = shipRoom.survCamera.CamSize;
			RenderTexture temporary = RenderTexture.GetTemporary((int)(256f * shipRoom.survCamera.CamAspect), 256, 16, RenderTextureFormat.ARGB32);
			textures[j] = temporary;
			camera.targetTexture = temporary;
			ViewPorts[j].material.SetTexture("_MainTex", temporary);
		}
	}

	protected override IEnumerator CoAnimateOpen()
	{
		Viewables.SetActive(false);
		FillQuad.material.SetFloat("_Center", -5f);
		FillQuad.material.SetColor("_Color2", Color.clear);
		for (float timer3 = 0f; timer3 < 0.25f; timer3 += Time.deltaTime)
		{
			FillQuad.material.SetColor("_Color2", Color.Lerp(Color.clear, Color.black, timer3 / 0.25f));
			yield return null;
		}
		FillQuad.material.SetColor("_Color2", Color.black);
		Viewables.SetActive(true);
		for (float timer2 = 0f; timer2 < 0.1f; timer2 += Time.deltaTime)
		{
			FillQuad.material.SetFloat("_Center", Mathf.Lerp(-5f, 0f, timer2 / 0.1f));
			yield return null;
		}
		for (float timer = 0f; timer < 0.15f; timer += Time.deltaTime)
		{
			FillQuad.material.SetFloat("_Center", Mathf.Lerp(-3f, 0.4f, timer / 0.15f));
			yield return null;
		}
		FillQuad.material.SetFloat("_Center", 0.4f);
	}

	private IEnumerator CoAnimateClose()
	{
		for (float timer2 = 0f; timer2 < 0.1f; timer2 += Time.deltaTime)
		{
			FillQuad.material.SetFloat("_Center", Mathf.Lerp(0.4f, -5f, timer2 / 0.1f));
			yield return null;
		}
		Viewables.SetActive(false);
		for (float timer = 0f; timer < 0.3f; timer += Time.deltaTime)
		{
			FillQuad.material.SetColor("_Color2", Color.Lerp(Color.black, Color.clear, timer / 0.3f));
			yield return null;
		}
		FillQuad.material.SetColor("_Color2", Color.clear);
	}

	protected override IEnumerator CoDestroySelf()
	{
		yield return CoAnimateClose();
		Object.Destroy(base.gameObject);
	}

	public void OnDestroy()
	{
		ShipStatus.Instance.RpcRepairSystem(SystemTypes.Security, 2);
		for (int i = 0; i < textures.Length; i++)
		{
			textures[i].Release();
		}
	}
}
