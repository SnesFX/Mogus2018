public interface IUsable
{
	float UsableDistance { get; }

	float PercentCool { get; }

	void SetOutline(bool on, bool mainTarget);

	bool CanUse(PlayerControl pc);

	void Use();
}
