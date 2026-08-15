namespace MemoryGame.Models
{
    /// <summary>
    /// Stellt eine einzelne Speicherkarte dar.
    /// </summary>
    public sealed class MemoryCard
    {
        /// <summary>
        /// Ruft den eindeutigen Bezeichner der Karte ab oder legt diesen fest.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Ruft das Symbol (Emoji oder Text) der Kartenrückseite ab oder legt dieses fest.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// Ruft einen Wert ab, der angibt, ob die Karte aufgedeckt ist, oder legt diesen fest.
        /// </summary>
        public bool IsRevealed { get; set; }

        /// <summary>
        /// Ruft einen Wert ab, der angibt, ob die Karte bereits zugeordnet wurde, oder legt diesen fest.
        /// </summary>
        public bool IsMatched { get; set; }
    }
}
