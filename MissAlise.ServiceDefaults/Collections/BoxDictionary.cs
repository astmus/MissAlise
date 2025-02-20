using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
#nullable disable
namespace MissAlise.Collections
{
	public abstract record Box : IStrongBox // если посмотреть нативную реализацию этого интерфейса то там тоже самое
	{
		public object Value { get; set; }
		public static Box<T> Create<T>(ref T value)
			=> new Box<T>(value);
		public static Box<T> Create<T>(T value)
			=> new Box<T>(value);

		public abstract T Get<T>();
		public abstract object Unbox();
		public string String() => Get<string>();
		public TimeSpan TimeSpan() => Get<TimeSpan>();
		public DateTime DateTime() => Get<DateTime>();
		public double Double() => Get<double>();
		public int Integer() => Get<int>();

		public static implicit operator Box(string value)
			=> Create(ref value);
		public static implicit operator Box(TimeSpan value)
			=> Create(ref value);
		public static implicit operator Box(DateTime value)
			=> Create(ref value);
		public static implicit operator Box(int value)
			=> Create(ref value);
		public static implicit operator Box(double value)
			=> Create(ref value);
	}

	public record Box<T> : Box
	{
		public Box(T val)
			=> value = val;
		private T value;
		public T Content { get => value; set => this.value = value; }

		//public static implicit operator Box<T>(T value) // опасная штука, пусть пока так побудет пока точно не пригодится
		//	=> new Box<T>(value);

		public override V Get<V>()
			=> value is V res ? res : default;
		public override object Unbox()
			=> value;
		public static implicit operator Box<T>(T value)
			=> Create(ref value);
	}

	public class BoxDictionary : Dictionary<string, Box>
	{
		#region ctors
		public BoxDictionary()
		{
		}

		public BoxDictionary(IDictionary<string, Box> dictionary) : base(dictionary)
		{
		}

		public BoxDictionary(IDictionary<string, Box> dictionary, IEqualityComparer<string> comparer) : base(dictionary, comparer)
		{
		}

		public BoxDictionary(IEnumerable<KeyValuePair<string, Box>> collection) : base(collection)
		{
		}

		public BoxDictionary(IEnumerable<KeyValuePair<string, Box>> collection, IEqualityComparer<string> comparer) : base(collection, comparer)
		{
		}

		public BoxDictionary(IEqualityComparer<string> comparer) : base(comparer)
		{
		}

		public BoxDictionary(int capacity) : base(capacity)
		{
		}

		public BoxDictionary(int capacity, IEqualityComparer<string> comparer) : base(capacity, comparer)
		{
		}
		#endregion

		public bool Set<T>(T value, [CallerMemberName] string key = null)
		{
			if (TryAdd(key, new Box<T>(value)) == false)
			{
				if (TryGetValue(key, out var refBox) && refBox is Box<T> box)
				{
					box.Content = value;
					return true;
				}
				return false;
			}
			return true;
		}

		public T Get<T>([CallerMemberName] string key = null, T def = default)
		{
			if (TryGetValue(key, out var r) && r is Box reference)
				return reference.Get<T>();
			return def;
		}
	}

	public class BoxDictionary<T> : Dictionary<T, Box>
	{
		public bool Set<V>(T key, V value)
		{
			//тут чего то не хватает потом подумать чего
			if (TryAdd(key, new Box<V>(value)) == false)
				if (TryGetValue(key, out var refBox) && refBox is Box<V> box)
				{
					box.Content = value;
					return true;
				}
			return false;
		}

		public V Get<V>(T key = default)
		{
			if (TryGetValue(key, out var r) && r is Box<V> reference)
				return reference.Content;
			return default;
		}
	}
}
#nullable restore