using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Physarum.AgentLogic;
using System.Diagnostics;
using static System.MathF;
using Color = Microsoft.Xna.Framework.Color;
namespace Physarum.Components
{
    internal class PhysarumComponent : DrawableGameComponent
    {
#pragma warning disable 8618

        #region Variables
        private GraphicsDevice _device
        {
            get { return GraphicsDevice; }
            set { }
        }
        Game game;

        Random rand = new Random();
        float Mx, My;

        int WindowHeight = 600;
        int WindowWidth = 800;

        float[,] PheromoneMap;
        float Pmin;
        float Pmax;

        float WeatheringNum;
        float DiffPercent;

        float Wigle;
        float Pheromone;
        float Speed;

        int CrazyTime;
        int Threshold;

        List<Food> Foods = new(0);
        List<Agent> Agents = new(40000);
        List<Sensor> Sensors = new();


        SpriteBatch SpriteBatch;
        Texture2D DebugTexture;

        Texture2D PheromoneTexture;
        Texture2D AgentTexture;
        Texture2D AngryAgentTexture;
        Texture2D SensorTexture;
        /*[[0.500 0.500 0.500] [0.500 0.500 0.500] [0.800 0.800 0.500] [0.000 0.200 0.500]]*/
        Vector3 a = new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 b = new Vector3(0.5f, 0.5f, 0.5f);
        Vector3 c = new Vector3(0.8f, 0.8f, 0.5f);
        Vector3 d = new Vector3(0f, 0.2f, 0.5f);
        Sensor[] sensors;
        #endregion Varibles
        #region Constructor
        public PhysarumComponent(Game game) : base(game)
        {
            this.game = game;
        }
        #endregion Constructor
        #region ProgramBody
        public override void Initialize()
        {
            PheromoneMap = new float[WindowWidth, WindowHeight];

            sensors = new Sensor[]
            {
                new Sensor(ref PheromoneMap, PI / 2, 4f),
                new Sensor(ref PheromoneMap, 0f - PI / 2f, 4f),
                new Sensor(ref PheromoneMap, 0f, 4f)
            };

            DiffPercent = 0.4f;
            WeatheringNum = 0.01f;

            Wigle = 0;
            Pheromone = 3f;
            Speed = 3f;

            CrazyTime = 4;
            Threshold = 30;

            InitSensors(sensors);

            InitAgents();

            InitFood();

            base.Initialize();
        }

        protected override void LoadContent()
        {

            PheromoneTexture = new Texture2D(_device, WindowWidth, WindowHeight);

            AgentTexture = new Texture2D(_device, 1, 1);
            AgentTexture.SetData(new Color[] { Color.Green });

            AngryAgentTexture = new Texture2D(_device, 1, 1);
            AngryAgentTexture.SetData(new Color[] { Color.Red });

            SensorTexture = new Texture2D(_device, 1, 1);
            SensorTexture.SetData(new Color[] { Color.Black });

            SpriteBatch = new SpriteBatch(_device);
            base.LoadContent();
        }

