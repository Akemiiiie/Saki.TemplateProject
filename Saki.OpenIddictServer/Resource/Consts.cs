namespace Saki.OpenIddictServer.Resource;

public static class Consts
{
    public const string Email = "email";
    public const string Password = "password";
    public const string ConsentNaming = "consent"; // Used for consent claim
    public const string GrantAccessValue = "Grant";
    public const string DenyAccessValue = "Deny";
}

public static class Prompts
{
    public const string Consent = "consent"; // Prompt for user consent
    public const string Login = "login"; // Prompt for user login
    public const string SelectAccount = "select_account"; // Prompt to select an account
}