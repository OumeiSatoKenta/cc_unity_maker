using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

namespace Game101_ChainReactor
{
    public class ReactorManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private ChainReactorGameManager _gameManager;
        [SerializeField, Tooltip("通常オーブスプライト")] private Sprite _orbSprite;
        [SerializeField, Tooltip("シールドオーブスプライト")] private Sprite _shieldOrbSprite;
        [SerializeField, Tooltip("ボーナスオーブスプライト")] private Sprite _bonusOrbSprite;
        [SerializeField, Tooltip("爆発エフェクトスプライト")] private Sprite _explosionSprite;

        private class OrbData
        {
            public GameObject obj;
            public SpriteRenderer renderer;
            public int type; // 0=通常, 1=移動, 2=シールド, 3=ボーナス
            public int shieldHP;
            public Vector2 moveDir;
            public float moveSpeed;
            public bool isExploding;
        }

        private readonly List<OrbData> _orbs = new List<OrbData>();
        private float _blastRadius;
        private int _totalOrbs;
        private bool _isActive;
        private bool _chainInProgress;
        private Camera _mainCamera;

        private const float PlayAreaMinX = -4f;
        private const float PlayAreaMaxX = 4f;
        private const float PlayAreaMinY = -3.5f;
        private const float PlayAreaMaxY = 3.5f;

        public int RemainingOrbs
        {
            get
            {
                int count = 0;
                foreach (var o in _orbs) if (o.obj != null && !o.isExploding) count++;
                return count;
            }
        }
        public int TotalOrbs => _totalOrbs;

        private void Awake() { _mainCamera = Camera.main; }

        public void SetupStage(int orbCount, float blastRadius, float moveRatio, float shieldRatio, float bonusRatio)
        {
            StopStage();
            _blastRadius = blastRadius;
            _totalOrbs = orbCount;
            _isActive = true;
            _chainInProgress = false;

            for (int i = 0; i < orbCount; i++)
            {
                float rand = Random.value;
                int type;
                if (rand < bonusRatio) type = 3;
                else if (rand < bonusRatio + shieldRatio) type = 2;
                else if (rand < bonusRatio + shieldRatio + moveRatio) type = 1;
                else type = 0;

                Vector2 pos = new Vector2(
                    Random.Range(PlayAreaMinX + 0.5f, PlayAreaMaxX - 0.5f),
                    Random.Range(PlayAreaMinY + 0.5f, PlayAreaMaxY - 0.5f));

                CreateOrb(pos, type);
            }
        }

        private void CreateOrb(Vector2 pos, int type)
        {
            Sprite sprite = type switch
            {
                2 => _shieldOrbSprite ?? _orbSprite,
                3 => _bonusOrbSprite ?? _orbSprite,
                _ => _orbSprite
            };

            var obj = new GameObject($"Orb_{type}_{_orbs.Count}");
            obj.transform.SetParent(transform);
            obj.transform.position = new Vector3(pos.x, pos.y, 0f);
            obj.transform.localScale = Vector3.one * 0.8f;
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 3;

            var data = new OrbData
            {
                obj = obj,
                renderer = sr,
                type = type,
                shieldHP = type == 2 ? 2 : 1,
                moveDir = type == 1 ? Random.insideUnitCircle.normalized : Vector2.zero,
                moveSpeed = type == 1 ? 0.5f : 0f,
                isExploding = false
            };
            _orbs.Add(data);
        }

        public void StopStage()
        {
            _isActive = false;
            StopAllCoroutines();
            foreach (var o in _orbs) if (o.obj != null) Destroy(o.obj);
            _orbs.Clear();
        }

        private void Update()
        {
            if (!_isActive || _gameManager == null || !_gameManager.IsPlaying) return;

            UpdateMovingOrbs();

            if (!_chainInProgress) HandleInput();
        }

        private void UpdateMovingOrbs()
        {
            foreach (var o in _orbs)
            {
                if (o.obj == null || o.type != 1 || o.isExploding) continue;
                Vector3 pos = o.obj.transform.position;
                pos += (Vector3)(o.moveDir * o.moveSpeed * Time.deltaTime);

                // 壁で反射
                if (pos.x < PlayAreaMinX || pos.x > PlayAreaMaxX) o.moveDir.x = -o.moveDir.x;
                if (pos.y < PlayAreaMinY || pos.y > PlayAreaMaxY) o.moveDir.y = -o.moveDir.y;
                pos.x = Mathf.Clamp(pos.x, PlayAreaMinX, PlayAreaMaxX);
                pos.y = Mathf.Clamp(pos.y, PlayAreaMinY, PlayAreaMaxY);

                o.obj.transform.position = pos;
            }
        }

        private void HandleInput()
        {
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                ProcessTap(Mouse.current.position.ReadValue());

            var ts = Touchscreen.current;
            if (ts != null)
            {
                foreach (var touch in ts.touches)
                {
                    if (touch.press.wasPressedThisFrame)
                    {
                        ProcessTap(touch.position.ReadValue());
                        break;
                    }
                }
            }
        }

