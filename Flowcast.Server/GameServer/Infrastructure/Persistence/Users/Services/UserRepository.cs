using Domain.Users;
using Domain.Users.Services;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace Infrastructure.Persistence.Users.Services
{
    public class UserRepository(ApplicationDbContext context) : IUserRepository
    {
        public async Task<Result<User>> GetById(Guid userId)
        {
            try
            {
                var user = await context.Users.FindAsync(userId);
                if (user == null)
                {
                    return Result.Failure<User>(UserErrors.NotFound(userId));
                }

                return Result.Success(user);
            }
            catch (Exception ex)
            {
                return Result.Failure<User>(Error.Failure("User.GetByIdError", $"Error occurred while get by id: {ex.Message}"));
            }
        }

        public async Task<Result<User>> GetByEmail(string email)
        {
            try
            {
                var user = await context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    return Result.Failure<User>(UserErrors.NotEmailFound(email));
                }

                return Result.Success(user);
            }
            catch (Exception ex)
            {
                return Result.Failure<User>(Error.Failure("User.GetByEmailError", $"Error occurred while get by email: {ex.Message}"));
            }
        }

        public async Task<Result> Add(User user)
        {
            try
            {
                await context.Users.AddAsync(user);
                await context.SaveChangesAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure<User>(Error.Failure("User.AddError", $"Error occurred while adding user: {ex.Message}"));
            }
        }

        public async Task<Result> Update(User user)
        {
            try
            {
                context.Users.Update(user);
                await context.SaveChangesAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure<User>(Error.Failure("User.UpdateError", $"Error occurred while updating user: {ex.Message}"));
            }
        }

        public async Task<Result> Save(User user)
        {
            try
            {
                var existingUser = await context.Users.FindAsync(user.Id);
                if (existingUser == null)
                {
                    return Result.Failure<User>(Error.Failure("User.SaveError", $"User not found with id: {user.Id}"));
                }

                context.Users.Update(user);
                await context.SaveChangesAsync();

                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure<User>(Error.Failure("User.SaveError", $"Error occurred while saving user: {ex.Message}"));
            }
        }
    }
}
