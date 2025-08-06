using SharedKernel;

namespace Domain.Games;

public static class Errors
{
    public static Error NotFound(Guid userId) => Error.NotFound(
        "Users.NotFound",
        $"The user with the Id = '{userId}' was not found");
}
