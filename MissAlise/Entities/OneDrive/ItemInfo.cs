namespace MissAlise.Entities.OneDrive;

public partial class ItemInfo
{
	public string Id { get; set; }
	public string? Title { get; set; }
	public string? MimeType { get; set; }
	public DateTimeOffset? CreatedDateTime { get; set; }
	public DateTimeOffset? ModifieDateTime { get; set; }
	public ItemInfo? Parent { get; set; }
}
