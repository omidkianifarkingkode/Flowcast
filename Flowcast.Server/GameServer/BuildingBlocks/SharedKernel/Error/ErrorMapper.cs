namespace SharedKernel;

public static class ErrorMapper
{
    public static int ToStatusCode(ErrorType type) => type switch
    {
        ErrorType.Failure => 400,
        ErrorType.Validation => 400,
        ErrorType.Forbidden => 403,
        ErrorType.NotFound => 404,
        ErrorType.Conflict => 409,
        ErrorType.TooLarge => 413, //Tolargge error
        ErrorType.RateLimited => 429, //rate limited error
        ErrorType.Problem => 500,
        _ => 400
    };
}
