using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApplication.Infrastructure.Contexts;
using WebApplication.Infrastructure.Entities;
using WebApplication.Infrastructure.Interfaces;

namespace WebApplication.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly InMemoryContext _dbContext;

        public UserService(InMemoryContext dbContext)
        {
            _dbContext = dbContext;

            // this is a hack to seed data into the in memory database. Do not use this in production.
            _dbContext.Database.EnsureCreated();
        }

        /// <inheritdoc />
        public async Task<User?> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            User? user = await _dbContext.Users.Where(user => user.Id == id)
                                         .Include(x => x.ContactDetail)
                                         .FirstOrDefaultAsync(cancellationToken);

            return user;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<User>> FindAsync(string? givenNames, string? lastName, CancellationToken cancellationToken = default)
        {
            // throw new NotImplementedException("Implement a way to find users that match the provided given names OR last name.");

            var users = await _dbContext.Users
                .AsNoTracking()
                .Where(user =>
                (!string.IsNullOrEmpty(givenNames) && user.GivenNames.ToLower() == givenNames.ToLower()) ||
                (!string.IsNullOrEmpty(lastName) && user.LastName.ToLower() == lastName.ToLower()))
                .Include(user => user.ContactDetail)
                .ToListAsync(cancellationToken);

            return users;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<User>> GetPaginatedAsync(int page, int count, CancellationToken cancellationToken = default)
        {
            // throw new NotImplementedException("Implement a way to get a 'page' of users.");

            return await _dbContext.Users.Skip((page - 1) * count)
                                          .Take(count)
                                          .AsNoTracking()
                                          .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<User> AddAsync(User user, CancellationToken cancellationToken = default)
        {
            // throw new NotImplementedException("Implement a way to add a new user, including their contact details.");

            await _dbContext.Users.AddAsync(user, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return user;
        }

        /// <inheritdoc />
        public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
        {
            // throw new NotImplementedException("Implement a way to update an existing user, including their contact details.");
            var updatedUser = await GetAsync(user.Id, cancellationToken);
            if (updatedUser == null) return default;

            updatedUser.GivenNames = user.GivenNames;
            updatedUser.LastName = user.LastName;
            updatedUser.ContactDetail = user.ContactDetail;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return updatedUser;
        }

        /// <inheritdoc />
        public async Task<User?> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var deletedUser = await GetAsync(id, cancellationToken);
            if (deletedUser == null) return default;

            _dbContext.Users.Remove(deletedUser);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return deletedUser;

        }

        /// <inheritdoc />
        public async Task<int> CountAsync(CancellationToken cancellationToken = default)
        {
            var count = await _dbContext.Users.CountAsync(cancellationToken);
            return count;
        }
    }
}
