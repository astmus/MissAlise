//using System;
//using System.Collections.Generic;
//using System.CommandLine;
//using System.Linq;
//using System.Reflection;

//namespace MissAlise.TelegramBot.CommandLine;

///// <summary>
///// Регистрация команд/подкоманд через BotCommandDescription и генерация RootCommand.
///// </summary>
//public sealed class BotBuilder
//{
//    private readonly BotCommandDescription _root = new() { Name = "root" };

//    public BotCommandDescription Root => _root;

//    public BotBuilder AddCommand<T>(string name, string? description = null)
//        where T : class
//    {
//        var cmd = CreateCommand(typeof(T), name, description);
//        _root.SubCommands.Add(cmd);
//        return this;
//    }

//    public BotBuilder AddSubCommand<T>(BotCommandDescription parent, string name, string? description = null)
//        where T : class
//    {
//        var cmd = CreateCommand(typeof(T), name, description);
//        parent.SubCommands.Add(cmd);
//        return this;
//    }

//    public BotDefinition Build()
//    {
//        var leafMap = new Dictionary<Command, BotDefinition.LeafRuntime>();
//        var leafByPath = new Dictionary<string, BotCommandDescription>(StringComparer.OrdinalIgnoreCase);

//        var rootCmd = new RootCommand();
//        foreach (var c in _root.SubCommands)
//            rootCmd.AddCommand(BuildCommand(c, leafMap, leafByPath, parentPath: null));

//        return new BotDefinition(rootCmd, _root, leafMap, leafByPath);
//    }

//    private Command BuildCommand(
//        BotCommandDescription d,
//        Dictionary<Command, BotDefinition.LeafRuntime> leafMap,
//        Dictionary<string, BotCommandDescription> leafByPath,
//        string? parentPath)
//    {
//        var cmd = new Command(d.Name, d.Description);

//        // Telegram-first: allow "/name" alias for top-level commands.
//        cmd.AddAlias("/" + d.Name);

//        var optMap = new Dictionary<string, Option>(StringComparer.OrdinalIgnoreCase);
//        foreach (var p in d.Parameters)
//        {
//            var opt = CreateOption(p);
//            cmd.AddOption(opt);
//            optMap[p.Name] = opt;
//        }

//        var path = parentPath is null ? d.Name : parentPath + " " + d.Name;

//        if (d.CommandType is not null)
//        {
//            leafMap[cmd] = new BotDefinition.LeafRuntime
//            {
//                Description = d,
//                Command = cmd,
//                Path = path,
//                OptionsByParamKey = optMap
//            };

//            leafByPath[BotDefinition.NormalizePath(path)] = d;
//        }

//        foreach (var s in d.SubCommands)
//            cmd.AddCommand(BuildCommand(s, leafMap, leafByPath, path));

//        return cmd;
//    }

//    private BotCommandDescription CreateCommand(Type type, string name, string? description)
//    {
//        var desc = new BotCommandDescription
//        {
//            Name = name,
//            Description = description,
//            CommandType = type
//        };

//        foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
//        {
//            if (!p.CanRead) continue;

//            // Convention: property -> option
//            var isNullable = Nullable.GetUnderlyingType(p.PropertyType) != null;
//            var coreType = Nullable.GetUnderlyingType(p.PropertyType) ?? p.PropertyType;

//            desc.Parameters.Add(new BotParameterDescription(
//                Name: p.Name,
//                CliName: "--" + ToKebab(p.Name),
//                ValueType: p.PropertyType,
//                IsRequired: !isNullable && coreType != typeof(bool),
//                IsOption: true,
//                Description: null,
//                AllowedValues: coreType.IsEnum ? Enum.GetNames(coreType) : null,
//                InlineProviderKey: null
//            ));
//        }

//        return desc;
//    }

//    private static Option CreateOption(BotParameterDescription p)
//    {
//        var (coreType, _) = UnwrapNullable(p.ValueType);

//        if (TryGetCollectionElementType(coreType, out var elementType))
//        {
//            var arrayType = elementType!.MakeArrayType();

//            var opt = (Option)Activator.CreateInstance(
//                typeof(Option<>).MakeGenericType(arrayType),
//                new object?[] { p.CliName, p.Description })!;

//            opt.IsRequired = p.IsRequired;
//            opt.AllowMultipleArgumentsPerToken = true;
//            opt.Arity = p.IsRequired ? ArgumentArity.OneOrMore : ArgumentArity.ZeroOrMore;
//            return opt;
//        }

//        if (coreType == typeof(bool))
//        {
//            var opt = new Option<bool>(p.CliName, description: p.Description);
//            opt.IsRequired = p.IsRequired;
//            return opt;
//        }

//        if (coreType.IsEnum)
//        {
//            var opt = (Option)Activator.CreateInstance(
//                typeof(Option<>).MakeGenericType(coreType),
//                new object?[] { p.CliName, p.Description })!;

//            opt.IsRequired = p.IsRequired;

//            // CLI-side restriction (helps for manual typing). Works for Option<TEnum>.
//            var allowed = p.AllowedValues?.Count > 0 ? p.AllowedValues.ToArray() : Enum.GetNames(coreType);
//            var mi = opt.GetType().GetMethod("FromAmong", new[] { typeof(string[]) });
//            mi?.Invoke(opt, new object?[] { allowed });

//            return opt;
//        }

//        {
//            var opt = (Option)Activator.CreateInstance(
//                typeof(Option<>).MakeGenericType(coreType),
//                new object?[] { p.CliName, p.Description })!;
//            opt.IsRequired = p.IsRequired;
//            return opt;
//        }
//    }

//    private static (Type CoreType, bool IsNullable) UnwrapNullable(Type type)
//    {
//        var underlying = Nullable.GetUnderlyingType(type);
//        return underlying is null ? (type, false) : (underlying, true);
//    }

//    private static bool TryGetCollectionElementType(Type type, out Type? elementType)
//    {
//        elementType = null;
//        if (type == typeof(string)) return false;

//        if (type.IsArray)
//        {
//            elementType = type.GetElementType();
//            return elementType is not null;
//        }

//        if (type.IsGenericType)
//        {
//            var def = type.GetGenericTypeDefinition();
//            if (def == typeof(List<>) || def == typeof(IReadOnlyList<>) || def == typeof(IEnumerable<>))
//            {
//                elementType = type.GetGenericArguments()[0];
//                return true;
//            }
//        }

//        var ienum = type.GetInterfaces()
//            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
//        if (ienum is not null)
//        {
//            elementType = ienum.GetGenericArguments()[0];
//            return elementType != typeof(char);
//        }

//        return false;
//    }

//    private static string ToKebab(string s)
//        => string.Concat(s.Select((c, i) => i > 0 && char.IsUpper(c) ? "-" + char.ToLower(c) : char.ToLower(c).ToString()));
//}
