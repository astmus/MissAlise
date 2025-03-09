using System.Collections;
using System.Collections.Specialized;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using Azure.Core;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

//using Microsoft.Graph;
using Microsoft.Kiota.Authentication.Azure;
using Microsoft.Kiota.Http.HttpClientLibrary;
using MissAlise.Application.Common;
using MissAlise.Application.Interfaces;
using MissAlise.Application.Services.Authentication;
using MissAlise.Entities.OneDrive;
using MissAlise.OneDrive.Auth;
using MissAlise.OneDrive.Drives.Item.Items.Item.Delta;
using MissAlise.OneDrive.Models;
using User = MissAlise.Entities.OneDrive.User;

namespace MissAlise.OneDrive
{
	internal class OneDriveService : IOneDriveService
	{
		private readonly AzureAd _azure;		
		private readonly IHandleContext _ctx;
		private readonly ApiClient _client;
		private readonly IOneDriveTokenService _tokenService;
		private readonly IAuthenticationService _manager;

		public OneDriveService(IOptions<AzureAd> azure, ApiClient client, IHandleContext ctx, IOneDriveTokenService tokenService, IAuthenticationService manager)
		{
			_azure = azure.Value;
			_ctx = ctx;
			_client = client;
			_tokenService = tokenService;
			_manager = manager;
		}

		public async Task<User> GetOwnerInfo(CancellationToken cancel)
		{
			var childrenRequest = _client.Drives["Me"].Items["Root"].Delta;
			var items = await childrenRequest.GetAsDeltaGetResponseAsync<DriveDeltaItems>(r => r.QueryParameters.Select = "Id Name File FileSystemInfo Video Photo Size ParentReference".ToLower().Split(' '),cancellationToken: cancel);			
			PageIterator<DriveItem, DriveDeltaItems> iterator = null;

			int varo = 0;
			StringBuilder sb = new StringBuilder();

				var list = await _client.Drives["Me"].Items["Root"].ListItem.GetAsync();
				iterator = PageIterator<DriveItem, DriveDeltaItems>.CreatePageIterator(_client.Adapter, items, callback:
				item =>
				{
					varo++;
					sb.AppendLine(item.Name + " state" + item.Deleted?.State);
					return true;
				}
				//}, requestConfigurator: request =>
				//{
				//	return request;
				//}
				);

			await iterator.IterateAsync(cancel);
			string str = sb.ToString();
			//var items = await oneDrive.RootChildren(ctx, cancel);
			//var me = await client.Me.GetAsync(cancellationToken: cancel);
			//if (me != null)
			//	return new User() { Id = me.Id, DisplayName = me.DisplayName, GivenName = me.GivenName, Mail = me.Mail, PreferredLanguage = me.PreferredLanguage, Surname = me.Surname };

			return null;
		}

		public Task<IEnumerable<ItemInfo>> GetRootItems(CancellationToken cancel)
		{			
			return default;
		}

		public Uri CreateAuthorizeLink(object stateIdentifier)
		{
			UriBuilder builder = new UriBuilder(_azure.AuthPath);
			var query = HttpUtility.ParseQueryString(builder.Query);
			query["scope"] = _azure.Scopes;
			query["client_id"] = _azure.ClientId;
			query["response_type"] = "code";
			query["redirect_uri"] = _azure.RedirectUri + _azure.CallbackPath;
			query["prompt"] = "select_account";
			query["state"] = stateIdentifier.ToString();
			builder.Query = query.ToString();
			return builder.Uri;
		}		

		public async Task<Result<AppUser>> RefreshUserAccessTokenAsync(AppUser user, CancellationToken cancel)
		{
			var response = await _tokenService.RefreshCredentials(_azure, user.AccessData.RefreshToken, cancel).ConfigureAwait(false);
			if (response.IsSuccessful)
			{
				user.AccessData = response.Content;
				user.AccessData.ExpiredAfter = DateTimeOffset.UtcNow.AddSeconds(response.Content.ExpiresIn);
				var result = await _manager.UpdateUserAsync(user).ConfigureAwait(false);
				return user;
			}
			else
				return Result.Fail<AppUser>(response.Error.Content);
		}
	}

	internal class DriveDataContext : OneDriveQueryable<DriveItem>
	{
		public DriveDataContext(ODataQueryProvider provider) : base(provider)
		{
		}

		public DriveDataContext(ODataQueryProvider provider, Expression? expression = null) : base(provider, expression)
		{
		}
	}

	internal class OneDriveQueryable<T> : IOrderedQueryable<T>
	{
		protected readonly Expression expression;
		protected ODataQueryProvider provider;
		public OneDriveQueryable(ODataQueryProvider provider)
		{
			this.provider = provider;
			this.expression = Expression.Constant(this);
		}

		public OneDriveQueryable(ODataQueryProvider provider, Expression? expression = null)
		{
			this.provider = provider;
			this.expression = expression ?? Expression.Constant(this);
		}

