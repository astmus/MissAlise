using LogicBuilder.Expressions.Utils.ExpressionDescriptors;

namespace MissAlise.TelegramBot.Building;

public interface IBotBuilder : IBotCommandContainer
{
	IBotBuilder AddCommand<T>(string name, string description, string? title = null) where T : class;
}

public interface IBotCommandContainer
{
	IBotSectionBuilder<TParent> BeginScope<TParent>(string name, string description, string? title = null);
}

public interface IBotSectionBuilder<out TParent> : IBotCommandContainer
{
	internal TParent EndSection();

	IBotBuilder EndScope<TSection>();

	IBotSectionBuilder<IBotSectionBuilder<TParent>> BeginSection<TSection>(string name, string description, string? title = null) where TSection : class;

	IBotSectionBuilder<TParent> AddCommand<T>(string name, string description) where T : class;
}

public static class BotBuilderExtensions
{
	public static IBotSectionBuilder<TParent> EndSection<TParent>(this IBotSectionBuilder<IBotSectionBuilder<TParent>> section)
	{ 
		return section.EndSection();
	}
}
