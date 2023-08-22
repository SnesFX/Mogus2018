using UnityEngine;

public class TowerBehaviour : MonoBehaviour
{
	public float timer;

	public float frameTime = 0.2f;

	public SpriteRenderer circle;

	public SpriteRenderer middle1;

	public SpriteRenderer middle2;

	public SpriteRenderer outer1;

	public SpriteRenderer outer2;

	public void Update()
	{
		timer += Time.deltaTime;
		if (timer < frameTime)
		{
			circle.color = Color.white;
			SpriteRenderer spriteRenderer = middle1;
			Color black = Color.black;
			outer2.color = black;
			black = black;
			outer1.color = black;
			black = black;
			middle2.color = black;
			spriteRenderer.color = black;
		}
		else if (timer < 2f * frameTime)
		{
			SpriteRenderer spriteRenderer2 = middle1;
			Color black = Color.white;
			middle2.color = black;
			spriteRenderer2.color = black;
			SpriteRenderer spriteRenderer3 = circle;
			black = Color.black;
			outer2.color = black;
			black = black;
			outer1.color = black;
			spriteRenderer3.color = black;
		}
		else if (timer < 3f * frameTime)
		{
			SpriteRenderer spriteRenderer4 = outer1;
			Color black = Color.white;
			outer2.color = black;
			spriteRenderer4.color = black;
			SpriteRenderer spriteRenderer5 = middle1;
			black = Color.black;
			circle.color = black;
			black = black;
			middle2.color = black;
			spriteRenderer5.color = black;
		}
		else
		{
			timer = 0f;
		}
	}
}
