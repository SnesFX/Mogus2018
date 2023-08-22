using System.Collections;
using UnityEngine;

public class Scene1Controller : SceneController
{
	public PlayerAnimator[] players;

	public DummyConsole[] Consoles;

	public Vector2[] WayPoints;

	public Camera backupCam;

	public void OnDrawGizmos()
	{
		for (int i = 0; i < WayPoints.Length; i++)
		{
			Vector2 vector = WayPoints[i];
			Vector2 vector2 = WayPoints[(i + 1) % WayPoints.Length];
			Gizmos.DrawLine(vector, vector2);
		}
	}

	public void OnEnable()
	{
		backupCam.cullingMask = 0;
		StartCoroutine(RunPlayer(0));
		StartCoroutine(RunPlayer(1));
	}

	public void OnDisable()
	{
		backupCam.cullingMask = 0x7FFFFFFF ^ LayerMask.GetMask("UI");
	}

	private IEnumerator RunPlayer(int idx)
	{
		PlayerAnimator myPlayer = players[idx];
		while (true)
		{
			for (int i = 0; i < WayPoints.Length; i++)
			{
				bool willInterrupt = i == 2 || i == 5;
				yield return myPlayer.WalkPlayerTo(WayPoints[i], willInterrupt, 0.1f);
				if (willInterrupt)
				{
					yield return DoUse(idx, (i != 2) ? 1 : 0);
				}
			}
		}
	}

	private IEnumerator DoUse(int idx, int consoleid)
	{
		PlayerAnimator myPlayer = players[idx];
		yield return WaitForSeconds(0.2f);
		if (idx == 0)
		{
			yield return myPlayer.finger.MoveTo(myPlayer.UseButton.transform.position, 0.75f);
		}
		else
		{
			yield return myPlayer.finger.MoveTo(Consoles[consoleid].transform.position, 0.75f);
		}
		yield return WaitForSeconds(0.2f);
		yield return myPlayer.finger.DoClick(0.4f);
		yield return WaitForSeconds(0.2f);
		if (!(myPlayer.joystick is DemoKeyboardStick))
		{
			yield return myPlayer.finger.MoveTo(myPlayer.joystick.transform.position, 0.75f);
		}
		else
		{
			yield return WaitForSeconds(0.75f);
		}
	}

	public static IEnumerator WaitForSeconds(float duration)
	{
		for (float time = 0f; time < duration; time += Time.deltaTime)
		{
			yield return null;
		}
	}
}
