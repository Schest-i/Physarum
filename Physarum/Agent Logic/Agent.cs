using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static System.MathF;

namespace Physarum.AgentLogic
{
    //i think crazy dont work
    internal class Agent
    {
        private Vector2 coordinates;

        public float x {
            get { return Round(coordinates.X, 0); }
            set { coordinates.X = value; }
        } 
        public float y {
            get { return Round(coordinates.Y, 0); }
            set { coordinates.Y = value; }
        }
        public float Angle {
            get { return angle; }
            set { _ = value; }
        }

        public bool IsAngry {
            get { return Angry; }
            set { _ = value; }
        }

        float speed;
        float angle;
        float pheromone;
        float threshold;

        bool Angry; // false - normanl, true - crazy
        int StartTime;
        int crazyTime;

        float WigleValue;

        List<Sensor> sensors;

        Random random;
        float[,] Pmap;

        int WindowWidth;
        int WindowHeight; 
        
        public Agent(List<Sensor> sensors, ref float[,] pmap, Vector2 coordinates, float speed = 0, float angle = 0f, float wigleAngle = 0, float pheromone = 0, float threshold = 0, int crazyTime = 0)
        {
            this.x = coordinates.X;
            this.y = coordinates.Y;
            
            this.speed = speed;
            this.angle = angle;
            this.WigleValue = wigleAngle;
            this.pheromone = pheromone;

            this.sensors = sensors;

            this.crazyTime = crazyTime;
            this.threshold = threshold;
            this.Angry = false;

            this.Pmap = pmap;

            random = new Random();

            WindowWidth = pmap.GetLength(0);
            WindowHeight = pmap.GetLength(1); 
        }
        public void Update(GameTime gameTime)
        {
            Sensor sensor = GetSensor(gameTime);
            
            int k = (int)Round(random.Next(-1, 1), 0, MidpointRounding.AwayFromZero);
            float randomAngle = k * WigleValue;

            float nX = coordinates.X + speed * Cos(angle + sensor.angle + randomAngle);
            float nY = coordinates.Y + speed * Sin(angle + sensor.angle + randomAngle);

            DrawLine(
                (int)x,
                (int)y,
                (int)Round(nX, 0),
                (int)Round(nY, 0)
                );

            x = (WindowWidth + nX) % WindowWidth;
            y = (WindowHeight + nY) % WindowHeight;
            

            angle += sensor.angle + randomAngle;

            if (StartTime + crazyTime <= gameTime.TotalGameTime.Seconds) Angry = false;
        }
        public Sensor GetSensor(GameTime gameTime)
        {

            float[] weightmap = GetWeightMap(gameTime);

            float weight = Angry ? weightmap.Min() : weightmap.Max();
            
            List<int> elementsIds = new List<int>();

            for (int i = 0; i < weightmap.Length; i++)
            {
                if (weightmap[i] == weight)
                {
                    elementsIds.Add(i);
                }
            }

            return sensors[elementsIds[random.Next(elementsIds.Count() - 1)]];
        }
        // Bresenham's line algorithm + schest fix for cutted lines
        private void DrawLine(int x0, int y0, int x1, int y1)
        {
            int dx = (int)Abs(x1 - x0);
            int dy = -(int)Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int error = dx + dy;
            int e2;

            Pmap[
                (WindowWidth + x0) % WindowWidth,
                (WindowHeight + y0) % WindowHeight
                ] += pheromone;
            while (true)
            {
                if (x0 == x1 && y0 == y1) break;
                e2 = 2 * error;
                if (e2 >= dy)
                {
                    if(x0 == x1) break;
                    error += dy;
                    x0 += sx;
                }
                if (e2 <= dx)
                {
                    if (y0 == y1) break;
                    error += dx;
                    y0 += sy;
                }
                Pmap[
                (WindowWidth + x0) % WindowWidth,
                (WindowHeight + y0) % WindowHeight
                ] += pheromone;
            }
        }
        private float[] GetWeightMap(GameTime gameTime)
        {
            float[] weightmap = new float[sensors.Count];
            float weight;

            for (int i = 0; i < sensors.Count; i++)
            {
                weight = sensors[i].GetValue((int)x, (int)y, angle);
                if (threshold < weight & !Angry) 
                {
                    Angry = true;
                    StartTime = gameTime.TotalGameTime.Seconds;
                }
                weightmap[i] = weight;
            }
            return weightmap;
        }
        //сделать debug draw
        //public void DebugDraw() 
        //{
        //    SpriteBatch.Draw(AgentTexture, new Vector2(Agents[i].x, Agents[i].y), Color.White);
        //    foreach (var item in Sensors)
        //    {
        //        SpriteBatch.Draw(AgentTexture, item.DebugDraw((int)Agents[i].x, (int)Agents[i].y, ), Color.White);
        //    }
        //}
    }
}