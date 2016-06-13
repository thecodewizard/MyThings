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
            String json = JsonConvert.SerializeObject(tiles);

            if (!String.IsNullOrEmpty(json)) return json;
            return null;
        }

        public static List<Tile> JsonToTileList(String json)
        {
            return JsonConvert.DeserializeObject<List<Tile>>(json) ?? new List<Tile>();
        }
    }
}
