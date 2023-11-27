using Microsoft.Xna.Framework;
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
        
        bool angrystate; // false - normanl, true - crazy

        int StartTime;
        int crazyTime;


        Random random = new Random();
        Sensor[] sensors;
        float[,] Pmap;

        
        public Agent(ref Sensor[] sensors, ref float[,] Pmap, float x = 0, float y = 0, float speed = 0, float angle = 0, float pheromone = 0, float threshold = 0,int crazyTime = 0, bool angrystate = false)
        {
            this.x = x;
            this.y = y;

            this.speed = speed;
            this.angle = angle;
            this.pheromone = pheromone;

            this.crazyTime = crazyTime;
            this.threshold = threshold;
            this.angrystate = angrystate;

            this.sensors = sensors;
            this.Pmap = Pmap;
            
        }
        public void Update(Sensor sensor, GameTime gameTime)
        {
            int oX = (int)x;
            int oY = (int)y;

            float nX = coordinates.X + (int)Round(speed * Cos(angle + sensor.angle), 0);
            float nY = coordinates.Y + (int)Round(speed * Sin(angle + sensor.angle), 0);

            DrawLine(
                oX,
                oY,
                (int)Round(nX, 0),
                (int)Round(nY, 0)
                );

            x = (Pmap.GetLength(0) + nX) % Pmap.GetLength(0);
            y = (Pmap.GetLength(1) + nY) % Pmap.GetLength(1);
            

            angle += sensor.angle * random.NextSingle() * PI / 4f;

            if (StartTime + crazyTime <= gameTime.TotalGameTime.Seconds) angrystate = false;
        }
        public Sensor GetBestSensor(GameTime gameTime)
        {
            (int, float) SelectedSensor = (0, 0f); // (number of sensor, value)
            
            float Scheck = sensors[0].GetValue(ref Pmap, (int)x, (int)y);
            float OldPvalue = 0;
            float Pvalue = 0;

            bool SEquals = false;

            for (int i = 0; i < sensors.Length; i++)
            {

                Pvalue = sensors[i].GetValue(ref Pmap, (int)x, (int)y);
                
                if (OldPvalue < Pvalue&& !angrystate)
                {
                    if (Pvalue >= threshold) 
                    {
                        angrystate = true;
                        StartTime = gameTime.TotalGameTime.Seconds;
                    }
                    OldPvalue = Pvalue;
                    SelectedSensor = (Array.IndexOf(sensors, sensors[i]), Pvalue);
                }
                else if(Pvalue > Pmap[(int)x, (int)y] && angrystate)
                {
                    OldPvalue = Pvalue;
                    SelectedSensor = (Array.IndexOf(sensors, sensors[i]), Pvalue);
                }
            }
            
            return sensors[SelectedSensor.Item1]; 
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
            Pmap[x0, y0] += pheromone;
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
                (Pmap.GetLength(0) + x0) % Pmap.GetLength(0),
                (Pmap.GetLength(1) + y0) % Pmap.GetLength(1)
                ] += pheromone;
            }
        }
        
    }
}