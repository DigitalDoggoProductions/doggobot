using System;

using Discord;

namespace DoggoBot.Core.Common.Colors
{
    public class Colors
    {
        private Random rand = new Random();

        public Color EmbedBlue = new Color(128, 212, 255);
        public Color EmbedLilac = new Color(214, 196, 237);
        public Color EmbedPink = new Color(255, 172, 194);
        public Color EmbedOrange = new Color(255, 204, 153);
        public Color EmbedMint = new Color(178, 247, 211);
        public Color EmbedYellow = new Color(255, 251, 181);
        public Color EmbedPurple = new Color(163, 97, 249);

        public Color EmbedError = new Color(255, 77, 77);
        public Color EmbedSuccess = new Color(204, 255, 204);

        public Color RandomColor() { return new Color(rand.Next(0, 255), rand.Next(0, 255), rand.Next(0, 255)); }
    }
}
