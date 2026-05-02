using NexaCV.Api.Models;

namespace NexaCV.Api.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> ExistsByEmailOrUsernameAsync(string email, string username);
}
