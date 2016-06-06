﻿using System;
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
            return (from g in Context.Group.Include(g => g.Sensors) orderby g.Name select g).ToList();
        }

        public override Group GetByID(object id)
        {
            int groupId = -1;
            return !int.TryParse(id.ToString(), out groupId) 
                ? null 
                : (from g in Context.Group.Include(g => g.Sensors) where g.Id == groupId select g).FirstOrDefault();
        }

        public override Group Insert(Group group)
        {
            if (group.Sensors != null)
                foreach (Sensor sensor in group.Sensors)
                    if (Context.Entry<Sensor>(sensor).State != EntityState.Unchanged)
                        Context.Entry<Sensor>(sensor).State = EntityState.Unchanged;

            Context.Group.Add(group);
            return group;
        }
        #endregion

        #region Functionality Methods

        public List<Group> GetGroups()
        {
            return All().ToList();
        }

        public Group GetGroupById(int groupId)
        {
            return GetByID(groupId);
        }

        public void DeleteGroup(Group group)
        {
            Delete(group);
            SaveChanges();
        }

        //public Group SaveOrUpdateGroup(Group group)
        //{
        //    if (DbSet.Find(group.Id) != null)
        //    {
        //        //The group already exists -> Update the group
        //        Update(group);
        //    } else
        //    {
        //        //The group doesn't exist -> Insert the group
        //        Insert(group);
        //    }

        //    SaveChanges();
        //    return group;
        //}

        #endregion
    }
}
