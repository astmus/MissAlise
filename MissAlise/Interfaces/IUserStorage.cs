using MissAlise.Entities.OneDrive;

namespace MissAlise.Interfaces
{
	public interface IUserStorage
	{
		public IOneDriveUser Owner { get; }
		public Folder RootFolder { get; set; }
		Func<Task<int>> SaveAsync { get; }
	}
}
