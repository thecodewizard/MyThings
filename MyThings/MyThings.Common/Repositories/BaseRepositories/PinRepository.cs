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

        #region Static Pin Constructors

        public static Pin RenderClockPinForUser(String userid)
        {
            Pin clock = new Pin();
            clock.SavedId = 0;
            clock.SavedType = PinType.FixedClock;
            clock.UserId = userid;

            return clock;
        }

        public static List<Pin> RenderNavigationPinsForUser(String userid)
        {
            List<Pin> navigationpins = new List<Pin>();
            for (int i = 0; i < 6; i++)
            {
                Pin navItem = new Pin();
                navItem.SavedId = i;
                navItem.SavedType = PinType.FixedNavigation;
                navItem.UserId = userid;
                navigationpins.Add(navItem);
            }

            return navigationpins;
        }

        public static List<Pin> RenderErrorPinsForUser(String userid)
        {
            List<Pin> errorPins = new List<Pin>();
            for (int i = 0; i < 2; i++)
            {
                Pin errorItem = new Pin();
                errorItem.SavedId = i;
                errorItem.SavedType = PinType.FixedError;
                errorItem.UserId = userid;
                errorPins.Add(errorItem);
            }

            return errorPins;
        }

        #endregion
    }
}
