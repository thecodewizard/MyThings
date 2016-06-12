using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyThings.Common.Models;
using System.Data.Entity;
using System.Globalization;
using MyThings.Common.Models.NoSQL_Entities;
using Newtonsoft.Json;
using Container = MyThings.Common.Models.Container;

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
                {
                    if (Context.Entry<Sensor>(sensor).State != EntityState.Unchanged)
                        Context.Entry<Sensor>(sensor).State = EntityState.Unchanged;
                    if (sensor.Containers != null)
                    {
                        foreach (Container container in sensor.Containers)
                        {
                            if (Context.Entry<Container>(container).State != EntityState.Unchanged)
                                Context.Entry<Container>(container).State = EntityState.Unchanged;
                            if (Context.Entry<ContainerType>(container.ContainerType).State != EntityState.Unchanged)
                                Context.Entry<ContainerType>(container.ContainerType).State = EntityState.Unchanged;
                        }
                    }
                }

            Context.Group.Add(group);
            return group;
        }

        public override void Update(Group group)
        {
            if (group.Sensors != null)
                foreach (Sensor sensor in group.Sensors)
                {
                    if (Context.Entry<Sensor>(sensor).State != EntityState.Unchanged)
                        Context.Entry<Sensor>(sensor).State = EntityState.Unchanged;
                    if (sensor.Containers != null)
                    {
                        foreach (Container container in sensor.Containers)
                        {
                            if (Context.Entry<Container>(container).State != EntityState.Unchanged)
                                Context.Entry<Container>(container).State = EntityState.Unchanged;
                            if (Context.Entry<ContainerType>(container.ContainerType).State != EntityState.Unchanged)
                                Context.Entry<ContainerType>(container.ContainerType).State = EntityState.Unchanged;
                        }
                    }
                }

            DbSet.Attach(group);
            Context.Entry(group).State = EntityState.Modified;
        }

        public override void Delete(Group group)
        {
            if (group.Sensors != null)
                foreach (Sensor sensor in group.Sensors)
                {
                    if (Context.Entry<Sensor>(sensor).State != EntityState.Unchanged)
                        Context.Entry<Sensor>(sensor).State = EntityState.Unchanged;
                    if (sensor.Containers != null)
                    {
                        foreach (Container container in sensor.Containers)
                        {
                            if (Context.Entry<Container>(container).State != EntityState.Unchanged)
                                Context.Entry<Container>(container).State = EntityState.Unchanged;
                            if (Context.Entry<ContainerType>(container.ContainerType).State != EntityState.Unchanged)
                                Context.Entry<ContainerType>(container.ContainerType).State = EntityState.Unchanged;
                        }
                    }
                }
            if (Context.Entry(group).State == EntityState.Detached)
            {
                DbSet.Attach(group);
            }
            DbSet.Remove(group);
        }

        #endregion

        #region Functionality Methods

        public List<Group> GetGroups()
        {
            return All().ToList();
        }

        public List<Group> GetGroupsForUser(String userId)
        {
            return (from g in Context.Group where g.User_Id.Equals(userId) select g).ToList();
        }

        public Group GetGroupById(int groupId)
        {
            return GetByID(groupId);
        }

        public bool SensorInGroup(int groupId, int sensorId)
        {
            return
                (from g in Context.Group
                    where g.Id == groupId && (from s in g.Sensors select s.Id).ToList<int>().Contains(sensorId)
                    select g).Any();
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

        #region Virtual Sensor Logic

        public void CreateVirtualSensor(Group group)
        {
            SensorRepository _sensorRepository = new SensorRepository();
            ContainerRepository _containerRepository = new ContainerRepository();

            //Check if no virtual sensor exists yet
            group = GetVirtualSensor(group, _sensorRepository);
            if (group.VirtualSensor == null)
            {
                // Define what we need in the group.
                String company = group.Sensors.First().Company;
                DateTime oldestDate = DateTime.Now;
                List<Container> containers = new List<Container>();
                foreach (Sensor gSensor in group.Sensors)
                {
                    //Check if all the sensors are from the same user
                    if (!company.Equals(gSensor.Company)) return;

                    //Check the date of the oldest sensor in the group
                    if (oldestDate > gSensor.CreationDate) oldestDate = gSensor.CreationDate;

                    //See what different containers we have on all the sensor
                    foreach (Container container in gSensor.Containers)
                        containers.Add(container);
                }

                List<ContainerType> uniqueContainerTypes = new List<ContainerType>();
                foreach (Container c in containers)
                {
                    if (!uniqueContainerTypes.Contains(c.ContainerType))
                        uniqueContainerTypes.Add(c.ContainerType);
                }

                // Make the new sensor
                Sensor sensor = new Sensor();
                sensor.CreationDate = DateTime.Now;
                sensor.MACAddress = "VSensor Group " + group.Id;
                sensor.SensorEntries = 1;
                sensor.Company = company;
                sensor.Location = "Virtual Sensor - No Location Available";
                sensor.Name = "Virtual Sensor Group " + group.Id;
                sensor.IsVirtual = true;
                sensor.Containers = new List<Container>();

                // Make the new containers
                List<Container> VirtualContainers = new List<Container>(); 
                foreach (ContainerType uniqueType in uniqueContainerTypes)
                {
                    //Create the container
                    Container container = new Container();
                    container.Name = uniqueType.Name + "-" + sensor.MACAddress;
                    container.ContainerType = uniqueType;
                    container.CreationTime = DateTime.Now;
                    container.MACAddress = sensor.MACAddress;
                    container.SensorId = sensor.Id;
                    _containerRepository.Insert(container);

                    // Insert the First value in tablestorage to keep the system crashing when someone queries the new virt sensor
                    double payload = MachineLearningRepository.ParseAverageInTime(container, oldestDate, TimeSpan.FromHours(1));
                    String payloadValue = payload.ToString(CultureInfo.InvariantCulture);

                    ContainerEntity entity = new ContainerEntity(sensor.Company, container.MACAddress, container.ContainerType.Name, sensor.Location, payloadValue, oldestDate.Ticks.ToString(), null);
                    TableStorageRepository.WriteToVirtualSensorTable(entity, false);
                }
                _containerRepository.SaveChanges();

                // Update the sensor
                sensor.Containers = VirtualContainers;

                // Save the virtual sensors & containers
                _sensorRepository.Insert(sensor);
                _sensorRepository.SaveChanges();

                // Update the group
                group.VirtualSensorIdentifier = sensor.Id;
                Update(group);
                SaveChanges();
            }
        }

        public void UpdateVirtualSensor(Group group)
        {
            SensorRepository _sensorRepository = new SensorRepository();

            //Check if no virtual sensor exists yet
            group = GetVirtualSensor(group, _sensorRepository);
            if (group.VirtualSensor != null)
            {
                //Get the virtual sensor
                Sensor VirtSensor = _sensorRepository.GetSensorById(group.VirtualSensorIdentifier);

                //Calculate the virtual values
                if (VirtSensor?.Containers == null) return;
                foreach (Container container in VirtSensor.Containers)
                {
                    ContainerEntity entity = TableStorageRepository.GetMostRecentVirtualSensorEntity(container.MACAddress, container.ContainerType.Name);

                    long ticks;
                    bool success = long.TryParse(entity.receivedtimestamp, out ticks);
                    if (!success) return;

                    TimeSpan interval = TimeSpan.FromHours(1);
                    DateTime oldestDate = new DateTime(ticks);
                    DateTime nextEntityDate = oldestDate.Add(interval);

                    while (nextEntityDate < DateTime.Now)
                    {
                        //Render the payload for the nextEntity
                        double payload = MachineLearningRepository.ParseAverageInTime(container, nextEntityDate, interval);
                        String payloadValue = payload.ToString(CultureInfo.InvariantCulture);

                        //Write to tablestorage
                        ContainerEntity newEntity = new ContainerEntity(VirtSensor.Company, container.MACAddress, container.ContainerType.Name, VirtSensor.Location, payloadValue, nextEntityDate.Ticks.ToString(), null);
                        TableStorageRepository.WriteToVirtualSensorTable(newEntity, false);

                        //Set the time right.
                        nextEntityDate = nextEntityDate.Add(interval);
                    }
                }
            }
        }

        public void RemoveVirtualSensor(Group group)
        {
            SensorRepository _sensorRepository = new SensorRepository();
            ContainerRepository _containerRepository = new ContainerRepository();

            //Check if no virtual sensor exists yet
            group = GetVirtualSensor(group, _sensorRepository);
            if (group.VirtualSensor != null)
            {
                Sensor VirtSensor = _sensorRepository.GetSensorById(group.VirtualSensorIdentifier);
                if (VirtSensor?.Containers == null) return;
                List<Container> containers = VirtSensor.Containers;

                //Detach the container from the sensor
                VirtSensor.Containers = new List<Container>();
                _sensorRepository.Update(VirtSensor);
                _sensorRepository.SaveChanges();

                //Remove all the containers
                foreach (Container container in containers)
                {
                    _containerRepository.Delete(container);
                }
                _containerRepository.SaveChanges();

                //Remove the sensor
                _sensorRepository.DeleteSensor(VirtSensor);
                
                //Remove all the NoSQL Values
                TableStorageRepository.RemoveValuesFromTablestorage(VirtSensor.MACAddress);
            }
        }

        public Group GetVirtualSensor(Group group, SensorRepository sensorRepository)
        {
            Sensor sensor = sensorRepository.GetSensorById(group.VirtualSensorIdentifier);
            if (sensor != null)
            {
                group.VirtualSensor = sensor;
            }

            return group;
        }

        #endregion
    }
}

