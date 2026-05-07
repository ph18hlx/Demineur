using System;

namespace Demineur
{
    /// <summary>
    /// Séquence 6-7 : finalisation du jeu, exploration des cases, affichage des mines,
    /// mise à jour du compteur, conditions de victoire/défaite, rejouer
    /// </summary>
    class Program
    {
        // ── Constantes de validation ─────────────────────────────────────────
        const int MIN_SIZE = 6;
        const int MAX_SIZE = 30;

        // Pourcentages de mines par difficulté
        const double PCT_EASY = 0.10;
        const double PCT_MEDIUM = 0.25;
        const double PCT_HARD = 0.40;

        // Constantes d'affichage du plateau
        const int MARGIN_LEFT = 3;
        const int MARGIN_TOP = 6;
        const int STEP_X = 4;
        const int STEP_Y = 2;

        // Caractères du plateau
        const char C_TOP_LEFT = '╔';
        const char C_TOP_RIGHT = '╗';
        const char C_BOT_LEFT = '╚';
        const char C_BOT_RIGHT = '╝';
        const char C_HORIZ = '═';
        const char C_VERT = '║';
        const char C_TEE_TOP = '╦';
        const char C_TEE_BOT = '╩';
        const char C_TEE_LEFT = '╠';
        const char C_TEE_RIGHT = '╣';
        const char C_CROSS = '╬';

        // Variables du jeu
        static int nbRows;
        static int nbCols;
        static int nbMines;
        static int difficulty;

        // Variables du curseur
        static int posL = 0;
        static int posC = 0;

        // Tableau des flags posés par le joueur
        static bool[,] flags;

        // Tableau 2D représentant le visuel du plateau (true = mine)
        static bool[,] hasMine;

        // Tableau 2D représentant les cases explorées
        static bool[,] revealed;

        // Nombre de mines restantes à trouver
        static int minesLeft;

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.CursorVisible = true;

            // 1. Affichage du titre
            DisplayTitle();

            // 2. Demande la taille de la grille
            AskGridSize();

            // 3. Demande la difficulté
            AskDifficulty();

            // 4. Calcul et mémorisation du nombre de mines
            nbMines = CalcNbMines();

            // 5. Tableau de jeu terminé + consignes + compteur mines
            Console.Clear();
            DisplayTitle();
            DisplayGameHeader();
            DrawBoard();
            DisplayInstructions();
            DisplayMineCounter();

            // 6. Initialisation des tableaux
            flags = new bool[nbRows, nbCols];
            hasMine = new bool[nbRows, nbCols];
            revealed = new bool[nbRows, nbCols];

            // 7. Placement aléatoire des mines dans le tableau
            PlaceMines();

            minesLeft = nbMines;

            bool playAgain = true;

            while (playAgain == true)
            {
                // Réinitialisation
                posL = 0;
                posC = 0;
                minesLeft = nbMines;
                flags = new bool[nbRows, nbCols];
                hasMine = new bool[nbRows, nbCols];
                revealed = new bool[nbRows, nbCols];

                PlaceMines();

                Console.Clear();
                DisplayTitle();
                DisplayGameHeader();
                DrawBoard();
                DisplayInstructions();
                DisplayMineCounter();

                PlaceCursor();

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
                        MoveCursor(key.Key);
                    }
                    else if (key.Key == ConsoleKey.Spacebar)
                    {
                        PlaceFlag();
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        if (flags[posL, posC] == true)
                        {
                            RemoveFlag();
                        }
                        else
                        {
                            bool exploded = RevealCell();

                            if (exploded == true)
                            {
                                gameOver = true;
                                playerWon = false;
                            }
                        }
                    }
                    else if (key.Key == ConsoleKey.Escape)
                    {
                        gameOver = true;
                        playerWon = false;
                    }

                    if (gameOver == false)
                    {
                        DisplayMineCounter();

                        bool won = CheckWin();
                        if (won == true)
                        {
                            gameOver = true;
                            playerWon = true;
                        }
                    }

                    if (gameOver == false)
                    {
                        PlaceCursor();
                    }
                }

                // Fin de partie
                DisplayEndMessage(playerWon);
                playAgain = AskPlayAgain();

