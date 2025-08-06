using SharedKernel;

namespace Domain.Users;

public static class UserErrors
{
    public static Error NotFound(Guid userId) => Error.NotFound(
        "Users.NotFound",
        $"The user with the Id = '{userId}' was not found");

    public static Error NotEmailFound(string email) => Error.NotFound(
        "Users.NotFoundByEmail",
        $"The user with the Email = '{email}' was not found");

    public static Error Unauthorized() => Error.Failure(
        "Users.Unauthorized",
        "You are not authorized to perform this action.");

    public static Error EmailNotUnique(string email) => Error.Conflict(
        "Users.EmailNotUnique",
        $"The provided email = '{email}' is not unique");
}
