using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MissAlise.Application.Requests;
using MissAlise.Interfaces;


namespace MissAlise.Services.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ApiControllerBase : ControllerBase
	{
		protected void AddLinksHeaders(IPageInfo info, [CallerMemberName] string actionName = default)
		{
			if (info.HasNextPage)
			{
				var next = Url.Action(actionName, new PagedQuery { Page = info.Page + 1, PerPage = info.PerPage });
				Response.Headers.Append("Link", $"<{Request.Scheme}://{Request.Host}{next}>; rel=\"next\"");
			}

			if (info.HasPreviousPage)
			{
				var prev = Url.Action(actionName, new PagedQuery { Page = info.Page - 1, PerPage = info.PerPage });
				Response.Headers.Append("Link", $"<{Request.Scheme}://{Request.Host}{prev}>; rel=\"prev\"");
			}

			Response.Headers.Append("X-Total-Count", info.TotalCount.ToString());
			Response.Headers.Append("X-Total-Pages", info.TotalPages.ToString());
			Response.Headers.Append("X-Page-Number", info.Page.ToString());
			Response.Headers.Append("X-Page-Size", info.PerPage.ToString());
		}
	}
}
