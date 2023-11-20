using Microsoft.Xna.Framework;

namespace Physarum.AgentLogic
{
    internal class Sensor
    {
        public float angle;
        public float length;
        public Sensor(float angle = 0, float length = 0)
        {
            this.angle = angle;
            this.length = length;
        }
        public float GetValue(ref float[,] Pmap, int x, int y) 
        {
            float result;
            int MapX, MapY;
            MapX = (int)MathF.Round((float)(length * MathF.Cos(angle + angle)), 0) + x;
            MapY = (int)MathF.Round((float)(length * MathF.Sin(angle + angle)), 0) + y;

            MapX = (int)MathF.Abs((Pmap.GetLength(0) + MapX) % Pmap.GetLength(0));
            MapY = (int)MathF.Abs((Pmap.GetLength(1) + MapY) % Pmap.GetLength(1));
            
            return Pmap[MapX, MapY];
        }
    }
}
