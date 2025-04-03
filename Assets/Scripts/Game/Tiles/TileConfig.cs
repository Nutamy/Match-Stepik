using UnityEngine;

namespace Game.Tiles
{
    public enum TileKind
    {
        Normal,
        Blabk,
        Jelly
    }
    [CreateAssetMenu(fileName = "TileConfig", menuName = "Config/TileConfig")]
    public class TileConfig : ScriptableObject
    {
        [SerializeField] private Sprite _sprite;
        [SerializeField] private TileKind _tileKind;
        [SerializeField] private bool _isInteractable;

        public Sprite Sprite => _sprite;
        public TileKind TileKind => _tileKind;
        public bool IsInteractable => _isInteractable;
    }
}