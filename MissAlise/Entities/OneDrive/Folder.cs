namespace MissAlise.Entities.OneDrive;

public partial class Folder : Item
{
	public int? Parentfolderid { get; set; }

	public string Name { get; set; }

	public string Path { get; set; }

	public Folder? Parent { get; set; }

	public ICollection<Folder> Folders { get; set; } = new List<Folder>();

	public virtual ICollection<File> Files { get; set; } = new List<File>();
}
