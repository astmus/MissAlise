using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MissAlise.Entities.Identity;
using MissAlise.ValueObjects;
using MissAlise.ValueObjects.Identity;

namespace MissAlise.Interfaces
{
	public interface IOwnerResolver
	{
		Task<Guid> ResolveOwnerIdAsync(ExternalIdentity identity, CancellationToken ct);
	}
}
