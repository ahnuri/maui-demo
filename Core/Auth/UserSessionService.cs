namespace HannaUIDemo.Core.Auth;

/// <summary>Demo Hanna Cloud session — sign-in state and persisted profile.</summary>
public sealed class UserSessionService
{
	public const string LoggedInKey = "hanna_cloud_logged_in";
	public const string EmailKey = "hanna_cloud_email";
	public const string CloudSyncEnabledKey = "hanna_cloud_sync_enabled";
	const string ProfilePrefix = "hanna_profile_";

	/// <summary>
	/// Fixed demo identity for this UI prototype. Treated as the "signature" of the demo —
	/// the Sign-In screen pins the email field to this value (read-only) so reviewers always
	/// see the same account, no matter the persisted session state.
	/// </summary>
	public const string DemoEmail = "abdul@hannainst.in";

	public bool IsLoggedIn { get; private set; }
	public string Email { get; private set; } = DemoEmail;
	public bool IsVerified => Preferences.Get("hanna_verified", true);

	/// <summary>When logged in, true = cloud sync active (green), false = paused (grey).</summary>
	public bool IsCloudSyncEnabled => Preferences.Get(CloudSyncEnabledKey, true);

	public static event EventHandler? SessionChanged;

	public UserSessionService() => Load();

	public void Load()
	{
		IsLoggedIn = Preferences.Get(LoggedInKey, true);
		Email = Preferences.Get(EmailKey, DemoEmail);
	}

	public string GetDisplayName()
	{
		var profile = GetProfile();
		if (!string.IsNullOrWhiteSpace(profile.FirstName) || !string.IsNullOrWhiteSpace(profile.LastName))
			return profile.DisplayName;

		return Email.Contains("abdul", StringComparison.OrdinalIgnoreCase)
			? "Abdul Nuri"
			: profile.DisplayName;
	}

	public string GetInitials()
	{
		var name = GetDisplayName();
		var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		if (parts.Length >= 2)
			return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
		return name.Length > 0 ? char.ToUpperInvariant(name[0]).ToString() : "?";
	}

	public UserProfile GetProfile()
	{
		var profile = new UserProfile { Email = Email };
		profile.FirstName = Preferences.Get(ProfilePrefix + nameof(UserProfile.FirstName), string.Empty);
		profile.LastName = Preferences.Get(ProfilePrefix + nameof(UserProfile.LastName), string.Empty);
		profile.Organization = Preferences.Get(ProfilePrefix + nameof(UserProfile.Organization), string.Empty);
		profile.Mobile = Preferences.Get(ProfilePrefix + nameof(UserProfile.Mobile), string.Empty);
		profile.Address1 = Preferences.Get(ProfilePrefix + nameof(UserProfile.Address1), string.Empty);
		profile.Address2 = Preferences.Get(ProfilePrefix + nameof(UserProfile.Address2), string.Empty);
		profile.City = Preferences.Get(ProfilePrefix + nameof(UserProfile.City), string.Empty);
		profile.State = Preferences.Get(ProfilePrefix + nameof(UserProfile.State), string.Empty);
		profile.Postcode = Preferences.Get(ProfilePrefix + nameof(UserProfile.Postcode), string.Empty);
		return profile;
	}

	public void SaveProfile(UserProfile profile)
	{
		Preferences.Set(ProfilePrefix + nameof(UserProfile.FirstName), profile.FirstName ?? string.Empty);
		Preferences.Set(ProfilePrefix + nameof(UserProfile.LastName), profile.LastName ?? string.Empty);
		Preferences.Set(ProfilePrefix + nameof(UserProfile.Organization), profile.Organization ?? string.Empty);
		Preferences.Set(ProfilePrefix + nameof(UserProfile.Mobile), profile.Mobile ?? string.Empty);
		Preferences.Set(ProfilePrefix + nameof(UserProfile.Address1), profile.Address1 ?? string.Empty);
		Preferences.Set(ProfilePrefix + nameof(UserProfile.Address2), profile.Address2 ?? string.Empty);
		Preferences.Set(ProfilePrefix + nameof(UserProfile.City), profile.City ?? string.Empty);
		Preferences.Set(ProfilePrefix + nameof(UserProfile.State), profile.State ?? string.Empty);
		Preferences.Set(ProfilePrefix + nameof(UserProfile.Postcode), profile.Postcode ?? string.Empty);
	}

	public bool SignIn(string email, string password)
	{
		if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
			return false;

		Email = email.Trim();
		IsLoggedIn = true;
		Preferences.Set(LoggedInKey, true);
		Preferences.Set(EmailKey, Email);
		SessionChanged?.Invoke(this, EventArgs.Empty);
		return true;
	}

	public bool Register(string email, string password, UserProfile? optional = null)
	{
		if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
			return false;

		Email = email.Trim();
		Preferences.Set(EmailKey, Email);

		if (optional is not null)
		{
			optional.Email = Email;
			SaveProfile(optional);
		}

		IsLoggedIn = true;
		Preferences.Set(LoggedInKey, true);
		SessionChanged?.Invoke(this, EventArgs.Empty);
		return true;
	}

	public void SetCloudSyncEnabled(bool enabled)
	{
		Preferences.Set(CloudSyncEnabledKey, enabled);
		SessionChanged?.Invoke(this, EventArgs.Empty);
	}

	public void SignOut()
	{
		IsLoggedIn = false;
		Preferences.Set(LoggedInKey, false);
		SessionChanged?.Invoke(this, EventArgs.Empty);
	}
}
