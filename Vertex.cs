
using MongoDB.Bson.Serialization.Attributes;

namespace Models
{
    public class Vertex
    {      
        private int x;
        private int y;
        public Vertex(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        [BsonElement("X")]
        public int X
        {
            get
            {
                return this.x;
            }

            set
            {
                this.x = value;
            }
        }


        [BsonElement("Y")]
        public int Y 
        {
            get
            {
                return this.y;
            }

            set
            {
                this.y = value;
            }
        }
    }
}
