using Demineur.Features;
namespace Demineur
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MineField field = new MineField();
            field.DrawArena(5, 5, 5, 5);
        }
    }
}
