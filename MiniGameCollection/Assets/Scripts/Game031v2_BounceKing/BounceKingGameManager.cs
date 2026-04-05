using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

namespace Game031v2_BounceKing
{
    public enum BounceKingState
    {
        WaitingInstruction,
        WaitingLaunch,
        Playing,
        StageClear,
        Clear,
        GameOver
    }

    public class BounceKingGameManager : MonoBehaviour
    {
        [SerializeField] StageManager _stageManager;
        [SerializeField] InstructionPanel _instructionPanel;
        [SerializeField] PaddleController _paddle;
        [SerializeField] BlockManager _blockManager;
        [SerializeField] BounceKingUI _ui;

        [SerializeField] Sprite _ballSprite;

        public BounceKingState State { get; private set; } = BounceKingState.WaitingInstruction;

        int _score;
        int _life = 3;
        int _comboCount;
        float _comboMultiplier = 1f;
        int _currentStage;
        bool _isActive;

        List<BallController> _balls = new List<BallController>();
        float _currentSpeed = 5.0f;

        const int MaxLife = 3;
        Coroutine _autoAdvanceCo;

        void Start()
        {
            _instructionPanel.Show(
                "031v2",
                "BounceKing",
                "パドルでボールを打ち返してブロックを壊そう",
                "パドルをドラッグで左右に動かす。タップでボール発射",
                "全てのブロックを壊してステージクリア！"
            );
            _instructionPanel.OnDismissed -= StartGame;
            _instructionPanel.OnDismissed += StartGame;
        }

        void StartGame()
        {
            _score = 0;
            _life = MaxLife;
            _comboCount = 0;
            _comboMultiplier = 1f;
            _isActive = true;
            State = BounceKingState.WaitingLaunch;

            _blockManager.LoadSprites();
            if (_ballSprite == null)
                _ballSprite = Resources.Load<Sprite>("Sprites/Game031v2_BounceKing/Ball");
            _ui.Initialize(this);
            _ui.UpdateLife(_life);
            _ui.UpdateScore(_score);

            _stageManager.SetConfigs(new StageManager.StageConfig[]
            {
                new StageManager.StageConfig { speedMultiplier = 1.0f, countMultiplier = 3, complexityFactor = 0.0f, stageName = "Stage 1" },
                new StageManager.StageConfig { speedMultiplier = 1.2f, countMultiplier = 4, complexityFactor = 0.25f, stageName = "Stage 2" },
                new StageManager.StageConfig { speedMultiplier = 1.3f, countMultiplier = 5, complexityFactor = 0.4f, stageName = "Stage 3" },
                new StageManager.StageConfig { speedMultiplier = 1.5f, countMultiplier = 5, complexityFactor = 0.5f, stageName = "Stage 4" },
                new StageManager.StageConfig { speedMultiplier = 1.7f, countMultiplier = 6, complexityFactor = 0.6f, stageName = "Stage 5" },
            });

            _stageManager.OnStageChanged -= OnStageChanged;
            _stageManager.OnAllStagesCleared -= OnAllStagesCleared;
            _stageManager.OnStageChanged += OnStageChanged;
            _stageManager.OnAllStagesCleared += OnAllStagesCleared;
            _stageManager.StartFromBeginning();
        }

        void OnStageChanged(int stageIndex)
        {
            if (State == BounceKingState.Clear) return;
            _currentStage = stageIndex + 1;
            _comboCount = 0;
            _comboMultiplier = 1f;
            State = BounceKingState.WaitingLaunch;

            ClearBalls();
            var config = _stageManager.GetCurrentStageConfig();
            _currentSpeed = 5.0f * config.speedMultiplier;
            _blockManager.SetupStage(config, stageIndex);

            _paddle.Initialize();
            SpawnBall();

            _ui.UpdateStage(_currentStage, _stageManager.TotalStages);
            _ui.UpdateScore(_score);
            _ui.UpdateLife(_life);
            _ui.UpdateBlocks(_blockManager.RemainingBlocks);
            _ui.HideStageClear();
        }

        void OnAllStagesCleared()
        {
            State = BounceKingState.Clear;
            _isActive = false;
            ClearBalls();
            _ui.ShowFinalClear(_score);
        }

        void SpawnBall()
        {
            var go = new GameObject("Ball");
            go.layer = LayerMask.NameToLayer("Default");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;
            if (_ballSprite != null)
            {
                sr.sprite = _ballSprite;
                float targetSize = 0.36f;
                float s = targetSize / (_ballSprite.rect.width / _ballSprite.pixelsPerUnit);
                go.transform.localScale = Vector3.one * s;
            }

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.5f; // will be scaled by transform

            var ball = go.AddComponent<BallController>();
            ball.Initialize(_currentSpeed, _paddle, this);
            _balls.Add(ball);
        }

