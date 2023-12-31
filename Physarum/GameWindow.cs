using Physarum.AgentLogic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Diagnostics;
using Microsoft.Xna.Framework.Input;

using static System.MathF;

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
    float DiffPercent;
    float Pheromone;
    float Speed;

    int CrazyTime;

    Food[] Foods = new Food[0];
    Agent[] Agents = new Agent[100000]; 
    Sensor[] Sensors = new Sensor[] { };


    SpriteBatch SpriteBatch;
    Texture2D PheromoneTexture;
    Texture2D AgentTexture;
    Texture2D SensorTexture;
    /*[[0.500 0.500 0.500] [0.500 0.500 0.500] [0.800 0.800 0.500] [0.000 0.200 0.500]]*/
    Vector3 a = new Vector3(0.5f, 0.5f, 0.5f);
    Vector3 b = new Vector3(0.5f, 0.5f, 0.5f);
    Vector3 c = new Vector3(0.8f, 0.8f, 0.5f);
    Vector3 d = new Vector3(0f, 0.2f, 0.5f);

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
        IsMouseVisible = true;

        PheromoneMap = new float[WindowWidth, WindowHeight];
        
        //init playing settings
        //Pheromone = rand.NextSingle();
        Pheromone = 10f;
        DiffPercent = 1f;
        WeatheringNum = 3f;
        Speed = 5f;
        CrazyTime = 3;

        //init Sensors
        //for (float i = 0; i <= 2f * PI; i += 2 * PI / 6f)
        //{
        //    Sensors = Sensors.Concat(new Sensor[] { new Sensor(ref PheromoneMap, i, 3f) }).ToArray();
        //}
        //Sensors = Sensors.Concat(new Sensor[] { new Sensor(0f, 3f) }).ToArray();
        for (float i = 0; i < Tau; i += Tau / 90f)
        {
            Sensors = Sensors.Concat(new Sensor[] { new Sensor(ref PheromoneMap, i, 1f) }).ToArray();
        }
        //Sensors = Sensors.Concat(new Sensor[] { new Sensor(ref PheromoneMap, Tau / 3f, 3f) }).ToArray();
        //Sensors = Sensors.Concat(new Sensor[] { new Sensor(ref PheromoneMap, 2 * Tau / 3, 3f) }).ToArray();
        //Sensors = Sensors.Concat(new Sensor[] { new Sensor(ref PheromoneMap, Tau, 3f) }).ToArray();

        //init agents
        for (int i = 0; i < Agents.Length; i++)
        {
            Agents[i] = new Agent(ref Sensors, ref PheromoneMap, (int)(WindowWidth / 2f), (int)(WindowHeight / 2f), Speed, Pheromone, CrazyTime);
        }

        //init foods
        for (int i = 0; i < Foods.Length; i++)
        {
            Foods[i] = new Food(ref PheromoneMap, WindowWidth / 2f, WindowHeight / 2f, 1f, 0.1f);
        }
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
        Console.WriteLine($"Update called");
        int Mx, My;

        Mx = Mouse.GetState().X;
        Mx = (int)Max(0, Mx);       
        Mx = (int)Min(_device.Viewport.Width - 1, Mx);
        My = Mouse.GetState().Y;
        My = (int)Max(0, My);
        My = (int)Min(_device.Viewport.Height - 1, My);

        float Ph = PheromoneMap[Mx, My];
        Window.Title = $"X:{Mx} Y:{My} Ph:{Ph}";
        for (int i = 0; i < Foods.Length; i++)
        {
            Foods[i].DispencePheromone(ref PheromoneMap);
        }
        for (int i = 0; i < Agents.Length; i++)
        {
            Agents[i].Update(gameTime);
        }

        Diffusion();
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

        if (Debugger.IsAttached) 
        {
            for (int i = 0; i < Agents.Length; i++)
            {
                x = (int)Agents[i].x;
                y = (int)Agents[i].y;
                SpriteBatch.Draw(AgentTexture, new Vector2(x, y), Color.White);
            }
        }
        SpriteBatch.End();
        
        base.Draw(gameTime);
    }
#endregion ProgramBody
#region Methods
    internal void Weathering() 
    {
        for (int i = 0; i < WindowWidth; i++)
        {
            for (int j = 0; j < WindowHeight; j++)
            {
                PheromoneMap[i, j] -= WeatheringNum;
                if (PheromoneMap[i, j] < 0) PheromoneMap[i, j] = 0;
            }
        }
    }
    
    internal void Diffusion()
    {
        
        float[,] NewPmap = new float[WindowWidth, WindowHeight];
        
        float CellDiffVal;
        
        int x, y;

        for (int i = 0; i < WindowWidth; i++)
        {
            for (int j = 0; j < WindowHeight; j++)
            {
                CellDiffVal = PheromoneMap[i, j] * DiffPercent / 9f;
                NewPmap[i, j] = PheromoneMap[i, j] * (1f - DiffPercent);
                for (int k = -1; k <= 1; k++)
                {
                    for (int l = -1; l <= 1; l++)
                    {       
                        x = (int)Abs((WindowWidth + i + k) % WindowWidth);
                        y = (int)Abs((WindowHeight + j + l) % WindowHeight);
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
        for (int i = 0; i < WindowHeight/*y*/; i++)
        {
            for (int j = 0; j < WindowWidth/*x*/; j++)
            {
                item = PheromoneMap[j, i];

                if (item < MinMax.Item1) MinMax.Item1 = item;
                if (item > MinMax.Item2) MinMax.Item2 = item;
            }
        }
        return MinMax;
    }
    //починить цвета
    internal Color GetColor(float value)
    {
        Vector3 color;

        color.X = a.X + b.X * Cos(Tau * (c.X * value + d.X));
        color.Y = a.Y + b.Y * Cos(Tau * (c.Y * value + d.Y));
        color.Z = a.Z + b.Z * Cos(Tau * (c.Z * value + d.Z));   

        return new Color(color.X, color.Y, color.Z);
    }
    internal Color[] MapToTexture() 
    {
        Color[] ColorMap = new Color[PheromoneTexture.Width * PheromoneTexture.Height];
        //float range = Pmax - Pmin;
        int ArrayOffset = WindowWidth;

        for (int i = 0; i < WindowHeight; i++) //j - x     i - y
        {
            for (int j = 0; j < WindowWidth; j++)
            {
                //float temp = (float)j / PheromoneMap.GetLength(0);
                //Color color = new Color(0f, 0f, (PheromoneMap[j, i] - Pmin) * range );
                //ColorMap[ArrayOffset * i + j] = GetColor((PheromoneMap[j, i] - Pmin) * range);
                ColorMap[ArrayOffset * i + j] = GetColor(PheromoneMap[j, i] / Pmax);

            }
        }
        return ColorMap;
    }
#endregion Methods
}