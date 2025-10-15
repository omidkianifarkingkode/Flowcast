namespace SharedKernel;

public static class ErrorMapper
{
    public static int ToStatusCode(ErrorType type) => type switch
    {
        ErrorType.Failure => 400,
        ErrorType.Validation => 400,
        ErrorType.Problem => 500,
        ErrorType.NotFound => 404,
        ErrorType.Conflict => 409,
        ErrorType.Forbidden => 403,
        _ => 400
    };
}
