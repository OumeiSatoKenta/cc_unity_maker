using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game097_PixelEvolution
{
    public class PixelEvolutionGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("進化マネージャー")] private EvolutionManager _evolutionManager;
        [SerializeField, Tooltip("UI管理")] private PixelEvolutionUI _ui;

        private int _generation;
        private int[] _choices = new int[3]; // 分岐での選択 (0 or 1)
        private int _choiceIndex;
        private bool _isPlaying;

        public const int MaxGeneration = 10;
        private static readonly int[] BranchGenerations = { 3, 5, 7 };

        private static readonly string[][] BranchLabels =
        {
            new[] { "水辺", "陸地" },
            new[] { "熱帯", "寒冷" },
            new[] { "捕食者", "草食者" }
        };

        private static readonly string[] FinalNames =
        {
            "アクアドラゴン",   // 000
            "サンゴクラゲ",     // 001
            "アイスシャーク",   // 010
            "ユキウオ",         // 011
            "ファイアビースト", // 100
            "サボテンゴーレム", // 101
            "フロストウルフ",   // 110
            "モスディア"        // 111
        };

        private void Start()
        {
            if (_evolutionManager == null) { Debug.LogError("[PixelEvolutionGM] _evolutionManager が未アサイン"); return; }
            if (_ui == null) { Debug.LogError("[PixelEvolutionGM] _ui が未アサイン"); return; }
            StartNewGame();
        }

        private void StartNewGame()
        {
            _generation = 1;
            _choiceIndex = 0;
            _choices = new int[3];
            _isPlaying = true;
            _ui.HideBranchPanel();
            _ui.HideClearPanel();
            _ui.ShowEvolveButton(true);
            UpdateDisplay();
        }

        public void OnEvolveButtonPressed()
        {
            if (!_isPlaying) return;

            _generation++;

            // 分岐チェック
            for (int i = 0; i < BranchGenerations.Length; i++)
            {
                if (_generation == BranchGenerations[i] && _choiceIndex == i)
                {
                    _ui.ShowEvolveButton(false);
                    _ui.ShowBranchPanel(BranchLabels[i][0], BranchLabels[i][1]);
                    UpdateDisplay();
                    return;
                }
            }

            UpdateDisplay();

            if (_generation >= MaxGeneration)
            {
                _isPlaying = false;
                _ui.ShowEvolveButton(false);
                int finalIndex = _choices[0] * 4 + _choices[1] * 2 + _choices[2];
                _ui.ShowClearPanel(FinalNames[finalIndex], _generation);
            }
        }

        public void OnBranchSelectedA() => OnBranchSelected(0);
        public void OnBranchSelectedB() => OnBranchSelected(1);

        public void OnBranchSelected(int choice)
        {
            if (!_isPlaying) return;
            if (_choiceIndex >= 3) return;
            _choices[_choiceIndex] = choice;
            _choiceIndex++;
            _ui.HideBranchPanel();
            _ui.ShowEvolveButton(true);
            UpdateDisplay();

            if (_generation >= MaxGeneration)
            {
                _isPlaying = false;
                _ui.ShowEvolveButton(false);
                int finalIndex = _choices[0] * 4 + _choices[1] * 2 + _choices[2];
                _ui.ShowClearPanel(FinalNames[finalIndex], _generation);
            }
        }

        private void UpdateDisplay()
        {
            string name = GetCreatureName();
            _ui.UpdateGeneration(_generation, MaxGeneration);
            _ui.UpdateCreatureName(name);
            _evolutionManager.UpdateCreature(_generation, _choices, _choiceIndex);
        }

        private string GetCreatureName()
        {
            if (_generation >= MaxGeneration)
            {
                int idx = _choices[0] * 4 + _choices[1] * 2 + _choices[2];
                return FinalNames[idx];
            }
            if (_generation <= 2) return "原始ピクセル";
            if (_generation <= 4) return _choices[0] == 0 ? "水棲微生物" : "陸上微生物";
            if (_generation <= 6)
            {
                string env = _choices[0] == 0 ? "水棲" : "陸上";
                string temp = _choices[1] == 0 ? "熱帯" : "寒冷";
                return $"{temp}{env}生物";
            }
            string e2 = _choices[0] == 0 ? "水棲" : "陸上";
            string t2 = _choices[1] == 0 ? "熱帯" : "寒冷";
            string p2 = _choiceIndex >= 3 ? (_choices[2] == 0 ? "捕食" : "草食") : "";
            return $"{t2}{e2}{p2}生命体";
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
    }
}
