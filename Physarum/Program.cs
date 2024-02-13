namespace Physarum
{

    internal partial class Program
    {
        static void Main(string[] args)
        {
            

            Bootstrap();

            using var game = new GameWindow();

            game.Run();
        }
    }
}