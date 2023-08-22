using System.Collections;
using UnityEngine;

public class DummyBehaviour : MonoBehaviour
{
	private PlayerControl myPlayer;

	private FloatRange voteTime = new FloatRange(3f, 8f);

	private bool voted;

	public void Start()
	{
		myPlayer = GetComponent<PlayerControl>();
	}

	public void Update()
	{
		if (myPlayer.IsDead)
		{
			return;
		}
		if ((bool)MeetingHud.Instance)
		{
			if (!voted)
			{
				voted = true;
				StartCoroutine(DoVote());
			}
		}
		else
		{
			voted = false;
		}
	}

	private IEnumerator DoVote()
	{
		yield return new WaitForSeconds(voteTime.Next());
		sbyte votebyte = -1;
		for (int i = 0; i < 100 && i != 99; i++)
		{
			int num = IntRange.Next(-1, GameData.Instance.AllPlayers.Count);
			if (num >= 0)
			{
				PlayerControl @object = GameData.Instance.AllPlayers[num].Object;
				if (!@object.IsDead)
				{
					votebyte = (sbyte)@object.PlayerId;
					break;
				}
				continue;
			}
			votebyte = (sbyte)num;
			break;
		}
		MeetingHud.Instance.RpcCastVote(myPlayer.PlayerId, votebyte);
	}
}
