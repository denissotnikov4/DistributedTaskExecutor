using ApiKeys.Logic.Models;
using Core.Results;

namespace ApiKeys.Logic.Services.Auth;

public interface IAuthService
{
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request);
}