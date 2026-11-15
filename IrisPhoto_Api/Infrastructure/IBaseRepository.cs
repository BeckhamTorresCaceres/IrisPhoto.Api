namespace IrisPhoto_Api.Infrastructure
{
    public interface IBaseRepository
    {
        public interface IBaseRepository<T> where T : class
        {
            // 1. Obtener toda la tabla (para filtros, DataTables, etc.)
            Task<IEnumerable<T>> GetAllAsync();

            // 2. Obtener por ID (Read)
            Task<T?> GetByIdAsync(int id);

            // 3. Crear (Create)
            Task AddAsync(T entity);

            // 4. Actualizar (Update)
            Task UpdateAsync(T entity);

            // 5. Eliminar (Delete)
            Task DeleteAsync(T entity);
        }
    }
}
