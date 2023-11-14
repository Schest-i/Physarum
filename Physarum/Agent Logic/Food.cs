namespace Physarum.AgentLogic
{
    internal class Food
    {
        float x;
        float y;

        float radius;
        float pheromone;
        
        public Food(float x = 0, float y = 0, float Dispenseradius = 0, float pheromone = 0) 
        {
            this.x = x;
            this.y = y;
            this.pheromone = pheromone;
            radius = Dispenseradius;
        }
        
        public void DispencePheromone(ref float[,] Pmap) 
        {
            int x, y;
            for (float i = 0; i <= radius; i += 0.1f)
            {
                for (float j = 0; j < 2 * MathF.PI; j += MathF.PI / 180f)
                {
                    x = (int)MathF.Round(i * MathF.Cos(j) + this.x, 0 );
                    y = (int)MathF.Round(i * MathF.Sin(j) + this.y, 0 );
                    if (x < 0 || y < 0 || x >= Pmap.GetLength(0) || y >= Pmap.GetLength(1)) continue;
                    Pmap[x, y] += (pheromone / radius) * i ;
                }
            }
        }
    }
}
