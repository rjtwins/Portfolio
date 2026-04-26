using BCT.Domain.Entities;

namespace BCT.Application.ServiceInterfaces;

public interface IRepository<T> where T : IdModel
{
	Task<T?> Get(int id);
	Task<T?> FirstOrDefault(Expression<Func<T, bool>> predicate);
	Task<List<T>> GetAll();
	Task<List<T>> GetAll(Expression<Func<T, bool>> predicate);
	Task<T?> Update(T entity);
	Task<T?> Add(T entity);
    Task<List<T>> AddRange(List<T> entity);
	Task Delete(T entity);
}
