using HannaUIDemo.Core.Devices;

namespace HannaUIDemo.Features.Instruments;

/// <summary>Resolves measure modules by <see cref="InstrumentKind"/>.</summary>
public sealed class InstrumentMeasureHost
{
	readonly IReadOnlyDictionary<InstrumentKind, IInstrumentMeasureModule> _modules;

	public InstrumentMeasureHost(IEnumerable<IInstrumentMeasureModule> modules) =>
		_modules = modules.ToDictionary(m => m.Kind);

	public IInstrumentMeasureModule Get(InstrumentKind kind) => _modules[kind];

	public IReadOnlyList<IInstrumentMeasureModule> All => _modules.Values.ToList();
}
