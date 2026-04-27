using BCT.Application.ServiceInterfaces;
using BCT.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace BCT.EF.Repositories
{
	public class Repository<T> : IRepository<T> where T : IdModel
	{
		private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

		public Repository(IDbContextFactory<ApplicationDbContext> contextFactory)
		{
			_contextFactory = contextFactory;
		}
		
		protected ApplicationDbContext GetContext() => _contextFactory.CreateDbContext();

		public async Task<T?> Add(T entity)
		{
			using var context = GetContext();

			context.Add(entity);
			await context.SaveChangesAsync();
			return context.Set<T>().FirstOrDefault(x => x.Id == entity.Id);
		}

        public async Task<List<T>> AddRange(List<T> entities)
        {
            using var context = GetContext();
            foreach (var e in entities)
            {
                context.Add<T>(e);
            }

            await context.SaveChangesAsync();
            return entities;
        }

		public async Task Delete(T entity)
		{
			using var context = GetContext();

			context.Remove(entity);
			await context.SaveChangesAsync();
		}

		public async Task<T?> FirstOrDefault(Expression<Func<T, bool>> predicate)
		{
			using var context = GetContext();

			return await Task.FromResult<T?>(context.Set<T>().FirstOrDefault(predicate));
		}

		public async Task<T?> Get(int id)
		{
			using var context = GetContext();

			return await Task.FromResult<T?>(context.Set<T>().FirstOrDefault(x => x.Id == id));
		}

		public async Task<List<T>> GetAll(Expression<Func<T, bool>> predicate)
		{
			using var context = GetContext();

			return await context.Set<T>().Where(predicate).ToListAsync();
		}

		public async Task<List<T>> GetAll()
		{
			using var context = GetContext();
			return await context.Set<T>().ToListAsync();
		}

		public async Task<T?> Update(T entity)
		{
			using var context = GetContext();

			context.Update(entity);
			await context.SaveChangesAsync();

			return context.Set<T>().FirstOrDefault(x => x.Id == entity.Id);
		}
	}
}
