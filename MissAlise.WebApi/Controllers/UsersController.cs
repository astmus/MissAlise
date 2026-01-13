using System.Net.Mime;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using MissAlise.Application.Dto;
using MissAlise.Application.Users.Requests;
using MissAlise.Background;
using MissAlise.Interfaces;

namespace MissAlise.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
[Produces(MediaTypeNames.Application.Json)]
public class UsersController : ControllerBase
{
	private readonly ILogger<UsersController> logger;
	private readonly IBackgroundJobRepository _jobs;
	private readonly IMediator _mm;

	public UsersController(ILogger<UsersController> logger, IBackgroundJobRepository jobs, IMediator mm)
	{
		this.logger = logger;
		_jobs = jobs;
		_mm = mm;
	}

	/// <summary>
	/// Список всех пользователей
	/// </summary>
	/// <returns></returns>
	[HttpGet]
	public Task<IEnumerable<User>> Get(CancellationToken cancel)
	{
		return Task.FromResult(Enumerable.Empty<User>());
	}

	[HttpGet("{userId:int}/jobs")]
	public async Task<IEnumerable<BackgroundJobDto>> GetTasks(int userId, CancellationToken cancel)
	{
		var users = await _jobs.AllJobsAsync(cancel);
		var res = await _mm.Send(new GetBackgroundJobsQuery(userId), cancel);		
		return res.Value;
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
