using System.Linq.Expressions;
using Core.Entidades;

namespace Core.Interfaces;

/// <summary>Repositorio genérico. 'incluir' permite cargar entidades relacionadas.</summary>
public interface IRepositorio<T> where T : EntidadBase
{
    Task<T?> ObtenerPorIdAsync(int id, params Expression<Func<T, object>>[] incluir);
    Task<IReadOnlyList<T>> ObtenerTodosAsync(params Expression<Func<T, object>>[] incluir);
    Task<IReadOnlyList<T>> BuscarAsync(Expression<Func<T, bool>> filtro, params Expression<Func<T, object>>[] incluir);
    Task<T> AgregarAsync(T entidad);
    Task ActualizarAsync(T entidad);
    Task EliminarAsync(T entidad);
}
