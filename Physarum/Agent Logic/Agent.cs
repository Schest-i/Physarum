using Microsoft.Xna.Framework;
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

        float speed;
        float angle;
        float pheromone;
        float threshold;
        
        bool agentstate; // false - normanl, true - crazy

        int StartTime;
        int crazyTime;

        Sensor[] sensors;
        float[] weightmap;

        Random random = new Random();
        float[,] Pmap;

        int WindowWidth;
        int WindowHeight; 
        
        public Agent(ref Sensor[] sensors, ref float[,] Pmap, float x = 0, float y = 0, float speed = 0, float angle = 0, float pheromone = 0, float threshold = 0,int crazyTime = 0, bool angrystate = false)
        {
            this.x = x;
            this.y = y;

            this.speed = speed;
            this.angle = angle;
            this.pheromone = pheromone;

            this.crazyTime = crazyTime;
            this.threshold = threshold;
            this.agentstate = angrystate;

            this.sensors = sensors;
            this.Pmap = Pmap;

            WindowWidth = Pmap.GetLength(0);
            WindowHeight = Pmap.GetLength(1);

            weightmap = new float[sensors.Length]; 
        }
        public void Update(GameTime gameTime)
        {
            UpdateWeightMap(gameTime);
            var sensor = GetSensor();

            float nX = coordinates.X + (int)Round(speed * Cos(angle + sensor.angle), 0);
            float nY = coordinates.Y + (int)Round(speed * Sin(angle + sensor.angle), 0);

            DrawLine(
                (int)x,
                (int)y,
                (int)Round(nX, 0),
                (int)Round(nY, 0)
                );

            x = (WindowWidth + nX) % WindowWidth;
            y = (WindowHeight + nY) % WindowHeight;
            

            angle += sensor.angle * random.NextSingle() * PI / 4f;

            if (StartTime + crazyTime <= gameTime.TotalGameTime.Seconds) agentstate = false;
        }
        public Sensor GetSensor()
        {
            //fix states
            if (!agentstate || true)
            {
                float max = weightmap.Max(); 
                for (int i = 0; i < weightmap.Length; i++)
                {
                    if (weightmap[i] == max)
                    {
                        return sensors[i];
                    }
                }
            }
            if (agentstate)
            {
                //int WorseElement = 0;

                //for (int i = 0; i < weightmap.Length; i++)
                //{
                //    if (weightmap[WorseElement] > weightmap[i])
                //    {
                //        WorseElement = i;
                //    }
                //}
                //if (weightmap.Where(w => w.Equals(WorseElement)).Count() > 1)
                //{
                //    int[] ElementsId = new int[0];
                //    for (int i = 0; i < weightmap.Length; i++)
                //    {
                //        if (weightmap[i] == weightmap[WorseElement])
                //        {
                //            ElementsId.Concat(new int[] { i });
                //        }
                //    }
                //    return sensors[ElementsId[random.Next(ElementsId.Length - 1)]];
                //}
                //return sensors[WorseElement];
                return new Sensor(ref Pmap);    
            }
            return new Sensor(ref Pmap);
        }

        // Bresenham's line algorithm + schest fix for cutted lines
        public void DrawLine(int x0, int y0, int x1, int y1)
        {
            int dx = (int)Abs(x1 - x0);
            int sx = x0 < x1 ? 1 : -1;
            int dy = -(int)Abs(y1 - y0);
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
        private void UpdateWeightMap(GameTime gameTime)
        {
            float weight;
            for (int i = 0; i < sensors.Length; i++)
            {
                weight = sensors[i].GetValue((int)x, (int)y);
                if (threshold < weight & !agentstate) 
                {
                    agentstate = true;
                    StartTime = gameTime.TotalGameTime.Seconds;
                }
                weightmap[i] = weight;
            }
        }
    }
}