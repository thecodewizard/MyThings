using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyThings.Common.Repositories;

namespace MyThings.Common.Models
{
    public class Pin
    {
        //Fields
        public int Id { get; set; }

        //Reference Fields
        public String UserId { get; set; }
        public int TileId { get; set; }

        //Saved Fields
        public int SavedId { get; set; }
        public PinType SavedType { get; set; }

        //Marking Field
        [NotMapped]
        public bool IsDeleted { get; private set; }

        //Functionality
        public Pin Save()
        {
            //Only use this method to create a single pin. With multiple pins, working with the repository directly is more efficient.
            PinRepository pinRepository = new PinRepository();
            if (this.Id == 0)
            {
                //The pin does not have an ID -> Add this to the database
                Pin pin = pinRepository.Insert(this);
                pinRepository.SaveChanges();
                this.Id = pin.Id;
                return pin;
            } else
            {
                //The pin has an ID -> Update the existing pin
                pinRepository.Update(this);
            }
            return this;
        }

        public void Delete()
        {
            //Only use this method to delete a single pin. With multiple pins, working with the repository directly is more efficient.
            PinRepository pinRepository = new PinRepository();
            pinRepository.DeletePin(this);
            this.IsDeleted = true;
        }
    }

    public enum PinType
    {
        Container, Sensor, Error, Group
    }
}
