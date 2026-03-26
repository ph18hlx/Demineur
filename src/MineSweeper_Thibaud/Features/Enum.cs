namespace Demineur
{
    /// <summary>
    /// Niveau de difficulté du jeu
    /// </summary>
    enum Difficulty
    {
        Easy = 1,
        Medium = 2,
        Hard = 3
    }

    /// <summary>
    /// État visuel d'une cellule du plateau
    /// </summary>
    enum CellState
    {
        Hidden,   // Case non explorée
        Revealed, // Case explorée (ombragée gris)
        Flagged,  // Flag posé par le joueur (ombragé vert)
        Exploded  // Mine qui a explosé (*)
    }
}