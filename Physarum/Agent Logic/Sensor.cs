using static System.MathF;
using Microsoft.Xna.Framework;

namespace Physarum.AgentLogic
{
    internal class Sensor
    {
        public float angle;
        public float length;

        float[,] Pmap;

        int WindowHeight;
        int WindowWidth; 

        public Sensor(ref float[,] Pmap, float angle = 0, float length = 0)
        {
            this.angle = angle;
            this.length = length;

            this.Pmap = Pmap;

            WindowWidth = Pmap.GetLength(0);
            WindowHeight = Pmap.GetLength(1);
        }
        public float GetValue(int x, int y, float angle) 
        {
            int MapX, MapY;
            MapX = (int)Round(length * Cos(this.angle + angle), 0) + x;
            MapY = (int)Round(length * Sin(this.angle + angle), 0) + y;

            MapX = (int)Abs((WindowWidth + MapX) % WindowWidth);
            MapY = (int)Abs((WindowHeight + MapY) % WindowHeight);
            
            return Pmap[MapX, MapY];
        }
        public Vector2 DebugDraw(int x, int y, float angle) 
        {
            int X, Y;
            X = (int)Round(length * Cos(this.angle + angle), 0) + x;
            Y = (int)Round(length * Sin(this.angle + angle), 0) + y;

            X = (int)Abs((WindowWidth + X) % WindowWidth);
            Y = (int)Abs((WindowHeight + Y) % WindowHeight);
            
            return new Vector2(X, Y);
        }
    }
}
