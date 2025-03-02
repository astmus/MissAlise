namespace MissAlise.Entities.OneDrive;

public partial class DataFile : ItemInfo
{
	public string Name { get; set; }
	public string Path { get; set; }
	public string? Extension { get; set; }
	public long? Size { get; set; }	
}
