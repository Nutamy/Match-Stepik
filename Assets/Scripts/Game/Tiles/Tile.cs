using Game.MatchTiles;
using UnityEngine;

namespace Game.Tiles
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class Tile : MonoBehaviour
    {
        public TileConfig TileConfig { get; private set; }
        public bool IsInteractable { get; private set; }
        public bool IsMatched { get; private set; }
        public TileSpecial Special { get; private set; }

        private SpriteRenderer _specialRenderer;

        public void SetTileConfig(TileConfig tileConfig)
        {
            TileConfig = tileConfig;
            IsInteractable = tileConfig.IsInteractable;
            IsMatched = false;
            GetComponent<SpriteRenderer>().sprite = tileConfig.Sprite;
        }

        public bool SetMatched(bool value) => IsMatched = value;

        /// <summary>Делает фишку особой (ракетой, бомбой или цветной бомбой) или обычной; особая рисуется вместо фишки.</summary>
        public void SetSpecial(TileSpecial special)
        {
            Special = special;
            var main = GetComponent<SpriteRenderer>();
            if (special == TileSpecial.None)
            {
                main.enabled = true;
                if (_specialRenderer != null) _specialRenderer.gameObject.SetActive(false);
                return;
            }

            // Ракета рисуется вместо фишки: одна ракета носом туда, куда полетит; бомба — как есть
            var overlay = GetSpecialRenderer();
            overlay.sprite = SpriteFor(special);
            // Картинка ракеты смотрит вправо; вертикальная ракета — та же картинка носом вверх
            overlay.transform.localRotation = Quaternion.Euler(0f, 0f, special == TileSpecial.RocketVertical ? 90f : 0f);
            FitOverlay(overlay);
            overlay.gameObject.SetActive(true);
            main.enabled = false;
        }

        private static Sprite SpriteFor(TileSpecial special)
        {
            switch (special)
            {
                case TileSpecial.Bomb: return SpecialSprites.Bomb;
                case TileSpecial.ColorBomb: return SpecialSprites.ColorBomb;
                default: return SpecialSprites.Rocket;
            }
        }

        private SpriteRenderer GetSpecialRenderer()
        {
            if (_specialRenderer != null) return _specialRenderer;

            var main = GetComponent<SpriteRenderer>();
            var go = new GameObject("Special");
            go.transform.SetParent(transform, false);
            _specialRenderer = go.AddComponent<SpriteRenderer>();
            _specialRenderer.sortingLayerID = main.sortingLayerID;
            _specialRenderer.sortingOrder = main.sortingOrder + 1;
            _specialRenderer.maskInteraction = main.maskInteraction;
            return _specialRenderer;
        }

        // Особая фишка по большей стороне занимает OverlayFill от размера фишки независимо от разрешения картинки
        private const float OverlayFill = 0.95f;

        private void FitOverlay(SpriteRenderer overlay)
        {
            var mainSprite = GetComponent<SpriteRenderer>().sprite;
            float tileSize = mainSprite != null ? Mathf.Max(mainSprite.bounds.size.x, mainSprite.bounds.size.y) : 1f;
            float iconSize = Mathf.Max(overlay.sprite.bounds.size.x, overlay.sprite.bounds.size.y);
            overlay.transform.localScale = Vector3.one * (tileSize * OverlayFill / Mathf.Max(0.0001f, iconSize));
        }
    }
}
