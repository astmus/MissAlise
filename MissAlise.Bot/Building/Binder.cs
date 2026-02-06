
using System;
using System.CommandLine.Parsing;
using System.Linq;
using System.Reflection;

namespace BotDsl;

public static class Binder
{
    public static object Bind(Type t, ParseResult result)
    {
        var obj = Activator.CreateInstance(t)!;
        foreach (var p in t.GetProperties())
        {
            var opt = result.CommandResult.Command.Options
                .FirstOrDefault(o => o.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase));
            if (opt == null) continue;
            var val = result.GetValueForOption(opt);
            p.SetValue(obj, val);
        }
        return obj;
    }
}
