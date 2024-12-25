using System.Linq;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace MissAlise.Entities.YandexTracker
{
	public abstract record Entity
	{
		protected static string typeId;
		public Entity()
			=> typeId ??= GetType().Name;

		public override string ToString()
		{
			if (SerializableItems() is IEnumerable<object> items)
				return $"{typeId} {string.Join(" | ", items)}";
			else
				return $"{typeId} {base.ToString()}";
		}

		protected virtual IEnumerable<object> SerializableItems()
			=> default;
	}

	public abstract record StringEntity : Entity<string>
	{ }

	public abstract record IntEntity : Entity<int>
	{ }

	public abstract record Entity<TIdentifier> : Entity
	{
		//      [JsonProperty("id")]		
		protected virtual TIdentifier Identifier { get; set; }

		//public virtual IEnumerable<object> GetValues()
		//{
		//    yield return Identifier;
		//}
	}
}