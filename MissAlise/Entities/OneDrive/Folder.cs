namespace MissAlise.Entities.OneDrive;

public partial class Folder : ItemInfo
{
	public int? Parentfolderid { get; set; }
	public string Path { get; set; }
	public Folder? Parent { get; set; }	
	public ICollection<ItemInfo> Children { get; set; } = new List<ItemInfo>();
}
