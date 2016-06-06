using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyThings.Common.Repositories.BaseRepositories;

namespace MyThings.Common.Models
{
    public class Tile
    {
        //Fields
        public int Id { get; set; }
        public int Col { get; set; }
        public int Row { get; set; }
        public float Size_X { get; set; }
        public float Size_Y { get; set; }

        //Reference Field
        [NotMapped]
        public Pin Pin { get; set; }

        //Marking Field
        [NotMapped]
        public bool IsDeleted { get; private set; }

        //Functionality
        public Tile Save()
        {
            //Only use this method to create a single tile. With multiple tiles, working with the repository directly is more efficient.
            TileRepository tileRepository = new TileRepository();
            if (this.Id == 0)
            {
                //The tile does not have an ID -> Add this to the database
                Tile tile = tileRepository.Insert(this);
                tileRepository.SaveChanges();
                return tile;
            } else
            {
                //The tile has an ID -> Update the existing tile
                tileRepository.Update(this);
            }
            return this;
        }


        public void Delete()
        {
            //Only use this method to delete a single tile. With multiple pins, working with the repository directly is more efficient.
            TileRepository tileRepository = new TileRepository();
            tileRepository.DeleteTile(this);
            this.IsDeleted = true;
        }
    }

    public class RawGridsterTile
    {
        public int Col { get; set; }
        public int Row { get; set; }
        public float Size_X { get; set; }
        public float Size_Y { get; set; }
    }
}
