using MissAlise.Entities.OneDrive;
using MongoDB.Bson;

namespace MissAlise.DataBase.Models
{
	internal class DbUserProfile : UserProfile
	{
		public ObjectId Id { get; set; }
	}
}
