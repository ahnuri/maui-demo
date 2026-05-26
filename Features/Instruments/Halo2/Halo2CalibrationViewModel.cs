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
public partial class Halo2CalibrationViewModel : LocalizedViewModelBase
{
	/// <summary>Currently highlighted buffer index (0-based).</summary>
	[ObservableProperty] private int _activePointIndex;

	/// <summary>Live reading text shown above the buffer tile (canned per index in demo).</summary>
	[ObservableProperty] private string _readingText = string.Empty;

	/// <summary>Buffer name shown next to the active tile.</summary>
	[ObservableProperty] private string _bufferLabel = string.Empty;

	/// <summary>Demo buffer set (real device determines which buffers are available).</summary>
	[ObservableProperty] private IReadOnlyList<string> _bufferPoints = Array.Empty<string>();

	IReadOnlyList<string> BuildBufferPoints() =>
	[
		Loc.T("Halo_Calibration_BufferFormat", Loc.T("Halo_Calibration_Buffer_701")),
		Loc.T("Halo_Calibration_BufferFormat", Loc.T("Halo_Calibration_Buffer_401")),
		Loc.T("Halo_Calibration_BufferFormat", Loc.T("Halo_Calibration_Buffer_1001")),
		Loc.T("Halo_Calibration_BufferFormat", Loc.T("Halo_Calibration_Buffer_701")),
		Loc.T("Halo_Calibration_BufferFormat", Loc.T("Halo_Calibration_Buffer_401")),
	];

	protected override void ApplyLocalization()
	{
		BufferPoints = BuildBufferPoints();
		BufferLabel = BufferPoints[ActivePointIndex];
		ReadingText = BuildReading(ActivePointIndex);
	}

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
		ReadingText = BuildReading(index);
	}

	string BuildReading(int index) => index switch
	{
		0 => Loc.T("Halo_Calibration_ReadingFormat", Loc.T("Halo_Calibration_Reading_700")),
		1 => Loc.T("Halo_Calibration_ReadingFormat", Loc.T("Halo_Calibration_Reading_402")),
		2 => Loc.T("Halo_Calibration_ReadingFormat", Loc.T("Halo_Calibration_Reading_1000")),
		_ => Loc.T("Halo_Calibration_ReadingFormat", Loc.T("Halo_Calibration_Reading_701"))
	};

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
