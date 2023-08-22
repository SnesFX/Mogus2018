using System.Collections;
using System.Linq;
using UnityEngine;

public class EmptyGarbageMinigame : Minigame
{
	public FloatRange HandleRange = new FloatRange(-0.65f, 0.65f);

	public Vector2Range SpawnRange = default(Vector2Range);

	public Collider2D Blocker;

	public AreaEffector2D Popper;

	public Collider2D Handle;

	public SpriteRenderer Bars;

	private Controller controller = new Controller();

	private bool finished;

	public int NumObjects = 15;

	private SpriteRenderer[] Objects;

	public SpriteRenderer[] GarbagePrefabs;

	public SpriteRenderer[] LeafPrefabs;

	public SpriteRenderer[] SpecialObjectPrefabs;

	public AudioClip LeverDown;

	public AudioClip LeverUp;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		int i = 0;
		Objects = new SpriteRenderer[NumObjects];
		RandomFill<SpriteRenderer> randomFill = new RandomFill<SpriteRenderer>();
		int? num = (((object)MyNormTask != null) ? new int?(MyNormTask.taskStep) : null);
		if (num.HasValue && num.Value == 0)
		{
			if (MyNormTask.StartAt == SystemTypes.Cafeteria)
			{
				randomFill.Set(GarbagePrefabs);
			}
			else
			{
				randomFill.Set(LeafPrefabs);
			}
		}
		else
		{
			randomFill.Set(GarbagePrefabs.Union(LeafPrefabs));
			for (; i < SpecialObjectPrefabs.Length; i++)
			{
				SpriteRenderer spriteRenderer = (Objects[i] = Object.Instantiate(SpecialObjectPrefabs[i]));
				spriteRenderer.transform.SetParent(base.transform);
				spriteRenderer.transform.localPosition = SpawnRange.Next();
			}
		}
		for (; i < Objects.Length; i++)
		{
			SpriteRenderer spriteRenderer2 = (Objects[i] = Object.Instantiate(randomFill.Get()));
			spriteRenderer2.transform.SetParent(base.transform);
			Vector3 localPosition = SpawnRange.Next();
			localPosition.z = FloatRange.Next(-0.5f, 0.5f);
			spriteRenderer2.transform.localPosition = localPosition;
			spriteRenderer2.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.white, Color.black, (localPosition.z + 0.5f) * 0.7f);
		}
	}

	public void Update()
	{
		controller.Update();
		Vector3 localPosition = Handle.transform.localPosition;
		float num = HandleRange.ReverseLerp(localPosition.y);
		switch (controller.CheckDrag(Handle))
		{
		case DragState.Dragging:
			if (finished)
			{
				break;
			}
			if (num > 0.5f)
			{
				Vector2 vector = controller.DragPosition - (Vector2)base.transform.position;
				float num2 = HandleRange.ReverseLerp(HandleRange.Clamp(vector.y));
				localPosition.y = HandleRange.Lerp(num2 / 2f + 0.5f);
				break;
			}
			localPosition.y = Mathf.Lerp(localPosition.y, HandleRange.min, num + Time.deltaTime * 15f);
			if (Blocker.enabled)
			{
				if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySound(LeverDown, false);
				}
				Blocker.enabled = false;
				StopAllCoroutines();
				StartCoroutine(PopObjects());
				StartCoroutine(AnimateObjects());
			}
			break;
		case DragState.NoTouch:
			localPosition.y = Mathf.Lerp(localPosition.y, HandleRange.max, num + Time.deltaTime * 15f);
			break;
		case DragState.Released:
			if (!Blocker.enabled)
			{
				Blocker.enabled = true;
				if (Constants.ShouldPlaySfx())
				{
					SoundManager.Instance.PlaySound(LeverUp, false);
				}
			}
			if (!finished && Objects.All((SpriteRenderer o) => !o))
			{
				finished = true;
				MyNormTask.NextStep();
				StartCoroutine(CoStartClose());
			}
			break;
		}
		Handle.transform.localPosition = localPosition;
		Vector3 localScale = Bars.transform.localScale;
		localScale.y = HandleRange.ChangeRange(localPosition.y, -1f, 1f);
		Bars.transform.localScale = localScale;
	}

	private IEnumerator PopObjects()
	{
		Popper.enabled = true;
		yield return new WaitForSeconds(0.05f);
		Popper.enabled = false;
	}

	private IEnumerator AnimateObjects()
	{
		Vector3 pos = base.transform.localPosition;
		for (float t = 3f; t > 0f; t -= Time.deltaTime)
		{
			float r = t / 3f;
			base.transform.localPosition = pos + (Vector3)Vector2Range.NextEdge() * r * 0.1f;
			yield return null;
		}
	}

	public override void Close(bool allowMovement)
	{
		if ((bool)MyNormTask && MyNormTask.IsComplete)
		{
			ShipStatus.Instance.OpenHatch();
			PlayerControl.LocalPlayer.RpcPlayAnimation((byte)MyTask.TaskType);
		}
		base.Close(allowMovement);
	}
}
