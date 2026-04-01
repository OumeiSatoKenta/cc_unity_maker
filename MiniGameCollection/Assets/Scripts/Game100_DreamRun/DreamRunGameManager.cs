using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game100_DreamRun
{
    public class DreamRunGameManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ランマネージャー")] private RunManager _runManager;
        [SerializeField, Tooltip("UI管理")] private DreamRunUI _ui;

        public const int MaxLife = 3;
        public const int TotalFragments = 5;

        private int _life;
        private int _fragments;
        private float _distance;
        private bool _isPlaying;
        private Camera _mainCamera;

        private static readonly string[] StoryTexts =
        {
            "どこまでも続く道を走っている…",
            "空が紫に染まり、星が歌い始める",
            "足元の地面が透明になっていく",
            "遠くに光る扉が見える",
            "扉に手が届く…目が覚める前に"
        };

        private void Start()
        {
            if (_runManager == null) { Debug.LogError("[DreamRunGM] _runManager が未アサイン"); return; }
            if (_ui == null) { Debug.LogError("[DreamRunGM] _ui が未アサイン"); return; }
            _mainCamera = Camera.main;
            StartNewGame();
        }

        private void StartNewGame()
        {
            _life = MaxLife;
            _fragments = 0;
            _distance = 0f;
            _isPlaying = true;
            _ui.HideClearPanel();
            _ui.HideGameOverPanel();
            _ui.HideStoryText();
            UpdateDisplay();
            _runManager.StartRun();
        }

        private void Update()
        {
            if (!_isPlaying) return;
            _distance += Time.deltaTime * 5f;
            _ui.UpdateDistance(_distance);

            // 背景色を距離に応じて変化
            if (_mainCamera != null)
            {
                float t = Mathf.PingPong(_distance * 0.02f, 1f);
                _mainCamera.backgroundColor = Color.Lerp(
                    new Color(0.3f, 0.15f, 0.5f),
                    new Color(0.1f, 0.2f, 0.5f), t);
            }
        }

        public void OnHitObstacle()
        {
            if (!_isPlaying) return;
            _life--;
            UpdateDisplay();
            if (_life <= 0)
            {
                _isPlaying = false;
                _runManager.StopRun();
                _ui.ShowGameOverPanel(_distance, _fragments);
            }
        }

        public void OnCollectFragment()
        {
            if (!_isPlaying) return;
            if (_fragments >= TotalFragments) return;

            string text = StoryTexts[_fragments];
            _fragments++;
            UpdateDisplay();
            _ui.ShowStoryText(text);

            if (_fragments >= TotalFragments)
            {
                _isPlaying = false;
                _runManager.StopRun();
                StartCoroutine(DelayClear());
            }
        }

        private System.Collections.IEnumerator DelayClear()
        {
            yield return new WaitForSeconds(3f);
            if (_ui != null) _ui.ShowClearPanel(_distance, _fragments);
        }

        private void UpdateDisplay()
        {
            _ui.UpdateDistance(_distance);
            _ui.UpdateFragments(_fragments, TotalFragments);
            _ui.UpdateLife(_life, MaxLife);
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public bool IsPlaying => _isPlaying;
        public int Fragments => _fragments;
    }
}
