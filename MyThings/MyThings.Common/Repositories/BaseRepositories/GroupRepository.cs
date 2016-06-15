using System;
using System.Collections.Generic;
using System.Linq;
using MyThings.Common.Models;
using System.Data.Entity;
using System.Globalization;
using MyThings.Common.Models.NoSQL_Entities;
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
            return (from g in Context.Group.Include(g => g.Sensors) where g.User_Id.Equals(userId) select g).ToList();
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
            //Remove the virtual sensor
            if (GetVirtualSensor(group, new SensorRepository()) != null)
            {
                RemoveVirtualSensor(group);
            }

            //Delete the group
            Delete(group);
            SaveChanges();
        }

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
                _sensorRepository.Insert(sensor);
                _sensorRepository.SaveChanges();

                // Make the new containers
                List<Container> VirtualContainers = new List<Container>(); 
                foreach (ContainerType uniqueType in uniqueContainerTypes)
                {
                    //Create the container
                    Container container = new Container();
                    container.Name = "VContainer " +  group.Id + "-" + uniqueType.Name;
                    container.ContainerType = uniqueType;
                    container.CreationTime = DateTime.Now;
                    container.MACAddress = sensor.MACAddress;
                    container.SensorId = sensor.Id;
                    _containerRepository.Insert(container);

                    // Insert the First value in tablestorage to keep the system crashing when someone queries the new virt sensor
                    double payloadTotal = 0;
                    double numberOfContainers = 0;
                    foreach (Container c in containers)
                    {
                        if (c.ContainerType.Name.Equals(uniqueType.Name))
                        {
                            double? cPayload = MachineLearningRepository.ParseAverageInTime(c, oldestDate, TimeSpan.FromHours(1));
                            if (cPayload.HasValue)
                            {
                                payloadTotal += cPayload.Value;
                                numberOfContainers++;
                            }
                        }
                    }
                    double payload = (Math.Abs(numberOfContainers) <= 0) ? 0 : payloadTotal/numberOfContainers;

                    String payloadValue = payload.ToString(CultureInfo.InvariantCulture);
                    ContainerEntity entity = new ContainerEntity(sensor.Company, container.MACAddress, container.ContainerType.Name, sensor.Location, payloadValue, oldestDate.Ticks.ToString(), null);
                    TableStorageRepository.WriteToVirtualSensorTable(entity, false);
                }
                _containerRepository.SaveChanges();

                // Update the sensor
                sensor.Containers = VirtualContainers;
                _sensorRepository.Update(sensor);
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

                List<Container> containers = new List<Container>();
                foreach (Sensor gSensor in group.Sensors)
                {
                    Sensor gS = _sensorRepository.GetSensorById(gSensor.Id);
                    foreach (Container gC in gS.Containers) containers.Add(gC);
                }

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
                        double payloadTotal = 0;
                        double numberOfContainers = 0;
                        foreach (Container c in containers)
                        {
                            if (c.ContainerType.Name.Equals(container.ContainerType.Name))
                            {
                                double? cPayload = MachineLearningRepository.ParseAverageInTime(c, nextEntityDate, TimeSpan.FromHours(1));
                                if (cPayload.HasValue)
                                {
                                    payloadTotal += cPayload.Value;
                                    numberOfContainers++;
                                }
                            }
                        }
                        double payload = (Math.Abs(numberOfContainers) <= 0) ? 0 : payloadTotal / numberOfContainers;

                        //Write to tablestorage
                        String payloadValue = payload.ToString(CultureInfo.InvariantCulture);
                        ContainerEntity newEntity = new ContainerEntity(VirtSensor.Company, container.MACAddress, container.ContainerType.Name, VirtSensor.Location, payloadValue, nextEntityDate.Ticks.ToString(), null);
                        TableStorageRepository.WriteToVirtualSensorTable(newEntity, true);

                        //Set the time right.
                        nextEntityDate = nextEntityDate.Add(interval);
                    }
                }
            }
        }

        public void RemoveVirtualSensor(Group group)
        {
            SensorRepository _sensorRepository = new SensorRepository();

            //Check if no virtual sensor exists yet
            group = GetVirtualSensor(group, _sensorRepository);
            if (group.VirtualSensor != null)
            {
                //Remove the sensor
                _sensorRepository.DeleteSensor(group.VirtualSensor);

                //Remove all the NoSQL Values
                TableStorageRepository.RemoveValuesFromTablestorage(group.VirtualSensor.MACAddress);
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

