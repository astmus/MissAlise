namespace MissAlise.Application.Abstractions
{
	public class Result
	{
		public static readonly Result Successful = new Result();
		public bool Success => Error == default;        
		public string? Error { get; }		
		protected Result(string? error = null)
			=> Error = error;
				
		public static Result Fail(string error) => new(error);
		public static Result<T> Ok<T>(T value) => new(value, string.Empty);
		public static Result<T> Fail<T>(string error) => new(default, error);        
		public static Result<T> FromValue<T>(T? value) => value != null ? Ok(value) : Fail<T>("Provided value is null.");
	}

	public class Result<T> : Result
	{
		public T? Value { get; }
		internal Result(T? value, string error) : base(error)
			=> Value = value;		
		
		public static implicit operator Result<T>(T value) => FromValue(value);		
		public static implicit operator T?(Result<T> result) => result.Value;
	}
}
