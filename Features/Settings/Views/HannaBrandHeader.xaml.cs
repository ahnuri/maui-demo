namespace HannaUIDemo.Features.Settings.Views;

public partial class HannaBrandHeader : ContentView
{
	public static readonly BindableProperty TitleProperty =
		BindableProperty.Create(nameof(Title), typeof(string), typeof(HannaBrandHeader), string.Empty, propertyChanged: OnTextChanged);

	public static readonly BindableProperty SubtitleProperty =
		BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(HannaBrandHeader), string.Empty, propertyChanged: OnTextChanged);

	public static readonly BindableProperty LogoHeightProperty =
		BindableProperty.Create(nameof(LogoHeight), typeof(double), typeof(HannaBrandHeader), 44d, propertyChanged: OnLogoHeightChanged);

	public string Title
	{
		get => (string)GetValue(TitleProperty);
		set => SetValue(TitleProperty, value);
	}

	public string Subtitle
	{
		get => (string)GetValue(SubtitleProperty);
		set => SetValue(SubtitleProperty, value);
	}

	public double LogoHeight
	{
		get => (double)GetValue(LogoHeightProperty);
		set => SetValue(LogoHeightProperty, value);
	}

	public HannaBrandHeader() => InitializeComponent();

	static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is not HannaBrandHeader header)
			return;

		header.TitleLabel.Text = header.Title;
		header.TitleLabel.IsVisible = !string.IsNullOrWhiteSpace(header.Title);
		header.SubtitleLabel.Text = header.Subtitle;
		header.SubtitleLabel.IsVisible = !string.IsNullOrWhiteSpace(header.Subtitle);
	}

	static void OnLogoHeightChanged(BindableObject bindable, object oldValue, object newValue)
	{
		if (bindable is HannaBrandHeader header)
			header.LogoImage.HeightRequest = header.LogoHeight;
	}
}
