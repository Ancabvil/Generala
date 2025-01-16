﻿using generala.Models.Database.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace generala.Models.Database.Repositories;

//implementamos el repositorio común
public abstract class Repository<TEntity, TId> : IRepository<TEntity, TId> where TEntity : class
{
    protected GeneralaContext _context;

    public Repository(GeneralaContext context)
    {
        _context = context;
    }

    public async Task<ICollection<TEntity>> GetAllAsync()
    {
        return await _context.Set<TEntity>().ToArrayAsync();
    }

    public IQueryable<TEntity> GetQueryable(bool asNoTracking = true)
    {
        DbSet<TEntity> entities = _context.Set<TEntity>();
        return asNoTracking ? entities.AsNoTracking() : entities;
    }

    public async Task<TEntity> GetByIdAsync(TId id)
    {
        return await _context.Set<TEntity>().FindAsync(id);
    }

    public async Task<TEntity> InsertAsync(TEntity entity)
    {
        var entry = await _context.Set<TEntity>().AddAsync(entity);
        return entry.Entity;
    }

    public TEntity Update(TEntity entity)
    {
        _context.Set<TEntity>().Update(entity);
        return entity;
    }

    public Task Delete(TEntity entity)
    {
        _context.Set<TEntity>().Remove(entity);
        return Task.CompletedTask;
    }

    public async Task<bool> ExistAsync(TId id)
    {
        return await GetByIdAsync(id) != null;
    }
}


