using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace MissAlise.Entities.YandexTracker
{
	public partial class Changelog
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("self")]
		public Uri Self { get; set; }

		[JsonProperty("issue")]
		public Issue Issue { get; set; }

		[JsonProperty("updatedAt")]
		public string UpdatedAt { get; set; }

		[JsonProperty("updatedBy")]
		public ViewEntity UpdatedBy { get; set; }

		[JsonProperty("type")]
		public string Type { get; set; }

		[JsonProperty("transport")]
		public string Transport { get; set; }

		[JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
		public Field[] Fields { get; set; }

		[JsonProperty("attachments", NullValueHandling = NullValueHandling.Ignore)]
		public Attachments Attachments { get; set; }

		[JsonProperty("links", NullValueHandling = NullValueHandling.Ignore)]
		public Link[] Links { get; set; }
	}

	public partial class Attachments
	{
		[JsonProperty("added")]
		public Issue[] Added { get; set; }
	}


	public partial class Field
	{
		[JsonProperty("field")]
		public Issue FieldField { get; set; }

		[JsonProperty("from")]
		public FromUnion From { get; set; }

		[JsonProperty("to")]
		public FromUnion To { get; set; }
	}

	public partial class TypeClass
	{
		[JsonProperty("self")]
		public Uri Self { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("inward")]
		public string Inward { get; set; }

		[JsonProperty("outward")]
		public string Outward { get; set; }
	}

	public partial struct FromUnion
	{
		public FromElement[] FromElementArray;
		public Issue Issue;

		public static implicit operator FromUnion(FromElement[] FromElementArray) => new FromUnion { FromElementArray = FromElementArray };
		public static implicit operator FromUnion(Issue Issue) => new FromUnion { Issue = Issue };
		public bool IsNull => FromElementArray == null && Issue == null;
	}

	internal static class Converter
	{
		public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
		{
			MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
			DateParseHandling = DateParseHandling.None,
			Converters =
			{
				FromUnionConverter.Singleton,
				new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
			},
		};
	}

	internal class FromUnionConverter : JsonConverter
	{
		public override bool CanConvert(Type t) => t == typeof(FromUnion) || t == typeof(FromUnion?);

		public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
		{
			switch (reader.TokenType)
			{
				case JsonToken.Null:
				return new FromUnion { };
				case JsonToken.StartObject:
				var objectValue = serializer.Deserialize<Issue>(reader);
				return new FromUnion { Issue = objectValue };
				case JsonToken.StartArray:
				var arrayValue = serializer.Deserialize<FromElement[]>(reader);
				return new FromUnion { FromElementArray = arrayValue };
			}
			throw new Exception("Cannot deserialize type FromUnion");
		}

		public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
		{
			var value = (FromUnion)untypedValue;
			if (value.IsNull)
			{
				serializer.Serialize(writer, null);
				return;
			}
			if (value.FromElementArray != null)
			{
				serializer.Serialize(writer, value.FromElementArray);
				return;
			}
			if (value.Issue != null)
			{
				serializer.Serialize(writer, value.Issue);
				return;
			}
			throw new Exception("Cannot marshal type FromUnion");
		}

		public static readonly FromUnionConverter Singleton = new FromUnionConverter();
	}
}
