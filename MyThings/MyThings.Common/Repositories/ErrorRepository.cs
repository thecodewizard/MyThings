﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyThings.Common.Models;
using System.Data.Entity;

namespace MyThings.Common.Repositories
{
    public class ErrorRepository : GenericRepository<Error>
    {
        #region GenericRepository - Eager Loading Adaptations

        public override IEnumerable<Error> All()
        {
            return (from e in Context.Error.Include(e => e.Sensor).Include(e => e.Container) select e).ToList();
        }

        public override Error GetByID(object id)
        {
            int errorId = -1;
            return !int.TryParse(id.ToString(), out errorId)
                ? null
                : (from e in Context.Error.Include(e => e.Sensor).Include(e => e.Container) select e).FirstOrDefault();
        }

        public override Error Insert(Error error)
        {
            if(Context.Entry(error.Sensor).State != EntityState.Unchanged)
                Context.Entry(error.Sensor).State = EntityState.Unchanged;
            if(Context.Entry(error.Container).State != EntityState.Unchanged)
                Context.Entry(error.Container).State = EntityState.Unchanged;

            Context.Error.Add(error);
            return error;
        }

        #endregion

        #region Functionality Methods

        public List<Error> GetErrors()
        {
            return All().ToList();
        }

        public Error GetErrorById(int errorId)
        {
            return GetByID(errorId);
        }

        public Error SaveOrUpdateError(Error error)
        {
            if (DbSet.Find(error.Id) != null)
            {
                //The error already exists -> Update the error
                Update(error);
            }
            else
            {
                //The error doesn't exist -> Insert the error
                error = Insert(error);
            }

            SaveChanges();
            return error;
        }

        public void DeleteError(Error error)
        {
            Delete(error);
            SaveChanges();
        }

        #endregion
    }
}
