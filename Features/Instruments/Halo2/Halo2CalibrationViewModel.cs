using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Mvvm;

namespace HannaUIDemo.Features.Instruments.Halo2;

/// <summary>
/// Halo 2 five-point calibration screen state (demo only).
///
/// Each <c>BufferPoints[i]</c> represents a calibration buffer the user dips the probe into;
/// tapping a point selects it and shows a canned reading. <see cref="CalibrateCommand"/> is
/// intentionally a no-op placeholder — wire it to the actual BLE calibration command when
/// integrating the real device.
///
/// Lifecycle: created per-navigation (transient) by the DI container so each calibration
/// flow starts with fresh state. Pushed/popped via <see cref="Halo2Routes.Calibration"/>.
/// </summary>
public partial class Halo2CalibrationViewModel : PageViewModelBase
{
	/// <summary>Currently highlighted buffer index (0-based).</summary>
	[ObservableProperty] private int _activePointIndex;

	/// <summary>Live reading text shown above the buffer tile (canned per index in demo).</summary>
	[ObservableProperty] private string _readingText = "7.00 pH";

	/// <summary>Buffer name shown next to the active tile.</summary>
	[ObservableProperty] private string _bufferLabel = "pH 7.01";

	/// <summary>Demo buffer set (real device determines which buffers are available).</summary>
	public IReadOnlyList<string> BufferPoints { get; } =
		["pH 7.01", "pH 4.01", "pH 10.01", "pH 7.01", "pH 4.01"];

	/// <summary>
	/// Selects buffer by index. Parameter accepts either an int (from code) or a
	/// numeric string (from XAML CommandParameter). Out-of-range values are ignored.
	/// </summary>
	[RelayCommand]
	void SelectPoint(object? parameter)
	{
		var index = parameter switch
		{
			int i => i,
			string s when int.TryParse(s, out var n) => n,
			_ => ActivePointIndex
		};

		if (index < 0 || index >= BufferPoints.Count)
			return;

		ActivePointIndex = index;
		BufferLabel = BufferPoints[index];
		// Canned per-buffer readings — replace with the device's live mV→pH conversion.
		ReadingText = index switch
		{
			0 => "7.00 pH",
			1 => "4.02 pH",
			2 => "10.00 pH",
			_ => "7.01 pH"
		};
	}

	/// <summary>Stub — wire to BLE calibration send for real device.</summary>
	[RelayCommand]
	void Calibrate() { }

	/// <summary>Pops back to the Halo 2 settings page on the Shell stack.</summary>
	[RelayCommand]
	async Task GoBackAsync()
	{
		if (Shell.Current is not null)
			await Shell.Current.GoToAsync("..");
	}
}
