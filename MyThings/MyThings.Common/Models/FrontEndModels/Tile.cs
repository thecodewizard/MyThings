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
        public Pin Pin { get; set; }
    }
}
