using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HannaUIDemo.Core.Auth;
using HannaUIDemo.Core.Mvvm;

namespace HannaUIDemo.Features.Settings;

public partial class ProfileInformationViewModel : LocalizedViewModelBase
{
	readonly UserSessionService _session;

	[ObservableProperty] private string _email = string.Empty;
	[ObservableProperty] private string _firstName = string.Empty;
	[ObservableProperty] private string _lastName = string.Empty;
	[ObservableProperty] private string _organization = string.Empty;
	[ObservableProperty] private string _mobile = string.Empty;
	[ObservableProperty] private string _address1 = string.Empty;
	[ObservableProperty] private string _address2 = string.Empty;
	[ObservableProperty] private string _city = string.Empty;
	[ObservableProperty] private string _state = string.Empty;
	[ObservableProperty] private string _postcode = string.Empty;
	[ObservableProperty] private string _statusMessage = string.Empty;
	[ObservableProperty] private bool _showStatus;

	public ProfileInformationViewModel(UserSessionService session)
	{
		_session = session;
		LoadProfile();
		ApplyLocalization();
	}

	public string PageTitle => Loc.T("Cloud_ProfileInformation");
	public string Subtitle => Loc.T("Profile_Subtitle");
	public string AccountSectionTitle => Loc.T("Profile_AccountSection");
	public string PersonalSectionTitle => Loc.T("Profile_PersonalSection");
	public string AddressSectionTitle => Loc.T("Profile_AddressSection");
	public string EmailLabel => Loc.T("Profile_Email");
	public string FirstNameLabel => Loc.T("Profile_FirstName");
	public string LastNameLabel => Loc.T("Profile_LastName");
	public string OrganizationLabel => Loc.T("Profile_Organization");
	public string MobileLabel => Loc.T("Profile_Mobile");
	public string Address1Label => Loc.T("Profile_Address1");
	public string Address2Label => Loc.T("Profile_Address2");
	public string CityLabel => Loc.T("Profile_City");
	public string StateLabel => Loc.T("Profile_State");
	public string PostcodeLabel => Loc.T("Profile_Postcode");
	public string SaveButtonText => Loc.T("Profile_Save");

	protected override void ApplyLocalization()
	{
		OnPropertyChanged(nameof(PageTitle));
		OnPropertyChanged(nameof(Subtitle));
		OnPropertyChanged(nameof(AccountSectionTitle));
		OnPropertyChanged(nameof(PersonalSectionTitle));
		OnPropertyChanged(nameof(AddressSectionTitle));
		OnPropertyChanged(nameof(EmailLabel));
		OnPropertyChanged(nameof(FirstNameLabel));
		OnPropertyChanged(nameof(LastNameLabel));
		OnPropertyChanged(nameof(OrganizationLabel));
		OnPropertyChanged(nameof(MobileLabel));
		OnPropertyChanged(nameof(Address1Label));
		OnPropertyChanged(nameof(Address2Label));
		OnPropertyChanged(nameof(CityLabel));
		OnPropertyChanged(nameof(StateLabel));
		OnPropertyChanged(nameof(PostcodeLabel));
		OnPropertyChanged(nameof(SaveButtonText));
	}

	public void LoadProfile()
	{
		_session.Load();
		var profile = _session.GetProfile();
		Email = profile.Email;
		FirstName = profile.FirstName;
		LastName = profile.LastName;
		Organization = profile.Organization;
		Mobile = profile.Mobile;
		Address1 = profile.Address1;
		Address2 = profile.Address2;
		City = profile.City;
		State = profile.State;
		Postcode = profile.Postcode;
	}

	[RelayCommand]
	async Task SaveAsync()
	{
		_session.SaveProfile(new UserProfile
		{
			Email = Email,
			FirstName = FirstName.Trim(),
			LastName = LastName.Trim(),
			Organization = Organization.Trim(),
			Mobile = Mobile.Trim(),
			Address1 = Address1.Trim(),
			Address2 = Address2.Trim(),
			City = City.Trim(),
			State = State.Trim(),
			Postcode = Postcode.Trim()
		});

		StatusMessage = Loc.T("Profile_Saved");
		ShowStatus = true;

		await Task.Delay(1200);
		if (Shell.Current?.CurrentPage?.Navigation is { } nav)
			await nav.PopAsync();
	}
}
