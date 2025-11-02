namespace ECommerce.Common.Constants;

public static class AppConstants
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string Customer = "Customer";
        public const string Guest = "Guest";
    }

    public static class Policies
    {
        public const string RequireAdminRole = "RequireAdminRole";
        public const string RequireManagerRole = "RequireManagerRole";
        public const string RequireCustomerRole = "RequireCustomerRole";
    }

    public static class CacheKeys
    {
        public const string UserPrefix = "user:";
        public const string ProductPrefix = "product:";
        public const string CartPrefix = "cart:";
        public const string SessionPrefix = "session:";
    }

    public static class Headers
    {
        public const string CorrelationId = "X-Correlation-Id";
        public const string ApiVersion = "X-API-Version";
        public const string RateLimitRemaining = "X-RateLimit-Remaining";
        public const string RateLimitLimit = "X-RateLimit-Limit";
        public const string RateLimitReset = "X-RateLimit-Reset";
    }

    public static class ErrorCodes
    {
        public const string ValidationError = "VALIDATION_ERROR";
        public const string NotFound = "NOT_FOUND";
        public const string Unauthorized = "UNAUTHORIZED";
        public const string Forbidden = "FORBIDDEN";
        public const string Conflict = "CONFLICT";
        public const string InternalError = "INTERNAL_ERROR";
        public const string ServiceUnavailable = "SERVICE_UNAVAILABLE";
    }

    public static class Pagination
    {
        public const int DefaultPageNumber = 1;
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;
    }

    public static class Security
    {
        public const int MaxFailedLoginAttempts = 5;
        public const int LockoutMinutes = 15;
        public const int PasswordResetTokenExpirationHours = 1;
        public const int EmailVerificationTokenExpirationHours = 24;
        public const int RefreshTokenExpirationDays = 7;
        public const int AccessTokenExpirationMinutes = 60;
    }
}
