using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyThings.Common.Models;
using System.Data.Entity;

namespace MyThings.Common.Repositories
{
    public class GroupRepository : GenericRepository<Group>
    {
        #region GenericRepository - Eager Loading Adaptations

        public override IEnumerable<Group> All()
        {
            return (from g in Context.Group.Include(g => g.Sensors) select g).ToList();
        }

        public override Group GetByID(object id)
        {
            int groupId = -1;
            return !int.TryParse(id.ToString(), out groupId) 
                ? null 
                : (from g in Context.Group.Include(g => g.Sensors) select g).FirstOrDefault();
        }

        public override Group Insert(Group group)
        {
            foreach (Sensor sensor in group.Sensors)
                if(Context.Entry<Sensor>(sensor).State != EntityState.Unchanged)
                    Context.Entry<Sensor>(sensor).State = EntityState.Unchanged;

            Context.Group.Add(group);
            return group;
        }
        #endregion
    }
}
