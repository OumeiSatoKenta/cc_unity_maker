using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game004_WordCrystal
{
    /// <summary>
    /// 8個のクリスタルを管理し、タップ入力を一元処理する。
    /// ラウンド開始時にターゲット単語を含む文字セットを配置する。
    /// </summary>
    public class CrystalManager : MonoBehaviour
    {
        [SerializeField] private WordCrystalGameManager _gameManager;
        [SerializeField] private WordCrystalUI _ui;
        [SerializeField] private List<CrystalView> _crystals = new();

        private readonly List<char> _selectedLetters = new();

        // Common letter frequency distribution (ETAOIN SHRDLU ...)
        private static readonly char[] CommonLetters =
            "AAAAAABBCDDEEEEEEFFFGGHHHHIIIIIIJKLLLLMMNNNNNOOOOOOPPQRRRRRSSSSTTTTTUUUUVVWWXYYZ"
            .ToCharArray();

        public void GenerateRound()
        {
            string[] words = WordCrystalGameManager.WordList;
            string target = words[Random.Range(0, words.Length)];
            // Clamp to crystal count
            if (target.Length > _crystals.Count) target = target.Substring(0, _crystals.Count);

            var letters = new List<char>(target.ToCharArray());
            while (letters.Count < _crystals.Count)
                letters.Add(CommonLetters[Random.Range(0, CommonLetters.Length)]);

            // Fisher-Yates shuffle
            for (int i = letters.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (letters[i], letters[j]) = (letters[j], letters[i]);
            }

            for (int i = 0; i < _crystals.Count; i++)
                _crystals[i].Reset(letters[i]);

            _selectedLetters.Clear();
            _ui.UpdateCurrentWord("");
        }

        private void Update()
        {
            if (_gameManager == null || !_gameManager.IsPlaying) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            Vector2 worldPos = Camera.main.ScreenToWorldPoint(
                Mouse.current.position.ReadValue());
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            if (hit == null) return;

            var crystal = hit.GetComponent<CrystalView>();
            if (crystal == null || crystal.IsRevealed) return;

            crystal.Reveal();
            _selectedLetters.Add(crystal.Letter);
            _ui.UpdateCurrentWord(new string(_selectedLetters.ToArray()));
        }

        public void SubmitWord()
        {
            if (_selectedLetters.Count == 0) return;
            string word = new string(_selectedLetters.ToArray());
            bool valid = _gameManager.SubmitWord(word);
            if (!valid)
            {
                ClearSelection();
                _ui.ShowInvalidFeedback();
            }
            else
            {
                _selectedLetters.Clear();
            }
        }

        public void ClearSelection()
        {
            foreach (var c in _crystals)
                if (c.IsRevealed) c.Hide();
            _selectedLetters.Clear();
            _ui.UpdateCurrentWord("");
        }
    }
}
