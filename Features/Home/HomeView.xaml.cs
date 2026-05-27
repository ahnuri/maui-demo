namespace HannaUIDemo.Features.Home;

public partial class HomeView : ContentView
{
	/// <summary>
	/// Below this view width (in device-independent points) the two hero stat
	/// tiles stack vertically instead of sitting side-by-side. Chosen so that:
	/// <list type="bullet">
	///   <item><description>iPhone 12 / 13 mini (360pt) — vertical</description></item>
	///   <item><description>iPhone SE 2nd / 3rd gen (375pt) — vertical</description></item>
	///   <item><description>iPhone 14 / 15 (390pt) — horizontal</description></item>
	///   <item><description>iPhone 14 / 15 Pro (393pt) — horizontal</description></item>
	///   <item><description>iPhone Pro Max (428pt) — horizontal</description></item>
	///   <item><description>Any phone rotated to landscape — horizontal</description></item>
	/// </list>
	/// HomeView fills the page (Page Padding=0), so <c>Width</c> equals the
	/// screen width in portrait.
	/// </summary>
	const double StatStackBreakpoint = 385;

	public HomeView()
	{
		InitializeComponent();
		SizeChanged += OnSizeChanged;
		Loaded += OnLoaded;
	}

	void OnLoaded(object? sender, EventArgs e) => ApplyStatLayout();
	void OnSizeChanged(object? sender, EventArgs e) => ApplyStatLayout();

	void ApplyStatLayout()
	{
		if (StatsGrid is null || ConnectedTile is null || RecordsTile is null)
			return;

		var width = Width;
		if (width <= 0)
			return;

		var isCompact = width < StatStackBreakpoint;

		if (isCompact)
		{
			StatsGrid.ColumnDefinitions = [new ColumnDefinition(GridLength.Star)];
			StatsGrid.RowDefinitions =
			[
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Auto),
			];
			Grid.SetColumn(ConnectedTile, 0);
			Grid.SetRow(ConnectedTile, 0);
			Grid.SetColumn(RecordsTile, 0);
			Grid.SetRow(RecordsTile, 1);
		}
		else
		{
			StatsGrid.ColumnDefinitions =
			[
				new ColumnDefinition(GridLength.Star),
				new ColumnDefinition(GridLength.Star),
			];
			StatsGrid.RowDefinitions = [new RowDefinition(GridLength.Auto)];
			Grid.SetColumn(ConnectedTile, 0);
			Grid.SetRow(ConnectedTile, 0);
			Grid.SetColumn(RecordsTile, 1);
			Grid.SetRow(RecordsTile, 0);
		}
	}
}
