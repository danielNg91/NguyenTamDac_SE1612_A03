namespace Api.Auth;

public static class PolicyName {
    public const string ADMIN = nameof(Role.Admin);
    public const string CUSTOMER = nameof(Role.Customer);
}

public enum Role {
    Admin,
    Customer
}
