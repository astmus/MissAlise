using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using MissAlise.Application.Dto;
using MissAlise.Interfaces;

namespace MissAlise.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class UsersController : ControllerBase
{
	private readonly ILogger<UsersController> logger;
	private readonly IUserProfilesRepository usersProfiles;

	public UsersController(ILogger<UsersController> logger, IUserProfilesRepository usersProfiles)
	{
		this.logger = logger;
		this.usersProfiles = usersProfiles;
	}

	/// <summary>
	/// Список всех пользователей
	/// </summary>
	/// <returns></returns>
	[HttpGet]
	public async Task<IEnumerable<UserDto>> Get(CancellationToken cancel)
	{
		var users = await usersProfiles.AllAsync(cancel);
		Thread.Sleep(100000);
		cancel.ThrowIfCancellationRequested();
		return users.Select(user => new UserDto() { Id = user.Telegram.Id, DisplayName = user.Telegram.DisplayName }).ToArray();
	}

	/// <summary>
	/// Поиск пользователя по id
	/// </summary>
	/// <returns></returns>
	//[HttpGet("{id}", Name ="deruser")]
	//[ProducesResponseType(StatusCodes.Status200OK)]
	//[ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
	//[ProducesDefaultResponseType]
	//public async Task<ActionResult<UserDto>> GetUser(string id, CancellationToken cancel)
	//{
	//	var user = await usersProfiles.FindAsync(id, cancel);
	//	if (user != null)
	//		return Ok(user);

	//	return NotFound(id);
	//}
}
