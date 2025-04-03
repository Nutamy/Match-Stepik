using Levels;


namespace Game.Tiles
{
    public class BlankTileSetup
    {
        public bool[,] Blanks { get; private set; }

        public void SetupBlanks(LevelConfig levelConfig)
        {
            Blanks = new bool[levelConfig.Width, levelConfig.Height];
            var blankList = levelConfig.BlankTilesLayout;
            for (int i = 0; i < blankList.Count; i++)
            {
                Blanks[blankList[i].XPosition, blankList[i].YPosition] = true;
            }
        }
    }
}