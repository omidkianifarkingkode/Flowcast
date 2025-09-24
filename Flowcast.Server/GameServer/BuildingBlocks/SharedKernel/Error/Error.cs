namespace SharedKernel;

public record Error
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static readonly Error NullValue = new(
        "General.Null",
        "Null value was provided",
        ErrorType.Failure);

    public Error(string code, string description, ErrorType type)
    {
        Code = code;
        Description = description;
        Type = type;
    }

    public string Code { get; }

    public string Description { get; }

    public ErrorType Type { get; }

    public static Error Failure(string code, string description) =>
        new(code, description, ErrorType.Failure);

    public static Error NotFound(string code, string description) =>
        new(code, description, ErrorType.NotFound);

    public static Error Problem(string code, string description) =>
        new(code, description, ErrorType.Problem);

    public static Error Conflict(string code, string description) =>
        new(code, description, ErrorType.Conflict);

    public static Error Unauthorized(string code, string description) =>
        new(code, description, ErrorType.Unauthorized);

    public static Error Validation(string code, string description) =>
        new(code, description, ErrorType.Validation);

    public static Error Forbidden(string code, string description) =>
        new(code, description, ErrorType.Forbidden);

    public const string CodeUnauthorized = "Auth.Unauthorized";
    public const string CodeNotFound = "General.NotFound";
    public const string CodeConflict = "General.Conflict";
    public const string CodeForbidden = "Auth.Forbidden";
    public const string CodeValidation = "General.Validation";


    // Default errors
    public static readonly Error DefaultUnauthorized = Unauthorized(CodeUnauthorized, "The request is not authorized.");
    public static readonly Error DefaultNotFound = NotFound(CodeNotFound, "The requested resource was not found.");
    public static readonly Error DefaultConflict = Conflict(CodeConflict, "A conflict occurred.");
    public static readonly Error DefaultForbidden = Forbidden(CodeForbidden, "You do not have permission to access this resource.");
    public static readonly Error DefaultValidation = Validation(CodeValidation, "One or more validation errors occurred");

    public static Error UnauthorizedWith(string description) => Unauthorized(CodeUnauthorized, description);
    public static Error ForbiddenWith(string description) => Forbidden(CodeForbidden, description);
}
