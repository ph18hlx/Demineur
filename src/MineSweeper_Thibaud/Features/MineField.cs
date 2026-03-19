using static MineSweeper.Display.SuperConsole;

namespace Demineur.Features
{
    public class MineField
    {
        public void DrawArena(int x, int y, int width, int height)
        {
            // Ligne du haut
            DrawHorizontalLine(x, y, width, '─');

            // Ligne du bas
            DrawHorizontalLine(x, y + height - 1, width, '─');

            // Côté gauche
            DrawVerticalLine(x, y, height, '│');

            // Côté droit
            DrawVerticalLine(x + width - 1, y, height, '│');

            // Coins
            Console.SetCursorPosition(x, y);
            Console.Write('┌');
            Console.SetCursorPosition(x + width - 1, y);
            Console.Write('┐');
            Console.SetCursorPosition(x, y + height - 1);
            Console.Write('└');
            Console.SetCursorPosition(x + width - 1, y + height - 1);
            Console.Write('┘');
        }
    }
}
