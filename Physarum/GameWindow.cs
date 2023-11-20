using Physarum.AgentLogic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

class GameWindow : Game
{
#region Varibles
    Random rand = new Random();

    private GraphicsDevice _device
    {
        get { return GraphicsDevice; }
        set { }
    }

    int WindowHeight = 600;
    int WindowWidth = 800;

    float[,] PheromoneMap;
    float Pmin;
    float Pmax;

    float WeatheringNum;
    float DiffValue;
    float Pheromone;
    float Speed;
    int CrazyTime;

    Food[] Foods = new Food[1];
    Agent[] Agents = new Agent[2000]; 
    Sensor[] Sensors = new Sensor[] { };

    SpriteBatch SpriteBatch;
    Texture2D PheromoneTexture;
    Texture2D AgentTexture;
    Texture2D SensorTexture;

#endregion Varibles
#region Constructor
    public GameWindow()
    {
        var GraphicsDeviceManager = new GraphicsDeviceManager(this);
        IsFixedTimeStep = false;
        GraphicsDeviceManager.PreferredBackBufferWidth = WindowWidth;
        GraphicsDeviceManager.PreferredBackBufferHeight = WindowHeight;
    }
#endregion Constructor
#region ProgramBody
    protected override void Initialize()
    {
        PheromoneMap = new float[WindowWidth, WindowHeight];
        
        //init playing settings
        WeatheringNum = rand.NextSingle() * rand.NextSingle();
        //Pheromone = rand.NextSingle();
        Pheromone = 10f;
        Speed = rand.NextSingle() * 10;
        CrazyTime = (int)MathF.Round(rand.NextSingle() * 10, 0);
        
        //init Sensors
        //for (float i = 0; i <= 2f * MathF.PI; i += 2 * MathF.PI / 6f)
        //{
        //    Sensors = Sensors.Concat(new Sensor[] {new Sensor(i, 3f)}).ToArray();
        //}
        //Sensors = Sensors.Concat(new Sensor[] { new Sensor(0f, 3f) }).ToArray();
        Sensors = Sensors.Concat(new Sensor[] { new Sensor(MathF.PI / 9f, 3f) }).ToArray();
        Sensors = Sensors.Concat(new Sensor[] { new Sensor(MathF.PI - MathF.PI / 9f, 3f) }).ToArray();
        
        //init agents
        for (int i = 0; i < Agents.Length; i++)
        {
            Agents[i] = new Agent(ref Sensors, ref PheromoneMap, x: rand.Next(0, WindowWidth - 1), y: rand.Next(0, WindowHeight - 1), pheromone: Pheromone, speed: Speed, crazyTime: CrazyTime);

        }
        
        //init foods
        //Foods[0] = new Food(WindowWidth / 2f, WindowHeight / 2f, 10f, 2f);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        //AgentTexture = Content.Load<Texture2D>("AgentSprite");
        //SensorTexture = Content.Load<Texture2D>("SensorSprite");

        PheromoneTexture = new Texture2D(_device, WindowWidth, WindowHeight);
        AgentTexture = new Texture2D(_device, 1, 1);
        AgentTexture.SetData(new Color[] { Color.Green });
        SpriteBatch = new SpriteBatch(_device);
        base.LoadContent();
    }

    protected override void UnloadContent()
    {

        base.UnloadContent();
    }
    protected override void Update(GameTime gameTime)
    {
        //for (int i = 0; i < Foods.Length; i++)
        //{
        //    Foods[i].DispencePheromone(ref PheromoneMap);
        //}
        for (int i = 0; i < Agents.Length; i++)
        {
            Agents[i].Update(Agents[i].GetBestSensor(gameTime), gameTime);
        }

        Diffusion(DiffValue);
        Weathering();
        
        (Pmin, Pmax) = GetMinMax();
        
        PheromoneTexture.SetData(MapToTexture());
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        int x, y;
        _device.Clear(Color.White);
        SpriteBatch.Begin();
        SpriteBatch.Draw(PheromoneTexture, new Vector2(0f, 0f), Color.White);
#if DEBUG 
        for (int i = 0; i < Agents.Length; i++)
        {
            x = Agents[i].x;
            y = Agents[i].y;
            SpriteBatch.Draw(AgentTexture, new Vector2(x, y), Color.White);
        }
#endif
        SpriteBatch.End();
        
        base.Draw(gameTime);
    }
#endregion ProgramBody
#region Methods
    internal void Weathering() 
    {
        for (int i = 0; i < PheromoneMap.GetLength(0); i++)
        {
            for (int j = 0; j < PheromoneMap.GetLength(1); j++)
            {
                PheromoneMap[i, j] -= WeatheringNum;
                if (PheromoneMap[i, j] < 0) PheromoneMap[i, j] = 0;
            }
        }
    }
    
    internal void Diffusion(float DiffPercent)
    {
        float[,] NewPmap = new float[PheromoneMap.GetLength(0), PheromoneMap.GetLength(1)];
        
        float CellDiffVal;
        
        int x, y;

        for (int i = 0; i < PheromoneMap.GetLength(0); i++)
        {
            for (int j = 0; j < PheromoneMap.GetLength(1); j++)
            {
                CellDiffVal = PheromoneMap[i, j] * DiffPercent / 9f;
                NewPmap[i, j] = PheromoneMap[i, j] * (1f - DiffPercent);
                for (int k = -1; k <= 1; k++)
                {
                    for (int l = -1; l <= 1; l++)
                    {       
                        x = (int)MathF.Abs((PheromoneMap.GetLength(0) + i + k) % PheromoneMap.GetLength(0));
                        y = (int)MathF.Abs((PheromoneMap.GetLength(1) + j + l) % PheromoneMap.GetLength(1));
                        NewPmap[x, y] += CellDiffVal;
                    }
                }
            }
        }
        Array.Copy(NewPmap, PheromoneMap, PheromoneMap.Length);
    }

    internal (float, float) GetMinMax() 
    {
        (float, float) MinMax = (float.MaxValue,0f);
        float item;
        for (int i = 0; i < PheromoneMap.GetLength(1)/*y*/; i++)
        {
            for (int j = 0; j < PheromoneMap.GetLength(0)/*x*/; j++)
            {
                item = PheromoneMap[j, i];

                if (item < MinMax.Item1) MinMax.Item1 = item;
                if (item > MinMax.Item2) MinMax.Item2 = item;
            }
        }
        return MinMax;
    }
    internal Color[] MapToTexture() 
    {
        Color[] ColorMap = new Color[PheromoneTexture.Width * PheromoneTexture.Height];
        float OnePercent = 255f / Pmax;
        int ArrayOffset = PheromoneMap.GetLength(0); // 800

        for (int i = 0; i < PheromoneMap.GetLength(1); i++) //j - x     i - y
        {
            for (int j = 0; j < PheromoneMap.GetLength(0); j++)
            {
                ColorMap[ArrayOffset * i + j].R = 255;
                ColorMap[ArrayOffset * i + j].G = 0;
                ColorMap[ArrayOffset * i + j].B = 0;
                ColorMap[ArrayOffset * i + j].A = (byte)MathF.Round(PheromoneMap[j, i] * OnePercent, 0);
            }
        }
        return ColorMap;
    }
#endregion Methods
}