using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace MissAlise.Entities.OneDrive;

public partial class ItemInfo
{
	public string Id { get; set; }
	public string? Title { get; set; }
	public string Name { get; set; }
	public string? MimeType { get; set; }
	public DateTimeOffset? CreatedDateTime { get; set; }
	public DateTimeOffset? ModifieDateTime { get; set; }
	public DateTimeOffset? LastModifiedDateTime { get; set; }	
	public ParentInfo? Parent { get; set; }
	[NotMapped]
	[JsonProperty(PropertyName = "File.Hashes.Sha256Hash")]
	public string? Sha256Hash { get; set; }
	public Photo Photo { get; set; }	
	public Video Video { get; set; }
	[NotMapped]
	public Image Image { get; set; }
}

public partial class ParentInfo : ItemInfo
{
	public string DriveId { get; set; }	
}
