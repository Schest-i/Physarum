using Microsoft.Xna.Framework;

namespace Physarum.AgentLogic
{
    //i think crazy dont work
    internal class Agent
    {
        public int x;
        public int y;
        
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


        public Agent(ref Sensor[] sensors, ref float[,] Pmap ,int x = 0, int y = 0, float speed = 0, float angle = 0, float pheromone = 0, float threshold = 0,int crazyTime = 0, bool angrystate = false)
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
            int nX, nY;

            int lX0, lY0;
            int lX1, lY1;
            int hX, hY;

            nX = x + (int)MathF.Round(speed * MathF.Cos(angle + sensor.angle), 0);
            nY = y + (int)MathF.Round(speed * MathF.Sin(angle + sensor.angle), 0);

            hX = nX / Pmap.GetLength(0);
            hY = nY / Pmap.GetLength(1);
            
            lX0 = x;
            lY0 = y;
            //доделать линии
            //while (true)
            //{
            //    (x + nX);   
            //    DrawLine(x, y, nX, nY); 

            //}

            nX = (int)MathF.Abs((Pmap.GetLength(0) + nX) % Pmap.GetLength(0));
            nY = (int)MathF.Abs((Pmap.GetLength(1) + nY) % Pmap.GetLength(1));

            DrawLine(x,y,nX,nY);

            x = nX; 
            y = nY;

            angle += sensor.angle * (float)random.NextDouble() * MathF.PI / 4f;

            if (StartTime + crazyTime <= gameTime.TotalGameTime.Seconds) angrystate = false;
        }
        public Sensor GetBestSensor(GameTime gameTime)
        {
            (int, float) SelectedSensor = (0, 0f); // (number of sensor, value)
            
            float Scheck = sensors[0].GetValue(ref Pmap, x, y);
            float OldPvalue = 0;
            float Pvalue = 0;

            bool SEquals = false;

            for (int i = 0; i < sensors.Length; i++)
            {

                Pvalue = sensors[i].GetValue(ref Pmap, x, y);
                
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
                else if(Pvalue > Pmap[x, y] && angrystate)
                {
                    OldPvalue = Pvalue;
                    SelectedSensor = (Array.IndexOf(sensors, sensors[i]), Pvalue);
                }
            }
            
            return sensors[SelectedSensor.Item1]; 
        }

        // Bresenham's line algorithm
        public void DrawLine(int x0, int y0, int x1, int y1)
        {
            int dx = (int)MathF.Abs(x1 - x0);
            int sx = x0 < x1 ? 1 : -1;
            int dy = -(int)MathF.Abs(y1 - y0);
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
                Pmap[x0, y0] += pheromone;
            }
        }
        
    }
}
