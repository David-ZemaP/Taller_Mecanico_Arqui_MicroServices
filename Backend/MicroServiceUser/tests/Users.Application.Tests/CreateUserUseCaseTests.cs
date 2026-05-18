using System.Threading.Tasks;
using Xunit;
using Taller_Mecanico_Users.Application.UseCases.Users;
using Taller_Mecanico_Users.Domain.Interfaces;
using Taller_Mecanico_Users.Domain.Entities;
using Taller_Mecanico_Users.Domain.Common;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Generic;
using System.Threading;

namespace Taller_Mecanico_Users.Tests
{
    public class CreateUserUseCaseTests
    {
        [Fact]
        public async Task CreateUser_CreatesUserAndReturnsPassword()
        {
            var repo = new FakeUsuarioRepo();
            var passwordSecurity = new FakePasswordSecurity();
            var passwordHasher = new FakePasswordHasher();
            var mailSender = new FakeMailSender();
            var authHelper = new FakeAuthHelper();

            var logger = NullLogger<CreateUserUseCase>.Instance;

            var useCase = new CreateUserUseCase(
                repo,
                mailSender,
                passwordSecurity,
                passwordHasher,
                logger,
                authHelper);

            var result = await useCase.ExecuteAsync("test.user@example.com");

            Assert.True(result.IsSuccess);
            Assert.False(string.IsNullOrWhiteSpace(result.Value!.PlainPassword));
            Assert.Equal("test.user@example.com", result.Value.User.Email);
        }

        private class FakeUsuarioRepo : IUsuarioLoginRepository
        {
            private readonly List<UsuarioLogin> _storage = new();

            public Task<IEnumerable<UsuarioLogin>> GetAllAsync() => Task.FromResult<IEnumerable<UsuarioLogin>>(_storage);

            public Task<UsuarioLogin?> GetByEmailAsync(string email)
            {
                var found = _storage.Find(u => u.Email == email);
                return Task.FromResult<UsuarioLogin?>(found);
            }

            public Task<Result<UsuarioLogin?>> GetByIdAsync(int id)
            {
                var found = _storage.Find(u => u.UsuarioLoginId == id);
                return Task.FromResult(Result<UsuarioLogin?>.Success(found));
            }

            public Task<Result> AddAsync(UsuarioLogin entity)
            {
                entity.AsignarIdentificador(_storage.Count + 1);
                _storage.Add(entity);
                return Task.FromResult(Result.Success());
            }

            public Task<Result> UpdateAsync(UsuarioLogin entity)
            {
                return Task.FromResult(Result.Success());
            }

            public Task<Result> DeleteAsync(int id)
            {
                return Task.FromResult(Result.Success());
            }
        }

        private class FakePasswordSecurity : Domain.Interfaces.IPasswordSecurity
        {
            public Result ValidatePassword(string? password) => Result.Success();
            public string GenerateSecurePassword(int length = 12) => "Temp#1234";
        }

        private class FakePasswordHasher : Domain.Interfaces.IPasswordHasher
        {
            public string HashPassword(string plain) => "HASHED:" + plain;

            public bool VerifyPassword(string plain, string hash) => hash == ("HASHED:" + plain);
        }

        private class FakeMailSender : Domain.Interfaces.IMailSender
        {
            public Task SendEmailAsync(string recipient, string subject, string body)
            {
                return Task.CompletedTask;
            }
        }

        private class FakeAuthHelper : Domain.Interfaces.IAuthenticationHelper
        {
            public string? GetCurrentAuditActor() => "test-actor";
        }
    }
}
