using MissAlise.Entities.OneDrive;

namespace MissAlise.Interfaces
{
	public interface IUserStorage
	{
		public User? Owner { get; }
		public Folder RootFolder { get; set; }
		Func<Task<int>> SaveAsync { get; }
	}
}
