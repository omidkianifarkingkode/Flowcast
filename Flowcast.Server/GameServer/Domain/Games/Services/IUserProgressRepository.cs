using SharedKernel;

namespace Domain.Games.Services
{
    public interface IUserProgressRepository
    {
        Task<Result<UserProgress>> GetByUserIdAsync(Guid userId);
        Task<Result> AddAsync(UserProgress userProgress);
        Task<Result> UpdateAsync(UserProgress userProgress);
        Task<Result> SaveAsync(UserProgress userProgress);
    }
}
