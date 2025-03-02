using MediatR;
using Microsoft.AspNetCore.Identity;
using MissAlise.Application.Interfaces;
using MissAlise.Application.Services.Sync;
using MissAlise.Interfaces;
using Microsoft.EntityFrameworkCore;
using MissAlise.Application.Common;
using MissAlise.Application.Services.Authentication;
namespace MissAlise.Application
{
	public class AuthBehavior : IPipelineBehavior<SyncCommand, Result>
	{
		private readonly IHandleContext _ctx;
		private readonly IAuthenticationService _authService;
		private readonly IOneDriveService _oneDrive;

		public AuthBehavior(IHandleContext ctx, IAuthenticationService authService, IOneDriveService oneDrive)
		{
			_ctx = ctx;
			_authService = authService;
			_oneDrive = oneDrive;
		}

		public async Task<Result> Handle(SyncCommand request, RequestHandlerDelegate<Result> next, CancellationToken cancellationToken)
		{
			var claimant = _ctx.Get<Claimant>();
			var appUser = await _authService.LoginUserAsync(claimant.Id);
			if (appUser == null || appUser.EmailConfirmed == false)
				return Result.Fail<UnauthorizedAccessException>("Unauthorized user");

			if (appUser.HasExpiredCredentials())
			{
				var response = await _oneDrive.RefreshUserAccessTokenAsync(appUser, cancellationToken);
			}

			_ctx.Set(appUser);
			var result = await next();

			return result;
		}
	}
}
