using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MissAlise.Entities.OneDrive;

public partial class Item
{
	[Key]
	[Column("Itemid")]
	public int Id { get; set; }

	public string? Name { get; set; }

	public string? MimeType { get; set; }

	public DateTimeOffset? CreatedDateTime { get; set; }

	public DateTimeOffset? ModifieDateTime { get; set; }
}
