using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MissAlise.Entities.YandexTracker
{
	public partial record Attachment : StringEntity
	{
		[JsonProperty("self")]
		public Uri Self { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("content")]
		public Uri Content { get; set; }

		[JsonProperty("createdBy")]
		public ViewEntity CreatedBy { get; set; }

		[JsonProperty("createdAt")]
		public string CreatedAt { get; set; }

		[JsonProperty("mimetype")]
		public string Mimetype { get; set; }

		[JsonProperty("size")]
		public long Size { get; set; }
	}
}
