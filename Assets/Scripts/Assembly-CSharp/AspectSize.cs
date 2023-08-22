using UnityEngine;

public class AspectSize : MonoBehaviour
{
	public Sprite Background;

	public SpriteRenderer Renderer;

	public float PercentWidth = 0.95f;

	public void OnEnable()
	{
		Camera main = Camera.main;
		float orthographicSize = main.orthographicSize;
		float num = orthographicSize * main.aspect;
		Sprite sprite = ((!Background) ? Renderer.sprite : Background);
		float num2 = sprite.bounds.size.x / 2f;
		float num3 = num / num2 * PercentWidth;
		if (num3 < 1f)
		{
			base.transform.localScale = new Vector3(num3, num3, num3);
		}
	}
}
