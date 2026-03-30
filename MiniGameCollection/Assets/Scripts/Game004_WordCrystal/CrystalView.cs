using UnityEngine;

namespace Game004_WordCrystal
{
    /// <summary>
    /// クリスタルの状態とスプライト表示を管理する。入力処理は持たない。
    /// </summary>
    public class CrystalView : MonoBehaviour
    {
        [SerializeField] private Sprite _crystalSprite;
        [SerializeField] private Sprite _revealedSprite;

        public char Letter { get; private set; }
        public bool IsRevealed { get; private set; }

        private SpriteRenderer _sr;
        private TextMesh _textMesh;

        private void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _textMesh = GetComponentInChildren<TextMesh>();
        }

        public void Reset(char letter)
        {
            Letter = letter;
            IsRevealed = false;
            if (_sr != null && _crystalSprite != null) _sr.sprite = _crystalSprite;
            if (_textMesh != null) _textMesh.text = "";
        }

        public void Reveal()
        {
            IsRevealed = true;
            if (_sr != null && _revealedSprite != null) _sr.sprite = _revealedSprite;
            if (_textMesh != null) _textMesh.text = Letter.ToString();
        }

        public void Hide()
        {
            IsRevealed = false;
            if (_sr != null && _crystalSprite != null) _sr.sprite = _crystalSprite;
            if (_textMesh != null) _textMesh.text = "";
        }
    }
}
