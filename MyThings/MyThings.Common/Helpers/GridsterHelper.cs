using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyThings.Common.Models;
using Newtonsoft.Json;

namespace MyThings.Common.Helpers
{
    public class GridsterHelper
    {
        public static String TileListToJson(List<Tile> tiles)
        {
            List<RawGridsterTile> filteredGridsterTiles =
                (from t in tiles
                    orderby t.Col, t.Row
                    select new RawGridsterTile()
                    {
                        Col = t.Col,
                        Row = t.Row,
                        Size_X = t.Size_X,
                        Size_Y = t.Size_Y
                    }).ToList();
            String json = JsonConvert.SerializeObject(filteredGridsterTiles);

            if (!String.IsNullOrEmpty(json)) return json;
            return null;
        }

        public static List<Tile> JsonToTileList(String json)
        {
            List<RawGridsterTile> rawTiles = JsonConvert.DeserializeObject<List<RawGridsterTile>>(json) ?? new List<RawGridsterTile>();

            List<Tile> tiles = (from rt in rawTiles
                select new Tile()
                {
                    Col = rt.Col,
                    Row = rt.Row,
                    Size_X = rt.Size_X,
                    Size_Y = rt.Size_Y,
                }).ToList();

            return tiles; //TODO: Recycle Already existing tiles?
        }

        public static List<Tile> RichJsonToTileList(String json)
        {
            return JsonConvert.DeserializeObject<List<Tile>>(json) ?? new List<Tile>();
        }
    }
}