		public Type ElementType => typeof(T);
		public Expression Expression => expression;
		public IQueryProvider Provider => provider;
		public IEnumerator<T> GetEnumerator()
		{
			return provider.Execute<IEnumerable<T>>(Expression).GetEnumerator();
		}
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public class ODataVisitor : ExpressionVisitor
	{
		public string QueryString
		{
			get
			{
				var query = new StringBuilder(128);
				foreach (string key in oData.Keys)
				{
					//query.Append(key + "=");
					query.AppendJoin(",", oData.GetValues(key));
					query.Append("&");
				}
				query.Length -= 1;
				return query.ToString();
			}
		}
		NameValueCollection oData = new NameValueCollection();

		protected override Expression VisitMember(MemberExpression node)
		{
			oData.Add("$" + node.Expression.ToString(), node.Member.Name);
			return base.VisitMember(node);
		}
		protected override Expression VisitNew(NewExpression node)
			=> base.VisitNew(node);

		protected override Expression VisitParameter(ParameterExpression node)
			=> base.VisitParameter(node);
		protected override Expression VisitLabel(LabelExpression node)
			=> base.VisitLabel(node);
		protected override Expression VisitExtension(Expression node)
			=> base.VisitExtension(node);
		protected override Expression VisitUnary(UnaryExpression node)
			=> base.VisitUnary(node);
		protected override Expression VisitConditional(ConditionalExpression node)
			=> base.VisitConditional(node);
		protected override Expression VisitLambda<T>(Expression<T> node)
			=> base.VisitLambda(node);
		protected override MemberBinding VisitMemberBinding(MemberBinding node)
			=> base.VisitMemberBinding(node);
		protected override MemberMemberBinding VisitMemberMemberBinding(MemberMemberBinding node)
			=> base.VisitMemberMemberBinding(node);
		protected override Expression VisitMethodCall(MethodCallExpression node)
			=> base.VisitMethodCall(node);

		protected override Expression VisitBlock(BlockExpression node)
			=> base.VisitBlock(node);
		protected override Expression VisitConstant(ConstantExpression node)
			=> base.VisitConstant(node);
		protected override Expression VisitDebugInfo(DebugInfoExpression node)
			=> base.VisitDebugInfo(node);
		protected override Expression VisitDefault(DefaultExpression node)
			=> base.VisitDefault(node);
		protected override Expression VisitGoto(GotoExpression node)
			=> base.VisitGoto(node);
		protected override Expression VisitInvocation(InvocationExpression node)
			=> base.VisitInvocation(node);
		protected override LabelTarget? VisitLabelTarget(LabelTarget? node)
			=> base.VisitLabelTarget(node);
		protected override Expression VisitLoop(LoopExpression node)
			=> base.VisitLoop(node);
		protected override Expression VisitIndex(IndexExpression node)
			=> base.VisitIndex(node);
		protected override Expression VisitNewArray(NewArrayExpression node)
			=> base.VisitNewArray(node);
		protected override Expression VisitRuntimeVariables(RuntimeVariablesExpression node)
			=> base.VisitRuntimeVariables(node);
		protected override SwitchCase VisitSwitchCase(SwitchCase node)
			=> base.VisitSwitchCase(node);
		protected override Expression VisitSwitch(SwitchExpression node)
			=> base.VisitSwitch(node);
		protected override CatchBlock VisitCatchBlock(CatchBlock node)
			=> base.VisitCatchBlock(node);
		protected override Expression VisitTry(TryExpression node)
			=> base.VisitTry(node);
		protected override Expression VisitTypeBinary(TypeBinaryExpression node)
			=> base.VisitTypeBinary(node);
		protected override Expression VisitMemberInit(MemberInitExpression node)
			=> base.VisitMemberInit(node);
		protected override Expression VisitListInit(ListInitExpression node)
			=> base.VisitListInit(node);
		protected override ElementInit VisitElementInit(ElementInit node)
			=> base.VisitElementInit(node);
		protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
			=> base.VisitMemberAssignment(node);
		protected override MemberListBinding VisitMemberListBinding(MemberListBinding node)
			=> base.VisitMemberListBinding(node);
		protected override Expression VisitDynamic(DynamicExpression node)
			=> base.VisitDynamic(node);
	}

	internal class ODataQueryProvider : IQueryProvider
	{
		public IQueryable CreateQuery(Expression expression)
		{
			ArgumentNullException.ThrowIfNull(expression);

			if (!typeof(IQueryable).IsAssignableFrom(expression.Type))
				throw new ArgumentException(nameof(expression));

			return new DriveDataContext(this, expression);
		}

		public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
		{
			ArgumentNullException.ThrowIfNull(expression);

			if (!typeof(IQueryable<TElement>).IsAssignableFrom(expression.Type))
				throw new ArgumentException(nameof(expression));
			return new OneDriveQueryable<TElement>(this, expression);
		}

		public object Execute(Expression expression)
		{
			return Execute<DriveItem>(expression);
		}

		public TElement Execute<TElement>(Expression expression)
		{
			ODataVisitor vv = new ODataVisitor();
			vv.Visit(expression);
			var q = vv.QueryString;
			return default; //Activator.CreateInstance<TElement>();
		}
	}
}
