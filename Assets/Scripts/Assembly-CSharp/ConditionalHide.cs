using System.Linq;
using UnityEngine;

public class ConditionalHide : MonoBehaviour
{
	public RuntimePlatform[] HideForPlatforms = new RuntimePlatform[1] { RuntimePlatform.WindowsPlayer };

	private void Awake()
	{
		if (HideForPlatforms.Any((RuntimePlatform p) => p == Application.platform))
		{
			base.gameObject.SetActive(false);
		}
	}
}
