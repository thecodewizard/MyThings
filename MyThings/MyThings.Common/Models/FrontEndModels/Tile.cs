namespace MyThings.Common.Models
{
    public class Tile : RawJsonTile
    {
        //Reference Field
        public Pin Pin { get; set; }
    }

    public class RawJsonTile
    {
        //Fields
        public int Id { get; set; }
        public int Col { get; set; }
        public int Row { get; set; }
        public float Size_X { get; set; }
        public float Size_Y { get; set; }
    }
}
