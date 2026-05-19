namespace HannaUIDemo.Core.Auth;

/// <summary>Hanna Cloud account profile fields.</summary>
public sealed class UserProfile
{
	public string Email { get; set; } = string.Empty;
	public string FirstName { get; set; } = string.Empty;
	public string LastName { get; set; } = string.Empty;
	public string Organization { get; set; } = string.Empty;
	public string Mobile { get; set; } = string.Empty;
	public string Address1 { get; set; } = string.Empty;
	public string Address2 { get; set; } = string.Empty;
	public string City { get; set; } = string.Empty;
	public string State { get; set; } = string.Empty;
	public string Postcode { get; set; } = string.Empty;

	public string DisplayName =>
		string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName)
			? Email
			: $"{FirstName} {LastName}".Trim();
}
