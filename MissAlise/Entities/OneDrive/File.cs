using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace MissAlise.Entities.OneDrive;

public partial class File : Item
{
	public virtual int Fileid { get; set; }

	public string? Name { get; set; }

	public int? Size { get; set; }

	public string? Mimetype { get; set; }

	public string? Extension { get; set; }

	public Folder? Folder { get; set; }
}