        public void RequestLaunch(BallController ball)
        {
            if (State != BounceKingState.WaitingLaunch) return;
            State = BounceKingState.Playing;
            Vector2 launchDir = new Vector2(Random.Range(-0.4f, 0.4f), 1f).normalized;
            ball.Launch(launchDir);
        }

        public void OnBlockHit(Block block)
        {
            _comboCount++;
            UpdateComboMultiplier();
            int pts = Mathf.RoundToInt(block.GetScore() * _comboMultiplier);
            _score += pts;
            _ui.UpdateScore(_score);
            _ui.UpdateBlocks(_blockManager.RemainingBlocks);
            _ui.ShowCombo(_comboCount, _comboMultiplier);
        }

        public void OnAllBlocksDestroyed()
        {
            if (State != BounceKingState.Playing && State != BounceKingState.WaitingLaunch) return;

            // Award remaining score for this stage
            int bonus = _life * 200;
            _score += bonus;

            State = BounceKingState.StageClear;
            _ui.UpdateScore(_score);
            _ui.ShowStageClear(_currentStage, bonus);

            if (_autoAdvanceCo != null) StopCoroutine(_autoAdvanceCo);
            _autoAdvanceCo = StartCoroutine(AutoAdvance());
        }

        IEnumerator AutoAdvance()
        {
            yield return new WaitForSeconds(2.5f);
            if (State == BounceKingState.StageClear)
                _stageManager.CompleteCurrentStage();
        }

        public void OnNextStagePressed()
        {
            if (State != BounceKingState.StageClear) return;
            if (_autoAdvanceCo != null) StopCoroutine(_autoAdvanceCo);
            _stageManager.CompleteCurrentStage();
        }

        public void OnBallLost(BallController ball)
        {
            if (State == BounceKingState.StageClear ||
                State == BounceKingState.Clear ||
                State == BounceKingState.GameOver) return;

            _balls.Remove(ball);
            _comboCount = 0;
            _comboMultiplier = 1f;
            _ui.ShowCombo(0, 1f);

            if (_balls.Count > 0) return; // Other balls still active

            // All balls lost
            _life--;
            _ui.UpdateLife(_life);
            StartCoroutine(CameraShake(0.3f, 0.4f));

            if (_life <= 0)
            {
                TriggerGameOver();
            }
            else
            {
                State = BounceKingState.WaitingLaunch;
                SpawnBall();
            }
        }

        void TriggerGameOver()
        {
            if (State == BounceKingState.GameOver) return;
            if (_autoAdvanceCo != null) { StopCoroutine(_autoAdvanceCo); _autoAdvanceCo = null; }
            State = BounceKingState.GameOver;
            _isActive = false;
            ClearBalls();
            _ui.ShowGameOver(_score);
        }

        public void OnItemCollected(ItemType type)
        {
            switch (type)
            {
                case ItemType.PaddleExpand:
                    _paddle.ApplyExpand(15f);
                    break;
                case ItemType.PaddleShrink:
                    _paddle.ApplyShrink(10f);
                    break;
                case ItemType.MultiBall:
                    SpawnExtraBalls();
                    break;
            }
        }

        void SpawnExtraBalls()
        {
            if (_balls.Count == 0) return;
            var origin = _balls[0];
            int toSpawn = Mathf.Min(2, 3 - _balls.Count);
            for (int i = 0; i < toSpawn; i++)
            {
                SpawnBall();
                var newBall = _balls[_balls.Count - 1];
                newBall.transform.position = origin.transform.position;
                float angle = (i + 1) * 30f * Mathf.Deg2Rad;
                var dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
                newBall.Launch(dir);
                State = BounceKingState.Playing;
            }
        }

        void UpdateComboMultiplier()
        {
            if (_comboCount >= 20) _comboMultiplier = 3.0f;
            else if (_comboCount >= 10) _comboMultiplier = 2.0f;
            else if (_comboCount >= 5) _comboMultiplier = 1.5f;
            else _comboMultiplier = 1.0f;
        }

        void ClearBalls()
        {
            foreach (var b in _balls)
                if (b != null) Destroy(b.gameObject);
            _balls.Clear();
        }

        IEnumerator CameraShake(float intensity, float duration)
        {
            var cam = Camera.main;
            if (cam == null) yield break;
            Vector3 origPos = cam.transform.position;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float shake = intensity * (1f - elapsed / duration);
                cam.transform.position = origPos + (Vector3)Random.insideUnitCircle * shake;
                yield return null;
            }
            cam.transform.position = origPos;
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void ReturnToMenu()
        {
            SceneManager.LoadScene("TopMenu");
        }
    }
}
