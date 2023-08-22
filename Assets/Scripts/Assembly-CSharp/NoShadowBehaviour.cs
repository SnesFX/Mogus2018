using UnityEngine;

public class NoShadowBehaviour : MonoBehaviour
{
	public Renderer rend;

	public bool didHit;

	public Renderer shadowChild;

	public void Start()
	{
		LightSource.NoShadows.Add(base.gameObject.GetInstanceID(), this);
	}

	private void LateUpdate()
	{
		if ((bool)PlayerControl.LocalPlayer && !PlayerControl.LocalPlayer.IsDead)
		{
			if (didHit)
			{
				didHit = false;
				if ((bool)ShipStatus.Instance && ShipStatus.Instance.CalculateLightRadius(PlayerControl.LocalPlayer) > ShipStatus.Instance.MaxLightRadius / 2f)
				{
					SetMaskFunction(8);
					return;
				}
			}
			SetMaskFunction(1);
		}
		else
		{
			SetMaskFunction(8);
		}
	}

	private void SetMaskFunction(int func)
	{
		rend.material.SetInt("_Mask", func);
		if ((bool)shadowChild)
		{
			shadowChild.material.SetInt("_Mask", func);
		}
	}

	public void OnDestroy()
	{
		LightSource.NoShadows.Remove(base.gameObject.GetInstanceID());
	}
}
