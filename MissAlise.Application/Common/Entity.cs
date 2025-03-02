namespace MissAlise.Application.Common
{
	public abstract class Entity
	{
		public int Id { get; set; }
		public DateTime DateCreated { get; set; }
		public DateTime? DateUpdated { get; set; }
	}
}
