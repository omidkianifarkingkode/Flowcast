namespace SharedKernel;

public static class ErrorMapper
{
    public static int ToStatusCode(ErrorType type) => type switch
    {
        ErrorType.Failure => 400,
        ErrorType.Validation => 422,
        ErrorType.Problem => 500,
        ErrorType.NotFound => 404,
        ErrorType.Conflict => 409,
        _ => 400
    };
}
