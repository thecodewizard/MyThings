using System;
using System.Collections.Generic;
using System.Data.Entity;
using MyThings.Common.Context;
using MyThings.Common.Models;
using MyThings.Common.Repositories.Interfaces;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace MyThings.Common.Repositories
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {

        internal MyThingsContext Context;
        internal DbSet<TEntity> DbSet;


        public GenericRepository()
        {
            this.Context = new MyThingsContext();
            this.DbSet = Context.Set<TEntity>();
        }

        public GenericRepository(MyThingsContext context)
        {
            this.Context = context;
            this.DbSet = context.Set<TEntity>();
        }

        public virtual IEnumerable<TEntity> All()
        {
            return DbSet;
        }

        public virtual TEntity GetByID(object id)
        {
            return DbSet.Find(id);
        }

        public virtual TEntity Insert(TEntity entity)
        {
            return DbSet.Add(entity);
        }

        public virtual void Delete(object id)
        {
            TEntity entityToDelete = DbSet.Find(id);
            Delete(entityToDelete);
        }

        public virtual void Delete(TEntity entityToDelete)
        {
            if (Context.Entry(entityToDelete).State == EntityState.Detached)
            {
                DbSet.Attach(entityToDelete);
            }
            DbSet.Remove(entityToDelete);
        }

        public virtual void Update(TEntity entityToUpdate)
        {
            DbSet.Attach(entityToUpdate);
            Context.Entry(entityToUpdate).State = EntityState.Modified;
        }

        public virtual void SaveChanges()
        {
            bool saveFailed;
            do
            {
                saveFailed = false;

                try
                {
                    Context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    saveFailed = true;

                    // Update original values from the database 
                    if (ex.Data.Count == 0) return;
                    var entry = ex.Entries.Single();               
                    entry.OriginalValues.SetValues(entry.GetDatabaseValues());
                }

            } while (saveFailed);
        }
    }
}