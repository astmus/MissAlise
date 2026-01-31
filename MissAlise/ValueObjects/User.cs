using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissAlise.ValueObjects
{
	public readonly record struct UserId(Guid Value);
}
