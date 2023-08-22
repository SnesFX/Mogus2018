using System.Collections;
using UnityEngine;

public class WeaponsMinigame : Minigame
{
	public FloatRange XSpan = new FloatRange(-1.15f, 1.15f);

	public FloatRange YSpan = new FloatRange(-1.15f, 1.15f);

	public FloatRange TimeToSpawn;

	public ObjectPoolBehavior asteroidPool;

	public TextController ScoreText;

	public SpriteRenderer TargetReticle;

	public LineRenderer TargetLines;

	private Vector3 TargetCenter;

	public Collider2D BackgroundCol;

	public SpriteRenderer Background;

	public Controller myController = new Controller();

	private float Timer;

	public AudioClip ShootSound;

	public AudioClip[] ExplodeSounds;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		ScoreText.Text = "Destroyed: " + MyNormTask.taskStep;
		TimeToSpawn.Next();
	}

	protected override IEnumerator CoAnimateOpen()
	{
		for (float timer2 = 0f; timer2 < 0.1f; timer2 += Time.deltaTime)
		{
			float t = timer2 / 0.1f;
			base.transform.localScale = new Vector3(t, 0.1f, t);
			yield return null;
		}
		for (float timer = 0.010000001f; timer < 0.1f; timer += Time.deltaTime)
		{
			float t2 = timer / 0.1f;
			base.transform.localScale = new Vector3(1f, t2, 1f);
			yield return null;
		}
		base.transform.localScale = new Vector3(1f, 1f, 1f);
	}

	protected override IEnumerator CoDestroySelf()
	{
		for (float timer2 = 0.010000001f; timer2 < 0.1f; timer2 += Time.deltaTime)
		{
			float t = 1f - timer2 / 0.1f;
			base.transform.localScale = new Vector3(1f, t, 1f);
			yield return null;
		}
		for (float timer = 0f; timer < 0.1f; timer += Time.deltaTime)
		{
			float t2 = 1f - timer / 0.1f;
			base.transform.localScale = new Vector3(t2, 0.1f, t2);
			yield return null;
		}
		Object.Destroy(base.gameObject);
	}

	public void FixedUpdate()
	{
		Background.color = Color.Lerp(Palette.ClearWhite, Color.white, Mathf.Sin(Time.time * 3f) * 0.1f + 0.8f);
		if ((bool)MyNormTask && MyNormTask.IsComplete)
		{
			return;
		}
		Timer += Time.fixedDeltaTime;
		if (Timer >= TimeToSpawn.Last)
		{
			Timer = 0f;
			TimeToSpawn.Next();
			if (asteroidPool.InUse < MyNormTask.MaxStep - MyNormTask.TaskStep)
			{
				Asteroid ast = asteroidPool.Get<Asteroid>();
				ast.transform.localPosition = new Vector3(XSpan.max, YSpan.Next(), -1f);
				ast.TargetPosition = new Vector3(XSpan.min, YSpan.Next(), -1f);
				ButtonBehavior component = ast.GetComponent<ButtonBehavior>();
				component.OnClick.AddListener(delegate
				{
					BreakApart(ast);
				});
			}
		}
		myController.Update();
		if (myController.CheckDrag(BackgroundCol) == DragState.TouchStart)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(ShootSound, false);
			}
			Vector3 vector = (Vector3)myController.DragPosition - base.transform.position;
			vector.z = -2f;
			TargetReticle.transform.localPosition = vector;
			vector.z = 0f;
			TargetLines.SetPosition(1, vector);
			if (!ShipStatus.Instance.WeaponsImage.IsPlaying())
			{
				ShipStatus.Instance.FireWeapon();
				PlayerControl.LocalPlayer.RpcPlayAnimation(6);
			}
		}
	}

	public void BreakApart(Asteroid ast)
	{
		if (Constants.ShouldPlaySfx())
		{
			AudioSource audioSource = SoundManager.Instance.PlaySound(ExplodeSounds.Random(), false);
			audioSource.pitch = FloatRange.Next(0.8f, 1.2f);
		}
		if (MyNormTask.IsComplete)
		{
			return;
		}
		StartCoroutine(ast.CoBreakApart());
		if ((bool)MyNormTask)
		{
			MyNormTask.NextStep();
			ScoreText.Text = "Destroyed: " + MyNormTask.taskStep;
		}
		if (!MyNormTask || !MyNormTask.IsComplete)
		{
			return;
		}
		StartCoroutine(CoStartClose());
		foreach (Asteroid activeChild in asteroidPool.activeChildren)
		{
			if (!(activeChild == ast))
			{
				StartCoroutine(activeChild.CoBreakApart());
			}
		}
	}
}
