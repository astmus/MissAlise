namespace MissAlise.Utils;

public static class Identity<TType>
{
	public static readonly Type Type;
	public static readonly string Discriminator;	

	static Identity()
	{
		Type = typeof(TType);
		ArgumentNullException.ThrowIfNullOrEmpty(Discriminator = Type.FullName ?? string.Empty);
		Discriminator = Type.Name;
	}
}