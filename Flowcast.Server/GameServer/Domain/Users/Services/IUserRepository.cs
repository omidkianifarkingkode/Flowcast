using SharedKernel;

namespace Domain.Users.Services;

public interface IUserRepository
{
    Task<Result<User>> GetById(Guid playerId);
    Task<Result<User>> GetByEmail(string email);
    Task<Result> Add(User user);
    Task<Result> Update(User user);
    Task<Result> Save(User user);
}
