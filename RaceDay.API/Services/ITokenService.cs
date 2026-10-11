using RaceDay.API.Models;

namespace RaceDay.API.Services
{
    public interface ITokenService
    {
        string CreateToken(string email, string role, int userId);
    }
}