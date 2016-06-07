using System;
using System.Collections.Generic;
using System.Linq;
using MyThings.Common.Models;
using Newtonsoft.Json;

namespace MyThings.Common.Helpers
{
    public class GridsterHelper
    {
        public static String TileListToJson(List<Tile> tiles)
        {
            List<RawJsonTile> rawJsonTiles =
                (from t in tiles
                    orderby t.Id
                    select new RawJsonTile()
                    {
                        Id = t.Id,
                        Col = t.Col,
                        Row = t.Col,
                        Size_X = t.Size_X,
                        Size_Y = t.Size_Y
                    }).ToList();

            String json = JsonConvert.SerializeObject(rawJsonTiles);

            if (!String.IsNullOrEmpty(json)) return json;
            return null;
        }

        public static List<Tile> JsonToTileList(String json)
        {
            List<RawJsonTile> rawTiles = JsonConvert.DeserializeObject<List<RawJsonTile>>(json) ?? new List<RawJsonTile>();

            List<Tile> tiles = (from rt in rawTiles
                select new Tile()
                {
                    Id = rt.Id,
                    Col = rt.Col,
                    Row = rt.Row,
                    Size_X = rt.Size_X,
                    Size_Y = rt.Size_Y,
                }).ToList();

            return tiles;
        }
    }
}
