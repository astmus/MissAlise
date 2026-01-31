using MissAlise.Entities.Media;
using MissAlise.ValueObjects;
using MissAlise.ValueObjects.Media;

namespace MissAlise.Interfaces
{
	public interface IUserStorage
	{
		public UserId? Owner { get; }
		public MediaPath RootFolder { get; set; }
		Func<Task<int>> SaveAsync { get; }
	}
}
