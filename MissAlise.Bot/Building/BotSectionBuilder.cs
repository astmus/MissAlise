namespace MissAlise.TelegramBot.Building;

public sealed partial class BotBuilder
{
	internal sealed class BotSectionBuilder<TParent> : IBotSectionBuilder<TParent>
	{
		private readonly TParent _parent;
		private readonly BotBuilder _builder;

		public BotSectionBuilder(TParent parent, BotBuilder builder)
		{
			_parent = parent;
			_builder = builder;
		}

		public IBotSectionBuilder<T> BeginScope<T>(string name, string description)
		{			
			return _builder.BeginScope<T>(name, description);
		}

		public TParent EndSection()
		{
			_builder.EndSection();
			return _parent;
		}

		public IBotBuilder EndScope<TSection>()
		{
			return _builder.EndSection();
		}

		public IBotSectionBuilder<IBotSectionBuilder<TParent>> BeginSection<T>(string name, string description) where T : class
		{
			_builder.BeginScope<T>(name, description);

			IBotSectionBuilder<TParent> thisTyped = this;
			return new BotSectionBuilder<IBotSectionBuilder<TParent>>(thisTyped, _builder);
		}
		
		public IBotSectionBuilder<TParent> AddCommand<T>(string name, string description) where T : class
		{
			_builder.AddCommand<T>(name, description);			
			return this;
		}
	}
}