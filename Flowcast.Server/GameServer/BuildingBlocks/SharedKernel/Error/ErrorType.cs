namespace SharedKernel;

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    Problem = 2,
    NotFound = 3,
    Conflict = 4,
    Unauthorized = 5,
    Forbidden = 6,
    TooLarge = 7, // error 413
    RateLimited = 8 // error 429
}
