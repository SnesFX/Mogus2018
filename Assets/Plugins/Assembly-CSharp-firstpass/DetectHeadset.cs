using System.Runtime.InteropServices;

public class DetectHeadset
{
	[DllImport("__Internal")]
	private static extern bool _Detect();

	public static bool Detect()
	{
		return true;
	}
}
