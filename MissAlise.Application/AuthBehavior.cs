using MediatR;
using Microsoft.AspNetCore.Identity;
using MissAlise.Application.Abstractions;
using MissAlise.Application.Interfaces;
using MissAlise.Application.Models;
using MissAlise.Application.Services.Sync;
using MissAlise.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace MissAlise.Application
{
	public class AuthBehavior : IPipelineBehavior<SyncCommand, Result>
	{
		private readonly UserManager<AppUser> _manager;
		private readonly IHandleContext _ctx;
		private readonly SignInManager<AppUser> _signManager;

		public AuthBehavior(UserManager<AppUser> manager, IHandleContext ctx, SignInManager<AppUser> signManager)
		{
			_manager = manager;
			_ctx = ctx;
			_signManager = signManager;			
		}

		public async Task<Result> Handle(SyncCommand request, RequestHandlerDelegate<Result> next, CancellationToken cancellationToken)
		{
			var claimant = _ctx.Get<Claimant>("user.Id");
			var user = await _manager.Users.Include(user => user.AccessData).SingleOrDefaultAsync(user => user.Id == claimant.Id.ToString());


			if (user == null || user.EmailConfirmed == false)
				return Result.Fail<UnauthorizedAccessException>("Unauthorized user");
			else
				_ctx.Set(user);

			var result = await next();
			return result;
		}		
	}
}
