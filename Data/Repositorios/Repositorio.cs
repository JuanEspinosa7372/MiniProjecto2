using System.Linq.Expressions;
using Core.Entidades;
using Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositorios;

/// <summary>Implementación genérica del repositorio con EF Core.</summary>
public class Repositorio<T> : IRepositorio<T> where T : EntidadBase
{
    private readonly AppDbContext _contexto;
    private readonly DbSet<T> _dbSet;

    public Repositorio(AppDbContext contexto)
    {
        _contexto = contexto;
        _dbSet = contexto.Set<T>();
    }

    private IQueryable<T> Preparar(Expression<Func<T, object>>[] incluir)
    {
        IQueryable<T> consulta = _dbSet;
        foreach (var inclusion in incluir)
            consulta = consulta.Include(inclusion);
        return consulta;
    }

    public Task<T?> ObtenerPorIdAsync(int id, params Expression<Func<T, object>>[] incluir) =>
        Preparar(incluir).FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IReadOnlyList<T>> ObtenerTodosAsync(params Expression<Func<T, object>>[] incluir) =>
        await Preparar(incluir).ToListAsync();

    public async Task<IReadOnlyList<T>> BuscarAsync(
        Expression<Func<T, bool>> filtro, params Expression<Func<T, object>>[] incluir) =>
        await Preparar(incluir).Where(filtro).ToListAsync();

    public async Task<T> AgregarAsync(T entidad)
    {
        await _dbSet.AddAsync(entidad);
        await _contexto.SaveChangesAsync();
        return entidad;
    }

    public async Task ActualizarAsync(T entidad)
    {
        _dbSet.Update(entidad);
        await _contexto.SaveChangesAsync();
    }

    public async Task EliminarAsync(T entidad)
    {
        _dbSet.Remove(entidad);
        await _contexto.SaveChangesAsync();
    }
}
