using SharedKernel;

namespace Domain.Users;

public interface IUserRepository
{
    Result<User> GetById(long playerId);
    Result<User> GetByEmail(string email);
    void Save(User user);
}
