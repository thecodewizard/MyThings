using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyThings.Common.Models;

namespace MyThings.Common.Repositories.BaseRepositories
{
    public class TileRepository : GenericRepository<Tile>
    {
        #region Functionality Methods

        public List<Tile> GetTiles()
        {
            return All().ToList();
        }

        public Tile GetTile(int tileId)
        {
            return GetByID(tileId);
        }

        public void DeleteTile(Tile tile)
        {
            Delete(tile);
            SaveChanges();
        }

        #endregion
    }
}
