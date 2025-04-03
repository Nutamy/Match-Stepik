using System.Collections.Generic;
using UnityEngine;

namespace Game.Tiles
{
    public class BlankTileSetup : MonoBehaviour
    {
        [SerializeField] private List<BlankTile> _blankTileLayout;
        public bool[,] Blanks { get; private set; }

        public void SetupBlanks(int width, int height)
        {
            Blanks = new bool[width, height];
            for (int i = 0; i < _blankTileLayout.Count; i++)
            {
                Blanks[_blankTileLayout[i].XPosition, _blankTileLayout[i].YPosition] = true;
            }
        }
    }
}