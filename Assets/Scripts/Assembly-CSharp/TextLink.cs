using UnityEngine;

public class TextLink : MonoBehaviour
{
	public BoxCollider2D boxCollider;

	public string targetUrl;

	public bool needed;

	public void Set(Vector2 from, Vector2 to, string target)
	{
		targetUrl = target;
		Vector2 vector = to + from;
		base.transform.localPosition = new Vector3(vector.x / 2f, vector.y / 2f, -1f);
		vector = to - from;
		vector.y = 0f - vector.y;
		boxCollider.size = vector;
	}

	public void Click()
	{
		Application.OpenURL(targetUrl);
	}
}
