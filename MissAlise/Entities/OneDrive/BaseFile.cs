namespace MissAlise.Entities.OneDrive;

public partial class BaseFile : ItemInfo
{
	public string Caption { get; set; }
	public long? Size { get; set; }	
	public string? Extension { get; set; }
	public int FolderId { get; set; }
	public Folder? Folder { get; set; }
}