        protected override void UnloadContent()
        {

            base.UnloadContent();
        }
        public override void Update(GameTime gameTime)
        {
            Mx = Mouse.GetState().X;
            Mx = Max(0, Mx);
            Mx = Min(_device.Viewport.Width - 1, Mx);
            My = Mouse.GetState().Y;
            My = Max(0, My);
            My = Min(_device.Viewport.Height - 1, My);

            float Ph = PheromoneMap[(int)Mx, (int)My];
            game.Window.Title = $"FPS:{1f / (float)gameTime.ElapsedGameTime.TotalSeconds} X:{Mx} Y:{My} Ph:{Ph}";



            for (int i = 0; i < Foods.Count; i++)
            {
                Foods[i].DispencePheromone(ref PheromoneMap);
            }
            for (int i = 0; i < Agents.Count; i++)
            {
                Agents[i].Update(gameTime);
            }

            Diffusion();
            Weathering();

            (Pmin, Pmax) = GetMinMax();
            PheromoneTexture.SetData(MapToTexture());
            base.Update(gameTime);
        }
        public override void Draw(GameTime gameTime)
        {
            _device.Clear(Color.White);
            SpriteBatch.Begin();
            SpriteBatch.Draw(PheromoneTexture, new Vector2(0f, 0f), Color.White);

            if (Debugger.IsAttached)
            {
                for (int i = 0; i < Agents.Count; i++)
                {
                    if (Agents[i].IsAngry) SpriteBatch.Draw(AngryAgentTexture, new Vector2(Agents[i].x, Agents[i].y), Color.White);
                    else SpriteBatch.Draw(AgentTexture, new Vector2(Agents[i].x, Agents[i].y), Color.White);
                    foreach (var item in Sensors)
                    {
                        SpriteBatch.Draw(SensorTexture, item.DebugDraw((int)Agents[i].x, (int)Agents[i].y, Agents[i].Angle), Color.White);
                    }
                }
            }
            SpriteBatch.End();

            base.Draw(gameTime);
        }
        #endregion ProgramBody
        #region Methods
        private void InitSensors(Sensor[] sensors)
        {
            Sensors.Clear();
            for (int i = 0; i < sensors.Length; i++)
            {
                Sensors.Add(sensors[i]);
            }
        }
        private void InitAgents()
        {
            for (int i = 0; i < Agents.Capacity; i++)
            {
                Agents.Add(new Agent(Sensors, ref PheromoneMap, new Vector2(WindowWidth * rand.NextSingle(), WindowHeight * rand.NextSingle()), Speed, Tau * rand.NextSingle(), Wigle, Pheromone, Threshold, CrazyTime));
            }
        }
        private void InitFood()
        {
            for (int i = 0; i < Foods.Count; i++)
            {
                Foods[i] = new Food(ref PheromoneMap, WindowWidth / 2f, WindowHeight / 2f, 1f, 0.1f);
            }
        }
        private void Weathering()
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
        private void Diffusion()
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
        private (float, float) GetMinMax()
        {
            (float, float) MinMax = (float.MaxValue, 0f);
            float item;
            for (int i = 0; i < WindowHeight; i++)
            {
                for (int j = 0; j < WindowWidth; j++)
                {
                    item = PheromoneMap[j, i];

                    if (item < MinMax.Item1) MinMax.Item1 = item;
                    if (item > MinMax.Item2) MinMax.Item2 = item;
                }
            }
            return MinMax;
        }
        private Color GetColor(float value) =>
            new Color(
                    a.X + b.X * Cos(Tau * (c.X * value + d.X)),
                    a.Y + b.Y * Cos(Tau * (c.Y * value + d.Y)),
                    a.Z + b.Z * Cos(Tau * (c.Z * value + d.Z))
                );
        private Color[] MapToTexture()
        {
            Color[] ColorMap = new Color[PheromoneTexture.Width * PheromoneTexture.Height];

            int ArrayOffset = WindowWidth;

            for (int i = 0; i < WindowWidth; i++)
            {
                for (int j = 0; j < WindowHeight; j++)
                {
                    ColorMap[ArrayOffset * j + i] = GetColor(PheromoneMap[i, j] / Pmax);
                }
            }
            return ColorMap;
        }
        #region RandomizeMethods
        public void RandomizeShaderColor()
        {
            a = new Vector3(10 * rand.NextSingle(), 10 * rand.NextSingle(), 10 * rand.NextSingle());
            b = new Vector3(10 * rand.NextSingle(), 10 * rand.NextSingle(), 10 * rand.NextSingle());
            c = new Vector3(10 * rand.NextSingle(), 10 * rand.NextSingle(), 10 * rand.NextSingle());
            d = new Vector3(10 * rand.NextSingle(), 10 * rand.NextSingle(), 10 * rand.NextSingle());
            
        }
        public void RandomizeGameRules() 
        {
            DiffPercent = rand.NextSingle();
            WeatheringNum = rand.NextSingle();
        }
        public void RandomizeAgentRules()
        {
            Wigle = rand.NextSingle() * PI;
            Pheromone = rand.Next(1, 100) * rand.NextSingle();
            Speed = rand.Next(1, 100) * rand.NextSingle();

            CrazyTime = rand.Next(0, 100);
            Threshold = rand.Next(1, 100);
        }
        public void RandomizeSensors()
        {
            Sensor[] sensors = new Sensor[rand.Next(1, 10)];
            for (int i = 0; i < sensors.Length; i++)
            {
                sensors[i] = new Sensor(ref PheromoneMap, rand.NextSingle() * Tau, rand.NextSingle() * Speed * 2);
            }
            InitSensors(sensors);
        }
        #endregion
        #endregion Methods
    }
}