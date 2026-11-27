namespace Game.Score
{
    /// <summary>Номер шага каскада в текущем ходе: 1 — совпадение от обмена, 2+ — цепочка после падения.</summary>
    public class TurnCombo
    {
        // С какого шага каскада показываем похвалу
        public const int PraiseStep = 3;

        public int Step { get; private set; }

        public void Reset() => Step = 0;

        public int Next() => ++Step;

        public bool IsPraiseStep => Step == PraiseStep;
    }
}