                if (playAgain == true)
                {
                    Console.Clear();
                    DisplayTitle();
                    AskGridSize();
                    AskDifficulty();
                    nbMines = CalcNbMines();
                }
            }

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Au revoir et a bientot !");
            Console.ResetColor();
        }

        // ── Affichage du titre ───────────────────────────────────────────────

        /// <summary>
        /// Affiche le titre du jeu encadré d'étoiles en jaune
        /// </summary>
        static void DisplayTitle()
        {
            string stars = "**********************************************************************";
            string middle = "*                      Démineur simplifié                            *";

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(stars);
            Console.WriteLine(middle);
            Console.WriteLine(stars);
            Console.ResetColor();
            Console.WriteLine();
        }

        // ── Saisie des paramètres ────────────────────────────────────────────

        /// <summary>
        /// Demande et valide la taille de la grille (lignes + colonnes)
        /// </summary>
        static void AskGridSize()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Merci d'entrer le nombre de ligne et de colonne de votre plateau de jeux");
            Console.WriteLine("  en sachant qu'au minimum on a un plateau de 6 lignes x 6 colonnes !");
            Console.WriteLine("  et au maximum un plateau de 30 lignes x 30 colonnes !");
            Console.ResetColor();
            Console.WriteLine("------------------------------------------------------------------------");

            nbRows = ReadIntInRange("Nombre de ligne  : ", MIN_SIZE, MAX_SIZE);
            nbCols = ReadIntInRange("Nombre de colonne: ", MIN_SIZE, MAX_SIZE);
        }

        /// <summary>
        /// Demande et valide le niveau de difficulté
        /// </summary>
        static void AskDifficulty()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("  Merci d'entrer la difficulté pour votre jeu");
            Console.WriteLine("  en sachant que :");
            Console.WriteLine("        1 --> niveau facile");
            Console.WriteLine("        2 --> niveau moyen");
            Console.WriteLine("        3 --> niveau difficile");
            Console.ResetColor();
            Console.WriteLine("------------------------------------------------------------------------");

            difficulty = ReadIntInRange("Votre difficulté : ", 1, 3);
        }

        /// <summary>
        /// Lit un entier dans un intervalle donné avec validation et messages d'erreur
        /// </summary>
        /// <param name="prompt">Message affiché avant la saisie</param>
        /// <param name="min">Valeur minimale acceptée</param>
        /// <param name="max">Valeur maximale acceptée</param>
        /// <returns>Entier valide saisi par l'utilisateur</returns>
        static int ReadIntInRange(string prompt, int min, int max)
        {
            int value = 0;
            bool ok = false;

            while (ok == false)
            {
                Console.Write(prompt);
                bool isNumber = int.TryParse(Console.ReadLine(), out value);

                if (isNumber == false)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Votre valeur n'est pas un nombre ! Merci de réessayer !");
                    Console.ResetColor();
                }
                else if (value < min || value > max)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Valeur hors limite ! Merci de réessayer !");
                    Console.ResetColor();
                }
                else
                {
                    ok = true;
                }
            }

            return value;
        }

        // ── Calcul du nombre de mines ────────────────────────────────────────

        /// <summary>
        /// Calcule le nombre de mines selon la grille et la difficulté.
        /// Formule : Surface = (nbRows/2 + 1) * (nbCols/2 + 1)
        /// </summary>
        /// <returns>Nombre de mines à placer (minimum 1)</returns>
        static int CalcNbMines()
        {
            int surface = (nbRows / 2 + 1) * (nbCols / 2 + 1);
            double pct = PCT_EASY;

            if (difficulty == 2)
            {
                pct = PCT_MEDIUM;
            }
            else if (difficulty == 3)
            {
                pct = PCT_HARD;
            }

            int mines = (int)(surface * pct);

            if (mines < 1)
            {
                mines = 1;
            }

            return mines;
        }

        // ── Affichage en-tête de jeu ─────────────────────────────────────────

        /// <summary>
        /// Affiche le mode de jeu et le nombre de mines
        /// </summary>
        static void DisplayGameHeader()
        {
            Console.SetCursorPosition(0, 4);
            Console.Write("A vous de jouer !! Mode : ");

            if (difficulty == 1)
            {
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write("Facile");
            }
            else if (difficulty == 2)
            {
                Console.BackgroundColor = ConsoleColor.DarkYellow;
                Console.ForegroundColor = ConsoleColor.Black;
                Console.Write("Moyen");
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("Difficile");
            }

            Console.ResetColor();
            Console.WriteLine();

            // Affichage du nombre de mines
            Console.Write(nbMines + " mines se cachent dans le jeu !");
            Console.WriteLine();
        }

        // ── Construction du plateau ──────────────────────────────────────────

        /// <summary>
        /// Dessine le plateau de jeu complet avec bordures ASCII
        /// </summary>
        static void DrawBoard()
        {
            int totalRows = nbRows * STEP_Y;
            int totalCols = nbCols * STEP_X;

            for (int cr = 0; cr <= totalRows; cr++)
            {
                Console.SetCursorPosition(MARGIN_LEFT, MARGIN_TOP + cr);
                Console.ForegroundColor = ConsoleColor.DarkCyan;

                bool isBorderRow = (cr % STEP_Y == 0);
                int gridRow = cr / STEP_Y;

                for (int cc = 0; cc <= totalCols; cc++)
                {
                    bool isColBorder = (cc % STEP_X == 0);
                    int gridCol = cc / STEP_X;

                    if (isBorderRow == true && isColBorder == true)
                    {
                        Console.Write(GetCornerChar(gridRow, gridCol));
                    }
                    else if (isBorderRow == true)
                    {
                        Console.Write(C_HORIZ);
                    }
                    else if (isColBorder == true)
                    {
                        Console.Write(C_VERT);
                    }
                    else
                    {
                        Console.Write(' ');
                    }
                }
            }

            Console.ResetColor();
        }

        /// <summary>
        /// Retourne le caractère de coin ou de jonction ASCII selon la position
        /// </summary>
        /// <param name="gridRow">Ligne dans la grille (0 à nbRows)</param>
        /// <param name="gridCol">Colonne dans la grille (0 à nbCols)</param>
        static char GetCornerChar(int gridRow, int gridCol)
        {
            bool isTop = (gridRow == 0);
            bool isBottom = (gridRow == nbRows);
            bool isLeft = (gridCol == 0);
            bool isRight = (gridCol == nbCols);

            if (isTop == true && isLeft == true)
            {
                return C_TOP_LEFT;
            }
            else if (isTop == true && isRight == true)
            {
                return C_TOP_RIGHT;
            }
            else if (isBottom == true && isLeft == true)
            {
                return C_BOT_LEFT;
            }
            else if (isBottom == true && isRight == true)
            {
                return C_BOT_RIGHT;
            }
            else if (isTop == true)
            {
                return C_TEE_TOP;
            }
            else if (isBottom == true)
            {
                return C_TEE_BOT;
            }
            else if (isLeft == true)
            {
                return C_TEE_LEFT;
            }
            else if (isRight == true)
            {
                return C_TEE_RIGHT;
            }
            else
            {
                return C_CROSS;
            }
        }
        // ── Exploration d'une case ───────────────────────────────────────────

        /// <summary>
        /// Explore la case courante.
        /// Retourne true si une mine a explosé, false sinon.
        /// </summary>
        /// <returns>true = mine explosée, game over</returns>
        static bool RevealCell()
        {
            if (revealed[posL, posC] == true)
            {
                return false;
            }

            revealed[posL, posC] = true;

            int left = MARGIN_LEFT + STEP_X / 2 + posC * STEP_X;
            int top = MARGIN_TOP + STEP_Y / 2 + posL * STEP_Y;

            if (hasMine[posL, posC] == true)
            {
                // Mine explosée
                Console.SetCursorPosition(left, top);
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write('*');
                Console.ResetColor();

                minesLeft = minesLeft - 1;
                RevealAllMines();
                return true;
            }
            else
            {
                // Case sûre : affiche le caractère ombragé en gris
                Console.SetCursorPosition(left, top);
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.Write('▒');
                Console.ResetColor();
                return false;
            }
        }

        /// <summary>
        /// Révèle toutes les mines restantes sur le plateau en fin de partie
        /// </summary>
        static void RevealAllMines()
        {
            for (int r = 0; r < nbRows; r++)
            {
                for (int c = 0; c < nbCols; c++)
                {
                    if (hasMine[r, c] == true && revealed[r, c] == false)
                    {
                        revealed[r, c] = true;

                        int left = MARGIN_LEFT + STEP_X / 2 + c * STEP_X;
                        int top = MARGIN_TOP + STEP_Y / 2 + r * STEP_Y;

                        Console.SetCursorPosition(left, top);
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write('*');
                        Console.ResetColor();
                    }
                }
            }
        }

        // ── Condition de victoire ────────────────────────────────────────────

        /// <summary>
        /// Vérifie si le joueur a gagné :
        /// - toutes les cases sans mine ont été explorées, OU
        /// - toutes les mines sont flagguées
        /// </summary>
        /// <returns>true si la partie est gagnée</returns>
        static bool CheckWin()
        {
            bool allSafeRevealed = true;
            bool allMinesFlagged = true;

            for (int r = 0; r < nbRows; r++)
            {
                for (int c = 0; c < nbCols; c++)
                {
                    if (hasMine[r, c] == false && revealed[r, c] == false)
                    {
                        allSafeRevealed = false;
                    }

                    if (hasMine[r, c] == true && flags[r, c] == false)
                    {
                        allMinesFlagged = false;
                    }
                }
            }

            if (allSafeRevealed == true || allMinesFlagged == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // ── Fin de partie ────────────────────────────────────────────────────

        /// <summary>
        /// Affiche le message de fin de partie selon le résultat
        /// </summary>
        /// <param name="won">true si le joueur a gagné</param>
        static void DisplayEndMessage(bool won)
        {
            int bottomRow = MARGIN_TOP + nbRows * STEP_Y + 3;

            Console.SetCursorPosition(0, bottomRow);
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine();
            Console.WriteLine("C'est la fin !");
            Console.WriteLine();

            if (won == true)
            {
                Console.BackgroundColor = ConsoleColor.DarkGreen;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("!! BRAVO !! Vous avez reussi a pas marcher sur toutes les mines !");
                Console.ResetColor();
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Il restait " + minesLeft + " sur " + nbMines + " mines");
            }
            else
            {
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("BOOM ! Vous avez saute sur une mine !");
                Console.ResetColor();
            }

            Console.ResetColor();
            Console.WriteLine();
        }

        /// <summary>
        /// Demande si le joueur veut rejouer
        /// </summary>
        /// <returns>true si le joueur veut rejouer</returns>
        static bool AskPlayAgain()
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Voulez-vous recommencer ?");
            Console.Write("(o) pour oui, autre touche pour quitter ! : ");
            Console.ResetColor();

            ConsoleKeyInfo key = Console.ReadKey(true);
            Console.WriteLine();

            if (key.KeyChar == 'o' || key.KeyChar == 'O')
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // ── Placement des mines ──────────────────────────────────────────────

        /// <summary>
        /// Place les mines aléatoirement dans le tableau hasMine.
        /// Utilise Random pour choisir des positions aléatoires.
        /// Une case ne peut contenir qu'une seule mine.
        /// </summary>
        static void PlaceMines()
        {
            Random rnd = new Random();
            int placed = 0;

            while (placed < nbMines)
            {
                int r = rnd.Next(nbRows);
                int c = rnd.Next(nbCols);

                if (hasMine[r, c] == false)
                {
                    hasMine[r, c] = true;
                    placed = placed + 1;
                }
            }
        }

        // ── Déplacement du curseur ───────────────────────────────────────────

        /// <summary>
        /// Déplace le curseur selon la touche directionnelle pressée.
        /// Le curseur revient de l'autre côté si on dépasse un bord.
        /// </summary>
        /// <param name="key">Touche pressée par l'utilisateur</param>
        static void MoveCursor(ConsoleKey key)
        {
            if (key == ConsoleKey.UpArrow)
            {
                if (posL - 1 >= 0)
                {
                    posL = posL - 1;
                }
                else
                {
                    posL = nbRows - 1;
                }
            }
            else if (key == ConsoleKey.DownArrow)
            {
                if (posL + 1 < nbRows)
                {
                    posL = posL + 1;
                }
                else
                {
                    posL = 0;
                }
            }
            else if (key == ConsoleKey.LeftArrow)
            {
                if (posC - 1 >= 0)
                {
                    posC = posC - 1;
                }
                else
                {
                    posC = nbCols - 1;
                }
            }
            else if (key == ConsoleKey.RightArrow)
            {
                if (posC + 1 < nbCols)
                {
                    posC = posC + 1;
                }
                else
                {
                    posC = 0;
                }
            }
        }

        /// <summary>
        /// Place le curseur console au milieu de la cellule courante.
        /// Formule : MARGIN_LEFT + STEP_X/2 + posC * STEP_X
        ///           MARGIN_TOP  + STEP_Y/2 + posL * STEP_Y
        /// </summary>
        static void PlaceCursor()
        {
            int left = MARGIN_LEFT + STEP_X / 2 + posC * STEP_X;
            int top = MARGIN_TOP + STEP_Y / 2 + posL * STEP_Y;
            Console.SetCursorPosition(left, top);
        }

        // ── Flag ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Pose un flag sur la cellule courante (touche Espace).
        /// Affiche le caractère ombragé en vert si la case est libre.
        /// </summary>
        static void PlaceFlag()
        {
            if (flags[posL, posC] == false)
            {
                flags[posL, posC] = true;

                int left = MARGIN_LEFT + STEP_X / 2 + posC * STEP_X;
                int top = MARGIN_TOP + STEP_Y / 2 + posL * STEP_Y;

                Console.SetCursorPosition(left, top);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.BackgroundColor = ConsoleColor.DarkGray;
                Console.Write('▒');
                Console.ResetColor();
            }
        }

        /// <summary>
        /// Enlève le flag sur la cellule courante (touche Enter).
        /// Efface le caractère ombragé si un flag est posé.
        /// </summary>
        static void RemoveFlag()
        {
            if (flags[posL, posC] == true)
            {
                flags[posL, posC] = false;

                int left = MARGIN_LEFT + STEP_X / 2 + posC * STEP_X;
                int top = MARGIN_TOP + STEP_Y / 2 + posL * STEP_Y;

                Console.SetCursorPosition(left, top);
                Console.Write(' ');
            }
        }
        // ── Consignes ────────────────────────────────────────────────────────

        /// <summary>
        /// Affiche les consignes à droite du plateau de jeu
        /// </summary>
        static void DisplayInstructions()
        {
            // Position à droite du plateau
            int instrLeft = MARGIN_LEFT + nbCols * STEP_X + 4;
            int top = MARGIN_TOP;

            Console.SetCursorPosition(instrLeft, top);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("Consignes");

            Console.SetCursorPosition(instrLeft, top + 1);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("----------");

            Console.SetCursorPosition(instrLeft, top + 2);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("- Pour se deplacer : touches flechees");

            Console.SetCursorPosition(instrLeft, top + 3);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("- Pour explorer une case : Entree");

            Console.SetCursorPosition(instrLeft, top + 4);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("- Pour poser un flag : Espace");

            Console.SetCursorPosition(instrLeft, top + 5);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("- Entree sur un flag : enleve le flag");

            Console.SetCursorPosition(instrLeft, top + 6);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("- Pour quitter : Echap");

            Console.SetCursorPosition(instrLeft, top + 8);
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("La partie est gagnee :");

            Console.SetCursorPosition(instrLeft, top + 9);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("- une fois que toutes les cases");

            Console.SetCursorPosition(instrLeft, top + 10);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("  ont ete explorees");

            Console.SetCursorPosition(instrLeft, top + 11);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("- que toutes les mines n'ont");

            Console.SetCursorPosition(instrLeft, top + 12);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("  pas ete explosees");

            Console.ResetColor();
        }

        // ── Compteur de mines ────────────────────────────────────────────────

        /// <summary>
        /// Affiche le compteur de mines restantes sous le plateau
        /// </summary>
        static void DisplayMineCounter()
        {
            int savedLeft = Console.CursorLeft;
            int savedTop = Console.CursorTop;

            int bottomRow = MARGIN_TOP + nbRows * STEP_Y + 2;

            Console.SetCursorPosition(0, bottomRow);
            Console.Write("il reste encore ");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.Write(minesLeft);
            Console.ResetColor();

            Console.Write(" mine(s) cachee(s)   ");

            Console.SetCursorPosition(savedLeft, savedTop);
        }
    }
}