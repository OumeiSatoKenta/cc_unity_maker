using UnityEngine;

namespace Game007_NumberFlow
{
    /// <summary>
    /// グリッド上の1マス。数字・位置・訪問状態を保持し表示のみ担当。入力処理は持たない。
    /// </summary>
    public class NumberCell : MonoBehaviour
    {
        public int Number { get; private set; }
        public int Row { get; private set; }
        public int Col { get; private set; }
        public bool IsVisited { get; private set; }

        private SpriteRenderer _sr;
        private TextMesh _text;

        // Colors
        private static readonly Color ColorNormal  = new Color(0.28f, 0.30f, 0.36f);
        private static readonly Color ColorVisited = new Color(0.20f, 0.50f, 0.88f);
        private static readonly Color ColorCurrent = new Color(0.15f, 0.90f, 0.68f);
        private static readonly Color ColorStart   = new Color(0.22f, 0.82f, 0.32f);
        private static readonly Color ColorEnd     = new Color(0.92f, 0.76f, 0.12f);

        private void Awake()
        {
            _sr   = GetComponent<SpriteRenderer>();
            _text = GetComponentInChildren<TextMesh>();
        }

        public void Init(int number, int row, int col, int maxNumber)
        {
            Number = number;
            Row    = row;
            Col    = col;
            IsVisited = false;
            if (_text != null) _text.text = number.ToString();
            SetColor(number == 1 ? ColorStart : number == maxNumber ? ColorEnd : ColorNormal);
        }

        public void MarkVisited(bool isCurrent)
        {
            IsVisited = true;
            SetColor(isCurrent ? ColorCurrent : ColorVisited);
        }

        public void MarkCurrent()
        {
            SetColor(ColorCurrent);
        }

        public void Reset(int maxNumber)
        {
            IsVisited = false;
            SetColor(Number == 1 ? ColorStart : Number == maxNumber ? ColorEnd : ColorNormal);
        }

        private void SetColor(Color c)
        {
            if (_sr != null) _sr.color = c;
        }
    }
}
