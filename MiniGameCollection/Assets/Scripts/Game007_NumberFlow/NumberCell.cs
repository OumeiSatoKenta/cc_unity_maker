using UnityEngine;
using TMPro;

namespace Game007_NumberFlow
{
    public class NumberCell : MonoBehaviour
    {
        public Vector2Int GridPosition { get; private set; }
        public int Number { get; private set; } // 0 = empty, >0 = fixed hint number
        public bool IsVisited { get; private set; }
        public int VisitOrder { get; private set; }

        private SpriteRenderer _spriteRenderer;
        private TextMeshPro _numberText;

        public void Initialize(Vector2Int gridPos, int number)
        {
            GridPosition = gridPos;
            Number = number;
            IsVisited = false;
            VisitOrder = -1;

            _spriteRenderer = GetComponent<SpriteRenderer>();
            _numberText = GetComponentInChildren<TextMeshPro>();

            if (_numberText != null)
            {
                if (number > 0)
                {
                    _numberText.text = number.ToString();
                    _numberText.gameObject.SetActive(true);
                }
                else
                {
                    _numberText.text = "";
                    _numberText.gameObject.SetActive(false);
                }
            }
        }

        public void MarkVisited(int order, Sprite visitedSprite, Sprite currentSprite)
        {
            IsVisited = true;
            VisitOrder = order;
            if (_numberText != null)
            {
                _numberText.text = order.ToString();
                _numberText.gameObject.SetActive(true);
            }
            if (_spriteRenderer != null && currentSprite != null)
                _spriteRenderer.sprite = currentSprite;
        }

        public void MarkPrevious(Sprite visitedSprite)
        {
            if (_spriteRenderer != null && visitedSprite != null)
                _spriteRenderer.sprite = visitedSprite;
        }

        public void Reset(Sprite normalSprite, Sprite startSprite)
        {
            IsVisited = false;
            VisitOrder = -1;
            if (_numberText != null)
            {
                if (Number > 0)
                {
                    _numberText.text = Number.ToString();
                    _numberText.gameObject.SetActive(true);
                }
                else
                {
                    _numberText.text = "";
                    _numberText.gameObject.SetActive(false);
                }
            }
            if (_spriteRenderer != null)
            {
                _spriteRenderer.sprite = Number == 1 ? startSprite : normalSprite;
            }
        }
    }
}
