using System.ComponentModel.DataAnnotations.Schema;

namespace MissAlise.DataBase.Models;

public partial class Item
{
	public virtual int Itemid { get; set; }

	public string? Title { get; set; }

	public int? Type { get; set; }

	public DateTime? Createddatetime { get; set; }

	public DateTime? Modifiedatetime { get; set; }	
}
