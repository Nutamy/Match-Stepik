using UnityEngine;

namespace Game.Tiles
{
    public enum TileKind
    {
        Normal,
        Blank,
        Jelly
    }
    [CreateAssetMenu(fileName = "TileConfig", menuName = "Configs/TileConfig")]
    public class TileConfig : ScriptableObject
    {
        [SerializeField] private Sprite _sprite;
        [SerializeField] private TileKind _tileKind;
        [SerializeField] private bool _isInteractable;
        [Tooltip("Цвет частиц, когда фишка собрана")]
        [SerializeField] private Color _fxColor = Color.white;

        public Sprite Sprite => _sprite;
        public Color FxColor => _fxColor;
        public TileKind TileKind => _tileKind;
        public bool IsInteractable => _isInteractable;
    }
}