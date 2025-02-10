namespace MissAlise.Entities.OneDrive;

public partial class File : Item
{
	public string Caption { get; set; }
	public long? Size { get; set; }	
	public string? Extension { get; set; }
	public virtual int Folderid { get; set; }
	public Folder? Folder { get; set; }
}
