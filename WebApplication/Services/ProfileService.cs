using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.RegularExpressions;
using WebApplication.Models;

namespace WebApplication.Services
{
    public interface IProfileService : IAsyncDisposable
    {
        public abstract Task<ProfileError?> UpdateAccount(Account account, int profileId);
        public abstract Task<ProfileError?> UpdateEmployee(Employee employee, int profileId);
        public record ProfileError(string ErrorMessage);
    }
    public partial class ProfileService : System.Object, IProfileService
    {
        protected readonly ILogger<ProfileService> Logger = default!;
        protected IDbContextFactory<MispisCourseworkContext> DatabaseFactory { get; set; } = default!;

        public ProfileService(IDbContextFactory<MispisCourseworkContext> factory, ILogger<ProfileService> logger) 
            : base() { this.Logger = logger; this.DatabaseFactory = factory; }
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public async Task<IProfileService.ProfileError?> UpdateAccount(Account account, int profileId)
        {
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                var profileRecord = await dbcontext.Accounts.FirstOrDefaultAsync(item => item.Accountid == profileId);
                if (profileRecord == null) return new IProfileService.ProfileError("Аккаунт не найден");

                profileRecord.Surname = account.Surname; profileRecord.Name = account.Name;
                profileRecord.Patronymic = account.Patronymic;

                profileRecord.Address = account.Address; profileRecord.Phonenumber = account.Phonenumber;
                profileRecord.Birthday = account.Birthday;

                await dbcontext.SaveChangesAsync();
            }
            return default(IProfileService.ProfileError);
        }
        public async Task<IProfileService.ProfileError?> UpdateEmployee(Employee employee, int profileId)
        {
            var validatePassport = employee.Passport != null && Regex.IsMatch(employee.Passport, @"^[0-9]{4}-[0-9]{6}$");
            if (!validatePassport) return new IProfileService.ProfileError("Неверный паспорт");

            if (employee.Education.Length < 5) return new IProfileService.ProfileError("Неверные данные об образовании");
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                await dbcontext.Employees.Where(item => item.Accountid == profileId).ExecuteUpdateAsync(item => 
                item.SetProperty(p => p.Passport, p => employee.Passport)
                    .SetProperty(p => p.Gender, p => employee.Gender).SetProperty(p => p.Education, p => employee.Education));

                await dbcontext.SaveChangesAsync();
            }
            return default(IProfileService.ProfileError);
        }
    }
}
