using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissAlise.ValueObjects.Identity
{
	public sealed record ExternalIdentity(string Scheme, string ExternalId)
	{
		public override string ToString() => $"{Scheme}:{ExternalId}";
	}
}
