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
    }
}
