using System;

namespace Demineur
{
    /// <summary>
    /// Point d'entrée du programme.
    /// Orchestre le Menu, le MineField et la boucle de jeu.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            // Configuration de la console
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.CursorVisible = true;
            Console.Title = "Demineur";

            Menu menu = new Menu();
            bool playAgain = true;

            while (playAgain == true)
            {
                // ── 1. Écran de configuration ────────────────────────────────
                Console.Clear();
                menu.DisplayTitle();
                menu.AskGridSize();
                menu.AskDifficulty();

                // ── 2. Construction du plateau ───────────────────────────────
                MineField field = new MineField(menu.GetNbRows(), menu.GetNbCols(), menu.GetNbMines());

                Console.Clear();
                menu.DisplayTitle();
                menu.DisplayGameHeader();
                field.Draw();
                menu.DisplayInstructions(field.ConsoleWidth);
                field.PlaceCursor();

                // ── 3. Boucle de jeu ─────────────────────────────────────────
                bool gameOver = false;
                bool playerWon = false;

                while (gameOver == false)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);

                    if (key.Key == ConsoleKey.UpArrow ||
                        key.Key == ConsoleKey.DownArrow ||
                        key.Key == ConsoleKey.LeftArrow ||
                        key.Key == ConsoleKey.RightArrow)
                    {
                        field.MoveCursor(key.Key);
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        bool exploded = field.RevealCurrent();

                        if (exploded == true)
                        {
                            gameOver = true;
                            playerWon = false;
                        }
                    }
                    else if (key.Key == ConsoleKey.Spacebar)
                    {
                        field.ToggleFlagCurrent();
                    }
                    else if (key.Key == ConsoleKey.Escape)
                    {
                        gameOver = true;
                        playerWon = false;
                    }

                    // Mise à jour compteur de mines
                    menu.DisplayMineCounter(field.MinesLeft);

                    // Vérification de la victoire
                    if (gameOver == false)
                    {
                        bool won = field.CheckWin();

                        if (won == true)
                        {
                            gameOver = true;
                            playerWon = true;
                        }
                    }

                    // Repositionne le curseur sur la cellule active
                    if (gameOver == false)
                    {
                        field.PlaceCursor();
                    }
                }

                // ── 4. Fin de partie ─────────────────────────────────────────
                menu.DisplayEndMessage(playerWon, field.MinesLeft, field.ConsoleBottomRow);
                playAgain = menu.AskPlayAgain();
            }

            // ── 5. Au revoir ─────────────────────────────────────────────────
            Console.Clear();
            Console.CursorVisible = true;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Au revoir et a bientot !");
            Console.ResetColor();
        }
    }
}