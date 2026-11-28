using UnityEngine;

namespace Game.Tiles
{
    /// <summary>
    /// Картинки особых фишек. Если в Resources/Specials лежит готовая графика, берётся она,
    /// иначе рисуется временная заглушка с тёмным контуром: стрелка для ракеты, круг для бомб.
    /// </summary>
    public static class SpecialSprites
    {
        // Assets/Resources/Specials/Rocket.png — ракета на поле вместо фишки, носом вправо
        private const string RocketPath = "Specials/Rocket";
        // Assets/Resources/Specials/RocketFlight.png — летящая ракета, носом вправо
        private const string RocketFlightPath = "Specials/RocketFlight";

        // Assets/Resources/Specials/Bomb.png — бомба на поле вместо фишки
        private const string BombPath = "Specials/Bomb";

        // Assets/Resources/Specials/ColorBomb.png — цветная бомба (5 в ряд)
        private const string ColorBombPath = "Specials/ColorBomb";

        private static Sprite _bomb;
        private static Sprite _colorBomb;
        private static Sprite _rocket;
        private static Sprite _rocketFlight;

        // Без своей картинки фишка-ракета берёт картинку летящей ракеты
        public static Sprite Rocket => _rocket != null ? _rocket : (_rocket = Resources.Load<Sprite>(RocketPath) ?? RocketFlight);

        // Без своей картинки бомба — круглая заглушка
        public static Sprite Bomb => _bomb != null ? _bomb : (_bomb = Resources.Load<Sprite>(BombPath) ?? CreateBall());

        // Без своей картинки цветная бомба — круглая заглушка, как у бомбы
        public static Sprite ColorBomb => _colorBomb != null
            ? _colorBomb
            : (_colorBomb = Resources.Load<Sprite>(ColorBombPath) ?? CreateBall());

        public static Sprite RocketFlight => _rocketFlight != null
            ? _rocketFlight
            : (_rocketFlight = Resources.Load<Sprite>(RocketFlightPath) ?? CreateArrow());

        private const int Size = 128;

        private static Sprite CreateArrow() => Draw("RocketFlightPlaceholder", (x, y) => Arrow(x, y));

        private static Sprite CreateBall() => Draw("BombPlaceholder", (x, y) => Mathf.Sqrt(x * x + y * y) - 0.8f);

        // Расстояние со знаком до фигуры примерно: меньше нуля — внутри
        private static Sprite Draw(string name, System.Func<float, float, float> shape)
        {
            var texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false)
            {
                name = name,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            var fill = new Color(1f, 0.98f, 0.9f, 1f);
            var outline = new Color(0.25f, 0.12f, 0.05f, 1f);
            const float outlineWidth = 0.07f;
            const float aa = 1.5f / Size;

            var pixels = new Color[Size * Size];
            for (int py = 0; py < Size; py++)
            for (int px = 0; px < Size; px++)
            {
                float x = (px + 0.5f) / Size * 2f - 1f;
                float y = (py + 0.5f) / Size * 2f - 1f;
                float d = shape(x, y);

                float inner = Mathf.Clamp01(0.5f - d / aa);
                float outer = Mathf.Clamp01(0.5f - (d - outlineWidth) / aa);
                var color = Color.Lerp(outline, fill, inner);
                color.a = outer;
                pixels[py * Size + px] = color;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), Size);
        }

        // ═► : стержень и наконечник справа
        private static float Arrow(float x, float y)
        {
            float bar = Box(x + 0.25f, y, 0.55f, 0.12f);
            float head = Triangle(x, y, 0.25f, 0.86f, 0.34f);
            return Mathf.Min(bar, head);
        }

        private static float Box(float x, float y, float halfWidth, float halfHeight) =>
            Mathf.Max(Mathf.Abs(x) - halfWidth, Mathf.Abs(y) - halfHeight);

        // Наконечник: основание на x = baseX высотой 2*halfHeight, остриё в x = tipX
        private static float Triangle(float x, float y, float baseX, float tipX, float halfHeight)
        {
            float t = (x - baseX) / (tipX - baseX);
            float edge = Mathf.Abs(y) - halfHeight * (1f - t);
            float back = baseX - x;
            float front = x - tipX;
            // Нормируем наклонную грань, чтобы контур был одинаковой толщины
            float slope = halfHeight / (tipX - baseX);
            edge /= Mathf.Sqrt(1f + slope * slope);
            return Mathf.Max(edge, Mathf.Max(back, front));
        }
    }
}
