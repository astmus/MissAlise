using MediatR;
using MissAlise.Application.Commands;
using MissAlise.Application.Common;
using MissAlise.Application.Dto;
using MissAlise.Application.Interfaces;
using MissAlise.Entities.Identity;
using MissAlise.Interfaces;
using MissAlise.ValueObjects;
using MissAlise.ValueObjects.Identity;

namespace MissAlise.Application
{
	public class AuthBehavior : IPipelineBehavior<SyncCommand, Result>
	{
		private readonly IHandleContext _ctx;
		private readonly IUserProfilesRepository _userProfiles;
		private readonly IOneDriveService _oneDrive;
		private readonly IAccessCredentialsStore _accessStore;
		private readonly IOwnerResolver _ownerResolver;

		public AuthBehavior(IHandleContext ctx, IUserProfilesRepository authService, IOneDriveService oneDrive, IAccessCredentialsStore accessStore, IOwnerResolver ownerResolver)
		{
			_ctx = ctx;
			_userProfiles = authService;
			_oneDrive = oneDrive;
			_accessStore = accessStore;
			_ownerResolver = ownerResolver;
		}

		public async Task<Result> Handle(SyncCommand request, RequestHandlerDelegate<Result> next, CancellationToken cancellationToken)
		{
			UserId userId = default;
			if (request.Value == default)
			{
				var sender = _ctx.Get<UserProfile>();
				if (sender is null)
					return Result.Fail<UnauthorizedAccessException>("User context not found");

				var identity = sender.Identities.FirstOrDefault(i => i.Scheme == "telegram");
				userId = new UserId(await _ownerResolver.ResolveOwnerIdAsync(identity, cancellationToken));
			}

			var profile = await _userProfiles.FindByOwnerIdAsync(userId.Value, cancellationToken);
			if (profile is null)
				return Result.Fail<UnauthorizedAccessException>("User profile not found");

			var access = await _accessStore.GetAsync(userId, cancellationToken);
			if (access is null)
				return Result.Fail<UnauthorizedAccessException>("OneDrive not linked. Complete OAuth first.");

			if (access.IsExpired())
			{
				var refreshResult = await _oneDrive.RefreshUserAccessTokenAsync(userId, access, cancellationToken);
				if (!refreshResult.Success || refreshResult.Value is null)
					return Result.Fail<UnauthorizedAccessException>(refreshResult.Error ?? "Token refresh failed");
				access = refreshResult.Value;
			}

			_ctx.Set(new SyncPrincipal(profile, access));
			return await next();
		}
	}
}
