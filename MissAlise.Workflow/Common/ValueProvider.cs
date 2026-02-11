using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MissAlise.Workflow.Common;
public abstract class ValueProvider
{
	public abstract IEnumerable<object> GetValues();
}

public class StepValueProvider<T> : ValueProvider where T : IComparable,
		  IComparable<T>,
		  IEquatable<T>,
		  ISpanFormattable,
		  ISpanParsable<T>
{
	private readonly string _format;

	public StepValueProvider(T value, T min, T max, T diff, string format)
	{
		Value = value;
		Min = min;
		Max = max;
		Diff = diff;
		_format = format;
	}

	public T Value { get; set; }
	public T Min { get; }
	public T Max { get; }
	public T Diff { get; }

	public string Format(T reference)
	{
		if(reference.CompareTo(Min) >= 0 && reference.CompareTo(Max) <= 0)
			return string.Format(_format, reference);
		else
			return string.Format(_format, Value);
	}

	public override IEnumerable<object> GetValues()
	{
		yield return Value;
		yield return Min;
		yield return Max;
	}
}
