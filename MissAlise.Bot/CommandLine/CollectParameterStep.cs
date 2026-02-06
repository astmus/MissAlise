using MissAlise.Workflow;
using MissAlise.Workflow.Steps;

namespace MissAlise.TelegramBot.CommandLine;

/// <summary>Single step for CLI parameter collection: show prompt for next param, accept text or callback, store and advance or produce command.</summary>
public sealed class CollectParameterStep : IWorkflowStep
{
	private readonly IBotCommandLineParser _parser;
	private readonly IBotCommandFactory _factory;

	public CollectParameterStep(IBotCommandLineParser parser, IBotCommandFactory factory)
	{
		_parser = parser ?? throw new ArgumentNullException(nameof(parser));
		_factory = factory ?? throw new ArgumentNullException(nameof(factory));
	}

	public Task<WorkflowStepResult> ExecuteAsync(
		WorkflowContext context,
		WorkflowSession session,
		WorkflowInput input,
		CancellationToken cancellationToken)
	{
		var commandPath = session.Get(CliParameterCollectionState.CommandPath);
		var missingStr = session.Get(CliParameterCollectionState.MissingList);
		if (string.IsNullOrEmpty(commandPath) || string.IsNullOrEmpty(missingStr))
			return Task.FromResult(WorkflowStepResult.Stay());

		var missing = missingStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
		var idx = session.GetInt(CliParameterCollectionState.CurrentIndex, 0);
		if (idx < 0 || idx >= missing.Count)
			return Task.FromResult(WorkflowStepResult.Stay());

		var currentParam = missing[idx];
		string? value = null;

		if (input.Kind == WorkflowInputKind.Text && !string.IsNullOrWhiteSpace(input.Text))
			value = input.Text.Trim();
		else if (input.Kind == WorkflowInputKind.Callback && !string.IsNullOrWhiteSpace(input.Payload))
		{
			// Payload can be "param:value" or just "value" for current param
			var p = input.Payload.Trim();
			if (p.StartsWith(currentParam + ":", StringComparison.OrdinalIgnoreCase))
				value = p[(currentParam.Length + 1)..].Trim();
			else
				value = p;
		}

		if (value != null)
		{
			session.Set(CliParameterCollectionState.ValuePrefix + currentParam, value);
			session.Set(CliParameterCollectionState.CurrentIndex, (idx + 1).ToString());

			if (idx + 1 >= missing.Count)
			{
				var collected = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				foreach (var m in missing)
				{
					var v = session.Get(CliParameterCollectionState.ValuePrefix + m);
					if (v != null) collected[m] = v;
				}
				var command = _factory.CreateCommand(commandPath, collected, context);
				if (command != null)
					return Task.FromResult(WorkflowStepResult.Produce(command));
			}
		}

		return Task.FromResult(WorkflowStepResult.Stay());
	}
}
