using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Mvvm;

namespace HannaUIDemo.Features.Halo2;

/// <summary>Halo 2 five-point calibration demo state.</summary>
public partial class Halo2CalibrationViewModel : PageViewModelBase
{
	[ObservableProperty] private int _activePointIndex;
	[ObservableProperty] private string _readingText = "7.00 pH";
	[ObservableProperty] private string _bufferLabel = "pH 7.01";

	public IReadOnlyList<string> BufferPoints { get; } =
		["pH 7.01", "pH 4.01", "pH 10.01", "pH 7.01", "pH 4.01"];

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
		ReadingText = index switch
		{
			0 => "7.00 pH",
			1 => "4.02 pH",
			2 => "10.00 pH",
			_ => "7.01 pH"
		};
	}

	[RelayCommand]
	void Calibrate() { }

	[RelayCommand]
	async Task GoBackAsync()
	{
		if (Shell.Current is not null)
			await Shell.Current.GoToAsync("..");
	}
}
