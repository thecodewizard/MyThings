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

        public static String PinsToJson(List<Pin> pins)
        {
            String json = JsonConvert.SerializeObject(pins);

            if (!String.IsNullOrWhiteSpace(json)) return json;
            return null;
        }

        public static List<Pin> JsonToPins(String json)
        {
            return JsonConvert.DeserializeObject<List<Pin>>(json) ?? new List<Pin>();
        }
    }
}
