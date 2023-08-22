using System.Collections;
using System.Text;
using PowerTools;
using UnityEngine;

public class UploadDataGame : Minigame
{
	public SpriteAnim LeftFolder;

	public SpriteAnim RightFolder;

	public AnimationClip FolderOpen;

	public AnimationClip FolderClose;

	public SpriteRenderer Runner;

	public HorizontalGauge Gauge;

	public TextRenderer PercentText;

	public TextRenderer EstimatedText;

	public TextRenderer SourceText;

	public TextRenderer TargetText;

	public SpriteRenderer Button;

	public Sprite DownloadImage;

	public GameObject Status;

	public GameObject Tower;

	private int count;

	private float timer;

	public const float RandomChunks = 5f;

	public const float ConstantTime = 3f;

	private bool running = true;

	public override void Begin(PlayerTask task)
	{
		PlayerControl.LocalPlayer.SetPlayerMaterialColors(Runner);
		base.Begin(task);
		if (MyNormTask.taskStep == 0)
		{
			Button.sprite = DownloadImage;
			Tower.SetActive(false);
			SourceText.Text = MyTask.StartAt.ToString();
			TargetText.Text = "My Tablet";
		}
		else
		{
			SourceText.Text = "My Tablet";
			TargetText.Text = "Headquarters";
		}
	}

	public void Click()
	{
		StartCoroutine(Transition());
	}

	private IEnumerator Transition()
	{
		Button.gameObject.SetActive(false);
		Status.SetActive(true);
		float target = Gauge.transform.localScale.x;
		for (float t = 0f; t < 0.15f; t += Time.deltaTime)
		{
			Gauge.transform.localScale = new Vector3(t / 0.15f * target, 1f, 1f);
			yield return null;
		}
		StartCoroutine(PulseText());
		StartCoroutine(DoRun());
		StartCoroutine(DoText());
		StartCoroutine(DoPercent());
	}

	private IEnumerator PulseText()
	{
		MeshRenderer rend2 = PercentText.GetComponent<MeshRenderer>();
		MeshRenderer rend1 = EstimatedText.GetComponent<MeshRenderer>();
		Color gray = new Color(0.3f, 0.3f, 0.3f, 1f);
		while (running)
		{
			yield return new WaitForLerp(0.4f, delegate(float t)
			{
				Color value2 = Color.Lerp(Color.black, gray, t);
				rend2.material.SetColor("_OutlineColor", value2);
				rend1.material.SetColor("_OutlineColor", value2);
			});
			yield return new WaitForLerp(0.4f, delegate(float t)
			{
				Color value = Color.Lerp(gray, Color.black, t);
				rend2.material.SetColor("_OutlineColor", value);
				rend1.material.SetColor("_OutlineColor", value);
			});
		}
		rend2.material.SetColor("_OutlineColor", Color.black);
		rend1.material.SetColor("_OutlineColor", Color.black);
	}

	private IEnumerator DoPercent()
	{
		while (running)
		{
			float perc2 = (float)count / 5f * 0.7f + timer / 3f * 0.3f;
			if (perc2 >= 1f)
			{
				running = false;
			}
			perc2 = Mathf.Clamp(perc2, 0f, 1f);
			Gauge.Value = perc2;
			PercentText.Text = Mathf.RoundToInt(perc2 * 100f) + "%";
			yield return null;
		}
	}

	private IEnumerator DoText()
	{
		StringBuilder txt = new StringBuilder("Estimated Time: ");
		int baselen = txt.Length;
		int max = 604800;
		count = 0;
		while ((float)count < 5f)
		{
			txt.Length = baselen;
			int value = IntRange.Next(max / 6, max);
			int days = value / 86400;
			if (days > 0)
			{
				txt.Append(days + "d ");
			}
			int hours = value / 3600 % 24;
			if (hours > 0)
			{
				txt.Append(hours + "hr ");
			}
			int minutes = value / 60 % 60;
			if (minutes > 0)
			{
				txt.Append(minutes + "m ");
			}
			int seconds = value % 60;
			if (seconds > 0)
			{
				txt.Append(seconds + "s");
			}
			EstimatedText.Text = txt.ToString();
			max /= 4;
			yield return new WaitForSeconds(FloatRange.Next(0.6f, 1.2f));
			count++;
		}
		for (timer = 0f; timer < 3f; timer += Time.deltaTime)
		{
			txt.Length = baselen;
			int seconds2 = Mathf.RoundToInt(3f - timer);
			txt.Append(seconds2 + "s");
			EstimatedText.Text = txt.ToString();
			yield return null;
		}
	}

	private IEnumerator DoRun()
	{
		while (running)
		{
			LeftFolder.Play(FolderOpen);
			Vector3 pos = Runner.transform.localPosition;
			yield return new WaitForLerp(1.125f, delegate(float t)
			{
				pos.x = Mathf.Lerp(-1.25f, 0.5625f, t);
				Runner.transform.localPosition = pos;
			});
			LeftFolder.Play(FolderClose);
			RightFolder.Play(FolderOpen);
			yield return new WaitForLerp(1.375f, delegate(float t)
			{
				pos.x = Mathf.Lerp(0.5625f, 1.25f, t);
				Runner.transform.localPosition = pos;
			});
			yield return new WaitForAnimationFinish(RightFolder, FolderClose);
		}
		EstimatedText.Text = "Complete";
		MyNormTask.NextStep();
		StartCoroutine(CoStartClose());
	}
}
