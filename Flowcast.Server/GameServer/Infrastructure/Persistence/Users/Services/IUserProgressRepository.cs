using Domain.Games;
using Domain.Games.Services;
using Domain.Users;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Infrastructure.Persistence.Users.Services
{
    public class UserProgressRepository(ApplicationDbContext context) : IUserProgressRepository
    {
        public async Task<Result<UserProgress>> GetByUserIdAsync(Guid userId)
        {
            try
            {
                var userProgress = await context.UserProgress
                    .FirstOrDefaultAsync(up => up.UserId == userId);

                if (userProgress == null)
                {
                    return Result.Failure<UserProgress>(UserErrors.NotFound(userId));
                }

                return Result.Success(userProgress);
            }
            catch (Exception ex)
            {
                return Result.Failure<UserProgress>(Error.Failure("UserProgress.GetByUserIdError", $"Error occurred while retrieving user progress: {ex.Message}"));
            }
        }

        // Add new UserProgress
        public async Task<Result> AddAsync(UserProgress userProgress)
        {
            try
            {
                await context.UserProgress.AddAsync(userProgress);
                await context.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(Error.Failure("UserProgress.AddError", $"Error occurred while adding user progress: {ex.Message}"));
            }
        }

        // Update existing UserProgress
        public async Task<Result> UpdateAsync(UserProgress userProgress)
        {
            try
            {
                context.UserProgress.Update(userProgress);
                await context.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(Error.Failure("UserProgress.UpdateError", $"Error occurred while updating user progress: {ex.Message}"));
            }
        }

        // Save UserProgress (either insert or update)
        public async Task<Result> SaveAsync(UserProgress userProgress)
        {
            try
            {
                var existingUserProgress = await context.UserProgress
                    .FirstOrDefaultAsync(up => up.UserId == userProgress.UserId);

                if (existingUserProgress == null)
                {
                    await context.UserProgress.AddAsync(userProgress);
                }
                else
                {
                    context.UserProgress.Update(userProgress);
                }

                await context.SaveChangesAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure(Error.Failure("UserProgress.SaveError", $"Error occurred while saving user progress: {ex.Message}"));
            }
        }
    }
}
