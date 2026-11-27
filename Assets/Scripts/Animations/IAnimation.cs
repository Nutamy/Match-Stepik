using Cysharp.Threading.Tasks;
using DG.Tweening;
using Game.Tiles;
using UnityEngine;

namespace Animations
{
    public interface IAnimation
    {
        UniTask Reveal(GameObject target, float delay);
        UniTask HideTile(GameObject target);
        void DoPunchAnimate(GameObject target, Vector3 scale, float duration);
        // Добавляем этот метод:
        void PunchScoreText(GameObject target);
        void MoveUI(RectTransform target, Vector3 position, float duration, Ease ease);
        void AnimateTile(Tile tile, float value);
        void MoveTile(Tile tile, Vector3 position, Ease ease);
        // Фишка коротко вспухает и сжимается в ноль; по завершении выключается
        UniTask PopOut(GameObject target, float duration);
        // Падение фишки в клетку с лёгким отскоком при приземлении
        void DropTile(Tile tile, Vector3 localPosition, float duration, float delay);
        // Короткая тряска фишки на месте (ошибочный ход); в конце фишка стоит ровно в localPosition
        UniTask ShakeTile(Tile tile, Vector3 localPosition, Vector3 strength, float duration);
        // Подсказка: фишка покачивается (поворот), повторяясь с паузой, пока анимацию не убьют
        Tween SwayTile(Tile tile, float angle, float swayDuration, float pause);
        // Останавливает покачивание и ставит фишку ровно
        void StopSway(Tile tile, Tween sway);
    }
}