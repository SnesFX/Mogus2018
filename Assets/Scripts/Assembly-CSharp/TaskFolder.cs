using UnityEngine;

public class TaskFolder : MonoBehaviour
{
	public string FolderName;

	public TextRenderer Text;

	public TaskAdderGame Parent;

	public TaskFolder[] SubFolders;

	public PlayerTask[] Children;

	public TaskAddButton InfectedButton;

	public void Start()
	{
		Text.Text = FolderName;
	}

	public void OnClick()
	{
		Parent.ShowFolder(this);
	}
}
