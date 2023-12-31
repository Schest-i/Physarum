using static System.MathF;
namespace Physarum.AgentLogic
{
    internal class Food
    {
        float x;
        float y;

        float radius;
        float pheromone;

        int WindowHeight;
        int WindowWidth;

        public Food(ref float[,] Pmap, float x = 0, float y = 0, float Dispenseradius = 0, float pheromone = 0) 
        {
            this.x = x;
            this.y = y;

            this.pheromone = pheromone;
            radius = Dispenseradius;

            WindowWidth = Pmap.GetLength(0);
            WindowHeight = Pmap.GetLength(1);
        }
        
        public void DispencePheromone(ref float[,] Pmap) 
        {
            int x, y;
            for (float i = 0; i <= radius; i += 0.1f)
            {
                for (float j = 0; j < Tau; j += PI / 180f)
                {
                    x = (int)Round(i * Cos(j) + this.x, 0 );
                    y = (int)Round(i * Sin(j) + this.y, 0 );
                    x = (WindowWidth + x) % WindowWidth;
                    y = (WindowHeight + y) % WindowHeight;
                    Pmap[x, y] += (pheromone / radius) * i ;
                }
            }
        }
    }
}
