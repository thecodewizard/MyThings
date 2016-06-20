using System;
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
            Error prevError = (from e in GetErrors()
                               where
                                   e.SensorId.Equals(error.SensorId) && e.ContainerId.Equals(error.ContainerId) &&
                                   e.ErrorCode.Equals(error.ErrorCode)
                               select e).FirstOrDefault();

            //(DateTime.Now.Subtract(prevError.Time) > TimeSpan.FromDays(1))

            if (prevError == null)
            {
                if (error.Sensor != null && Context.Entry(error.Sensor).State != EntityState.Unchanged)
                    Context.Entry(error.Sensor).State = EntityState.Unchanged;
                if (error.Container != null && Context.Entry(error.Container).State != EntityState.Unchanged)
                    Context.Entry(error.Container).State = EntityState.Unchanged;

                Context.Error.Add(error);
            } else if (error.ErrorCode == 199 || error.ErrorCode == 299)
            {
                //Add always if generic error or warning.
                Context.Error.Add(error);
            }
            return error;
        }

        #endregion

        #region Functionality Methods

        public List<Error> GetErrors(int? count = null)
        {
            if (count.HasValue)
                return
                    (from e in Context.Error
                     orderby e.Time descending
                     select e)
                        .Take(count.Value)
                        .ToList();

            return All().ToList();
        }

        public List<Error> GetErrorsForUser(String userCompany)
        {
            return
                (from e in Context.Error where e.Sensor.Company.Equals(userCompany) orderby e.Time descending select e)
                    .ToList();
        }

        public Error GetErrorById(int errorId)
        {
            return GetByID(errorId);
        }

        public Error MarkErrorAsReaded(Error error, bool readed = true)
        {
            error.Read = readed;
            Update(error);
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
