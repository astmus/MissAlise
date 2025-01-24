using System.ComponentModel.DataAnnotations.Schema;

namespace MissAlise.Entities.OneDrive;

public partial class Item
{
	public virtual int Itemid { get; set; }

	public string? Title { get; set; }

	public int? Type { get; set; }

	public DateTimeOffset? Createddatetime { get; set; }

	public DateTimeOffset? Modifiedatetime { get; set; }
}
