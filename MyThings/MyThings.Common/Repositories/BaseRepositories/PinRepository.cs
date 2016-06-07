using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyThings.Common.Models;

namespace MyThings.Common.Repositories
{
    public class PinRepository : GenericRepository<Pin>
    {
        #region Functionality Methods

        public List<Pin> GetPinsForUser(String userId)
        {
            return (from p in Context.Pins where p.UserId == userId select p).ToList();
        }

        public Pin GetPinForTile(String userID, int tileId)
        {
            return (from p in Context.Pins where p.UserId == userID && p.TileId == tileId select p).FirstOrDefault();
        }

        public Pin GetPinById(int pinId)
        {
            return GetByID(pinId);
        }

        public void DeletePin(Pin pin)
        {
            Delete(pin);
            SaveChanges();
        }

        #endregion

        #region Smart Methods
        public int GetPinId(int valueId, PinType type)
        {
            return
                (from p in Context.Pins where p.SavedId == valueId && p.SavedType == type select p.Id).FirstOrDefault();
        }

        public List<PinType> GetFoundPinTypesById(int valueId)
        {
            return (from p in Context.Pins where p.SavedId == valueId select p.SavedType).ToList();
        }

        public bool IsSensorPinned(int sensorId)
        {
            return GetFoundPinTypesById(sensorId).Contains(PinType.Sensor);
        }

        public bool IsContainerPinned(int containerId)
        {
            return GetFoundPinTypesById(containerId).Contains(PinType.Container);
        }

        public bool IsGroupPinned(int groupId)
        {
            return GetFoundPinTypesById(groupId).Contains(PinType.Group);
        }

        public bool IsErrorPinned(int errorId)
        {
            return GetFoundPinTypesById(errorId).Contains(PinType.Error);
        }

        #endregion
    }
}