        private void ProcessTap(Vector2 screenPos)
        {
            if (_mainCamera == null) return;
            Vector2 worldPos = _mainCamera.ScreenToWorldPoint(screenPos);

            // プレイエリア外のタップは無視（UIボタン等のため）
            if (worldPos.x < PlayAreaMinX - 0.5f || worldPos.x > PlayAreaMaxX + 0.5f ||
                worldPos.y < PlayAreaMinY - 1f || worldPos.y > PlayAreaMaxY + 1f) return;

            _gameManager.OnTapUsed();
            StartCoroutine(ChainExplosion(worldPos, 0));
        }

        private IEnumerator ChainExplosion(Vector2 center, int depth)
        {
            _chainInProgress = true;

            // 爆発エフェクト
            SpawnExplosionEffect(center);

            // 範囲内のオーブを検索
            var toExplode = new List<(OrbData orb, int newDepth)>();
            foreach (var o in _orbs)
            {
                if (o.obj == null || o.isExploding) continue;
                float dist = Vector2.Distance(center, o.obj.transform.position);
                if (dist <= _blastRadius)
                {
                    o.shieldHP--;
                    if (o.shieldHP <= 0)
                    {
                        o.isExploding = true;
                        toExplode.Add((o, depth + 1));
                        _gameManager.OnOrbExploded(depth + 1, o.type == 3);
                    }
                    else
                    {
                        // シールドヒットエフェクト
                        StartCoroutine(ShieldHitEffect(o));
                    }
                }
            }

            // 連鎖爆発を0.15秒間隔で実行
            foreach (var (orb, newDepth) in toExplode)
            {
                if (orb.obj == null) continue;
                Vector2 orbPos = orb.obj.transform.position;

                // 爆発演出: スケール拡大 + フェード
                StartCoroutine(OrbExplodeEffect(orb));

                yield return new WaitForSeconds(0.15f);

                // 連鎖: このオーブの位置からさらに爆発
                StartCoroutine(ChainExplosion(orbPos, newDepth));
            }

            // この深度の連鎖が完了するまで少し待つ
            if (toExplode.Count == 0)
            {
                // 連鎖終了 - 全体の連鎖が終わったか確認
                yield return new WaitForSeconds(0.3f);
                if (!HasPendingExplosions())
                {
                    _chainInProgress = false;
                    _gameManager.OnChainComplete();
                }
            }
        }

        private bool HasPendingExplosions()
        {
            foreach (var o in _orbs)
                if (o.obj != null && o.isExploding) return true;
            return false;
        }

        private IEnumerator OrbExplodeEffect(OrbData orb)
        {
            if (orb.obj == null) yield break;
            var t = orb.obj.transform;
            float elapsed = 0f;
            float duration = 0.3f;
            Vector3 origScale = t.localScale;

            while (elapsed < duration && t != null)
            {
                elapsed += Time.deltaTime;
                float p = elapsed / duration;
                t.localScale = origScale * (1f + p * 1.5f);
                if (orb.renderer != null)
                    orb.renderer.color = new Color(1f, 1f, 1f, 1f - p);
                yield return null;
            }

            if (orb.obj != null) Destroy(orb.obj);
        }

        private IEnumerator ShieldHitEffect(OrbData orb)
        {
            if (orb.renderer == null) yield break;
            Color orig = orb.renderer.color;
            orb.renderer.color = Color.white;
            if (orb.obj != null)
                orb.obj.transform.localScale = Vector3.one * 1.0f;
            yield return new WaitForSeconds(0.1f);
            if (orb.renderer != null) orb.renderer.color = orig;
            if (orb.obj != null)
                orb.obj.transform.localScale = Vector3.one * 0.8f;
        }

        private void SpawnExplosionEffect(Vector2 pos)
        {
            if (_explosionSprite == null) return;
            var obj = new GameObject("Explosion");
            obj.transform.position = new Vector3(pos.x, pos.y, 0f);
            obj.transform.localScale = Vector3.zero;
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _explosionSprite;
            sr.sortingOrder = 10;
            sr.color = new Color(1f, 0.8f, 0.3f, 0.8f);
            StartCoroutine(ExplosionEffectCoroutine(obj, sr));
        }

        private IEnumerator ExplosionEffectCoroutine(GameObject obj, SpriteRenderer sr)
        {
            float elapsed = 0f;
            float duration = 0.4f;
            float targetScale = _blastRadius * 0.6f;

            while (elapsed < duration && obj != null)
            {
                elapsed += Time.deltaTime;
                float p = elapsed / duration;
                float scale = targetScale * Mathf.Sin(p * Mathf.PI);
                obj.transform.localScale = Vector3.one * scale;
                if (sr != null) sr.color = new Color(1f, 0.8f, 0.3f, 0.8f * (1f - p));
                yield return null;
            }
            if (obj != null) Destroy(obj);
        }
    }
}
