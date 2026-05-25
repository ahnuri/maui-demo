namespace HannaUIDemo.Features.Instruments;

/// <summary>Resolves log detail navigators by <see cref="InstrumentKind"/>.</summary>
public sealed class InstrumentLogNavigatorHost
{
	readonly IReadOnlyDictionary<InstrumentKind, Abstractions.IInstrumentLogNavigator> _navigators;

	public InstrumentLogNavigatorHost(IEnumerable<Abstractions.IInstrumentLogNavigator> navigators) =>
		_navigators = navigators.ToDictionary(n => n.Kind);

	public Abstractions.IInstrumentLogNavigator Get(InstrumentKind kind) => _navigators[kind];
}
