namespace MissAlise.Entities.OneDrive;

public partial class Folder : ItemInfo
{
	public int? Parentfolderid { get; set; }

	public string Title { get; set; }

	public string Path { get; set; }

	public Folder? Parent { get; set; }

	//public ICollection<Folder> Folders { get; set; } = new List<Folder>();

	public ICollection<BaseFile> Files { get; set; } = new List<BaseFile>();
}
