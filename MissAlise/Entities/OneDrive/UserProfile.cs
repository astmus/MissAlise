namespace MissAlise.Entities.OneDrive
{
	public class UserProfile : User
	{
		public User OneDrive { get; set; }
		public User Telegram { get; set; }
		public Credentials AccessData { get; set; }
	}
}
