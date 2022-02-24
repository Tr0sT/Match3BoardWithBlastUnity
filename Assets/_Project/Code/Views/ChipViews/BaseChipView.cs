#nullable enable
using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Views
{
    public abstract class BaseChipView : MonoBehaviour, IPointerClickHandler
    {
        public event Action? OnClick;
        private Sequence? moveSequence;

        public virtual void Init(int x, int y)
        {
            transform.localPosition = new Vector3(BaseCellView.Width * x, -BaseCellView.Height * y);
            gameObject.SetActive(true);
        }

        public virtual void MoveTo(int x, int y, float moveTime, Action onMoveComplete)
        {
            moveSequence = DOTween.Sequence().Append(
                transform.DOLocalMove(new Vector3(BaseCellView.Width * x, -BaseCellView.Height * y), moveTime).SetEase(Ease.Linear));
            moveSequence.OnComplete(onMoveComplete.Invoke);
        }

        public virtual void DeInit()
        {
            OnClick = null;
            moveSequence?.Kill();
            gameObject.SetActive(false);
        }

        protected virtual void OnDestroy()
        {
            DeInit();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            OnClick?.Invoke();
        }
    }
}