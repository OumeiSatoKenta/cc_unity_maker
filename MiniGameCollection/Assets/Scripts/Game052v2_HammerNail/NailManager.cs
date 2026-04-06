using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game052v2_HammerNail
{
    public enum NailType { Normal, Hard, Boss }

    public class NailData
    {
        public NailType Type;
        public int RequiredHits;
        public int HitCount;
        public bool IsDriven => HitCount >= RequiredHits;
        public float DriveProgress => RequiredHits > 0 ? (float)HitCount / RequiredHits : 0f;
    }

    public class NailManager : MonoBehaviour
    {
        [SerializeField] Sprite _normalNailSprite;
        [SerializeField] Sprite _hardNailSprite;
        [SerializeField] Sprite _bossNailSprite;

        [SerializeField] SpriteRenderer _hammerRenderer;

        public int CurrentNailIndex { get; private set; }
        public int TotalNails { get; private set; }
        public int RemainingNails => TotalNails - CurrentNailIndex;

        // Called after animation when all nails are driven
        public event Action OnAllNailsDriven;

        readonly List<NailData> _nails = new();
        readonly List<GameObject> _nailObjects = new();
        readonly List<Vector3> _nailInitialPositions = new();
        readonly List<float> _nailBaseScales = new();
        Coroutine _hitAnimCoroutine;
        Coroutine _hammerAnimCoroutine;

        public void SetupStage(int nailCount, float complexityFactor, int stageIndex)
        {
            ClearNails();
            _nails.Clear();

            for (int i = 0; i < nailCount; i++)
            {
                NailType type;
                int req;
                if (stageIndex >= 4)
                {
                    if (i == 0) { type = NailType.Boss; req = 5; }
                    else if (i % 3 == 2 && complexityFactor >= 0.3f) { type = NailType.Hard; req = 2; }
                    else { type = NailType.Normal; req = 1; }
                }
                else if (stageIndex >= 3)
                {
                    type = (i % 3 == 1) ? NailType.Hard : NailType.Normal;
                    req = type == NailType.Hard ? 2 : 1;
                }
                else if (stageIndex >= 2)
                {
                    type = (i == nailCount / 2) ? NailType.Hard : NailType.Normal;
                    req = type == NailType.Hard ? 2 : 1;
                }
                else
                {
                    type = NailType.Normal;
                    req = 1;
                }
                _nails.Add(new NailData { Type = type, RequiredHits = req, HitCount = 0 });
            }

            TotalNails = nailCount;
            CurrentNailIndex = 0;
            SpawnNailObjects();
        }

        void SpawnNailObjects()
        {
            var cam = Camera.main;
            if (cam == null) { Debug.LogError("[NailManager] Camera.main is null"); return; }

            float camSize = cam.orthographicSize;
            float camWidth = camSize * cam.aspect;
            float boardY = 0.3f;
            float availableWidth = camWidth * 1.6f;
            int count = _nails.Count;
            float spacing = availableWidth / (count + 1);
            float startX = -availableWidth * 0.5f + spacing;

            for (int i = 0; i < count; i++)
            {
                var data = _nails[i];
                Sprite sp = data.Type switch {
                    NailType.Hard => _hardNailSprite,
                    NailType.Boss => _bossNailSprite,
                    _ => _normalNailSprite
                };

                var go = new GameObject($"Nail_{i}");
                go.transform.position = new Vector3(startX + spacing * i, boardY, 0f);

                float scale = data.Type switch {
                    NailType.Boss => 0.55f,
                    NailType.Hard => 0.45f,
                    _ => 0.4f
                };
                go.transform.localScale = Vector3.one * scale;

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = sp;
                sr.sortingOrder = 5;

                _nailObjects.Add(go);
                _nailInitialPositions.Add(go.transform.position);
                _nailBaseScales.Add(scale);
            }

            HighlightCurrentNail();
        }

        void ClearNails()
        {
            foreach (var go in _nailObjects)
                if (go != null) Destroy(go);
            _nailObjects.Clear();
            _nailInitialPositions.Clear();
            _nailBaseScales.Clear();
        }

        void OnDestroy()
        {
            ClearNails();
        }

        public NailData GetCurrentNail()
        {
            if (CurrentNailIndex < _nails.Count) return _nails[CurrentNailIndex];
            return null;
        }

        // Returns true if nail becomes fully driven (CurrentNailIndex not yet incremented)
        public bool HitCurrentNail(HitResult result)
        {
            var nail = GetCurrentNail();
            if (nail == null) return false;

            if (result == HitResult.Miss) return false;

            nail.HitCount++;

            if (_hitAnimCoroutine != null) StopCoroutine(_hitAnimCoroutine);
            _hitAnimCoroutine = StartCoroutine(SinkNailAnimation(CurrentNailIndex, nail, result));

            return nail.IsDriven;
        }

        IEnumerator SinkNailAnimation(int idx, NailData nail, HitResult result)
        {
            if (idx >= _nailObjects.Count) yield break;
            var go = _nailObjects[idx];
            if (go == null) yield break;

            var sr = go.GetComponent<SpriteRenderer>();
            Vector3 initPos = _nailInitialPositions[idx];
            float baseScale = idx < _nailBaseScales.Count ? _nailBaseScales[idx] : 0.4f;
            float sinkAmount = nail.IsDriven ? (nail.RequiredHits * 0.15f) : (nail.DriveProgress * nail.RequiredHits * 0.15f);

            Vector3 origScale = Vector3.one * baseScale;
            float elapsed = 0f;
            float duration = 0.15f;

            Color hitColor = result == HitResult.Perfect
                ? new Color(1f, 0.95f, 0.3f)
                : new Color(0.4f, 1f, 0.4f);

            if (sr != null) sr.color = hitColor;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scaleMult = t < 0.5f ? 1f + t * 0.6f : 1f + (1f - t) * 0.6f;
                go.transform.localScale = origScale * scaleMult;
                float currentSink = sinkAmount * nail.DriveProgress;
                go.transform.position = initPos + Vector3.down * currentSink;
                yield return null;
            }

            go.transform.localScale = origScale;
            float finalSink = nail.DriveProgress * nail.RequiredHits * 0.15f;
            go.transform.position = initPos + Vector3.down * finalSink;

            if (sr != null) sr.color = Color.white;

            if (nail.IsDriven)
            {
                go.transform.position = initPos + Vector3.down * (nail.RequiredHits * 0.15f + 0.1f);
                if (sr != null) sr.color = new Color(0.6f, 0.6f, 0.6f, 0.8f);
                yield return new WaitForSeconds(0.2f);

                CurrentNailIndex++;
                HighlightCurrentNail();

                if (CurrentNailIndex >= TotalNails)
                    OnAllNailsDriven?.Invoke();
            }
        }

        void HighlightCurrentNail()
        {
            for (int i = 0; i < _nailObjects.Count; i++)
            {
                if (_nailObjects[i] == null) continue;
                var sr = _nailObjects[i].GetComponent<SpriteRenderer>();
                float baseScale = i < _nailBaseScales.Count ? _nailBaseScales[i] : 0.4f;
                if (i == CurrentNailIndex)
                {
                    if (sr != null) sr.color = Color.white;
                    _nailObjects[i].transform.localScale = Vector3.one * (baseScale * 1.1f);
                }
                else if (i > CurrentNailIndex)
                {
                    if (sr != null) sr.color = new Color(0.9f, 0.9f, 0.9f, 0.8f);
                    _nailObjects[i].transform.localScale = Vector3.one * baseScale;
                }
            }
        }

        // Miss: tilt current nail visually
        public void TiltCurrentNail()
        {
            if (CurrentNailIndex >= _nailObjects.Count) return;
            var go = _nailObjects[CurrentNailIndex];
            if (go == null) return;
            float tilt = UnityEngine.Random.Range(-15f, 15f);
            go.transform.rotation = Quaternion.Euler(0, 0, tilt);
        }

        public void PlayHammerAnim(Vector3 nailPos)
        {
            if (_hammerRenderer == null) return;
            if (_hammerAnimCoroutine != null) StopCoroutine(_hammerAnimCoroutine);
            _hammerAnimCoroutine = StartCoroutine(HammerSwingAnim(nailPos));
        }

        IEnumerator HammerSwingAnim(Vector3 nailPos)
        {
            if (_hammerRenderer == null) yield break;
            _hammerRenderer.gameObject.SetActive(true);
            Vector3 startPos = nailPos + new Vector3(0f, 1.5f, 0f);
            Vector3 endPos = nailPos + new Vector3(0f, 0.3f, 0f);
            float elapsed = 0f;
            float duration = 0.1f;
            _hammerRenderer.transform.position = startPos;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _hammerRenderer.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
                yield return null;
            }
            _hammerRenderer.transform.position = endPos;
            yield return new WaitForSeconds(0.05f);
            elapsed = 0f;
            while (elapsed < 0.1f)
            {
                elapsed += Time.deltaTime;
                _hammerRenderer.transform.position = Vector3.Lerp(endPos, startPos, elapsed / 0.1f);
                yield return null;
            }
            _hammerRenderer.gameObject.SetActive(false);
            _hammerAnimCoroutine = null;
        }
    }
}
