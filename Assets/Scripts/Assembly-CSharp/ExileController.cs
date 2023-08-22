using System.Collections;
using System.Linq;
using UnityEngine;

public class ExileController : MonoBehaviour
{
	public TextRenderer ImpostorText;

	public TextRenderer Text;

	public SpriteRenderer Player;

	public SpriteRenderer PlayerHat;

	public AnimationCurve LerpCurve;

	public float Duration = 7f;

	private string completeString = string.Empty;

	private PlayerControl exiled;

	public void Begin(PlayerControl exiled, bool tie)
	{
		this.exiled = exiled;
		Text.gameObject.SetActive(false);
		Text.Text = string.Empty;
		if ((bool)exiled)
		{
			int num = GameData.Instance.AllPlayers.Count((GameData.PlayerInfo p) => (bool)p.Object && p.Object.IsImpostor);
			string arg = ((!exiled.IsImpostor) ? "not " : string.Empty);
			string arg2 = ((num <= 1) ? "An" : "The");
			completeString = string.Format("{0} was {1}{2} Impostor", exiled.PlayerName, arg, arg2);
			exiled.SetPlayerMaterialColors(Player);
			PlayerControl.SetHatImage(exiled.HatId, PlayerHat);
		}
		else
		{
			completeString = string.Format("No one was ejected ({0})", (!tie) ? "Skipped" : "Tie");
			Player.gameObject.SetActive(false);
		}
		int num2 = GameData.Instance.AllPlayers.Count((GameData.PlayerInfo p) => (bool)p.Object && p.Object.IsImpostor && !p.Object.IsDead);
		if ((bool)exiled && exiled.IsImpostor)
		{
			num2--;
		}
		ImpostorText.Text = num2 + ((num2 == 1) ? " impostor remains" : " impostors remain");
		StartCoroutine(Animate());
	}

	private IEnumerator Animate()
	{
		yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.black, Color.clear);
		yield return new WaitForSeconds(1f);
		float width = Camera.main.orthographicSize * Camera.main.aspect + 1f;
		Vector2 left = Vector2.left * width;
		Vector2 right = Vector2.right * width;
		for (float t = 0f; t <= Duration; t += Time.deltaTime)
		{
			float p = t / Duration;
			Player.transform.localPosition = Vector2.Lerp(left, right, LerpCurve.Evaluate(p));
			float rot = (t + 0.75f) * 25f / Mathf.Exp(t * 0.75f + 1f);
			Player.transform.Rotate(new Vector3(0f, 0f, rot));
			if (p >= 0.3f)
			{
				float num = Mathf.Min(1f, (p - 0.3f) / 0.3f);
				int num2 = (int)(num * (float)completeString.Length);
				if (num2 > Text.Text.Length)
				{
					Text.Text = completeString.Substring(0, num2);
					Text.gameObject.SetActive(true);
				}
			}
			yield return null;
		}
		Text.Text = completeString;
		ImpostorText.gameObject.SetActive(true);
		yield return Effects.Bloop(0f, ImpostorText.transform);
		yield return new WaitForSeconds(0.5f);
		yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.clear, Color.black);
		if ((bool)exiled)
		{
			exiled.Exiled();
		}
		if (DestroyableSingleton<TutorialManager>.InstanceExists || !ShipStatus.Instance.IsGameOverDueToDeath())
		{
			DestroyableSingleton<HudManager>.Instance.StartCoroutine(DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.black, Color.clear));
			PlayerControl.LocalPlayer.canMove = true;
			PlayerControl.LocalPlayer.killTimer = PlayerControl.GameOptions.KillCooldown;
			Camera.main.GetComponent<FollowerCamera>().Locked = false;
			DestroyableSingleton<HudManager>.Instance.SetHudActive(true);
		}
		Object.Destroy(base.gameObject);
	}
}
