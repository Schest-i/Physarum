using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Physarum.AgentLogic
{
    //i think crazy dont work
    internal class Agent
    {
        int x;
        int y;
        
        float speed;

        float angle;
        float pheromone;
        float threshold;

        bool angrystate; // false - normanl, true - crazy

        TimeSpan AngryTime;
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
        public void Update(Sensor sensor, GameTime GameTime)
        {
            int nX, nY;
            Pmap[x, y] += pheromone;
            
            nX = x + (int)MathF.Round((float)(speed * MathF.Cos(angle + sensor.angle)), 0);
            nY = y + (int)MathF.Round((float)(speed * MathF.Sin(angle + sensor.angle)), 0);

            
            nX = (int)MathF.Abs((Pmap.GetLength(0) + nX) % Pmap.GetLength(0));
            nY = (int)MathF.Abs((Pmap.GetLength(1) + nY) % Pmap.GetLength(1));

            x = nX; 
            y = nY;

            angle += sensor.angle * (float)random.NextDouble() * MathF.PI / 4f;

            if (AngryTime.Seconds + crazyTime <= GameTime.TotalGameTime.TotalSeconds) angrystate = false;
        }
        public Sensor GetBestSensor()
        {
            (int, float) SelectedSensor = (0, 0f); // (number of sensor, value)
            float Pvalue = 0;
            int x, y;
            for (int i = 0; i < sensors.Length; i++)
            {
                
                x = (int)MathF.Round((float)(sensors[i].length * MathF.Cos(angle + sensors[i].angle)), 0) + this.x;
                y = (int)MathF.Round((float)(sensors[i].length * MathF.Sin(angle + sensors[i].angle)), 0) + this.y;

               

                x = (int)MathF.Abs((Pmap.GetLength(0) + x) % Pmap.GetLength(0));
                y = (int)MathF.Abs((Pmap.GetLength(1) + y) % Pmap.GetLength(1));

                if (Pvalue < Pmap[x, y] && !angrystate)
                {
                    if (Pmap[x, y] >= threshold) angrystate = true;
                    SelectedSensor = (Array.IndexOf(sensors, sensors[i]), Pmap[x, y]);
                }
                else if(Pvalue > Pmap[x, y] && angrystate)
                {
                    SelectedSensor = (Array.IndexOf(sensors, sensors[i]), Pmap[x, y]);
                }
            }
            
            return sensors[SelectedSensor.Item1]; 
        }
    }
}
