using System;
using System.Collections;
using UnityEngine;

namespace Game005v2_PipeConnect
{
    public enum PipeType { Empty, Straight, Elbow, TJunction, Source, Exit, Locked, ValveOpen, ValveClosed }

    /// <summary>
    /// 個々のパイプセル。接続方向計算・回転アニメーション・バルブ開閉を担当。
    /// 方向インデックス: 0=上, 1=右, 2=下, 3=左
    /// </summary>
    public class PipeCell : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _sr;

        public PipeType Type { get; private set; }
        public int RotationIndex { get; private set; } // 0-3 (90度単位)
        public bool IsLocked { get; private set; }
        public bool IsValveOpen { get; private set; }

        private bool _isAnimating;

        // スプライト参照（PipeManagerが設定）
        public Sprite SpriteValveOpen { get; set; }
        public Sprite SpriteValveClosed { get; set; }

        public event Action OnTapped;

        public void Initialize(PipeType type, int rotationIndex, bool locked)
        {
            Type = type;
            RotationIndex = rotationIndex;
            IsLocked = locked;
            IsValveOpen = (type == PipeType.ValveOpen);
            ApplyRotationImmediate();
        }

        private void ApplyRotationImmediate()
        {
            transform.localEulerAngles = new Vector3(0, 0, -RotationIndex * 90f);
        }

        /// <summary>現在の向きで接続できる方向 (上=0,右=1,下=2,左=3)</summary>
        public bool[] GetConnections()
        {
            var c = new bool[4];
            if (Type == PipeType.Source || Type == PipeType.Exit)
            {
                c[0] = c[1] = c[2] = c[3] = true;
                return c;
            }
            if (Type == PipeType.Empty) return c;
            if (Type == PipeType.ValveClosed) return c;

            // 基本接続（rotation=0時）
            bool[] baseConn = GetBaseConnections();
            // RotationIndex分だけ右回転
            for (int i = 0; i < 4; i++)
            {
                int srcDir = (i - RotationIndex + 4) % 4;
                c[i] = baseConn[srcDir];
            }
            return c;
        }

        private bool[] GetBaseConnections()
        {
            var c = new bool[4];
            switch (Type)
            {
                case PipeType.Straight:
                case PipeType.Locked:
                    c[0] = c[2] = true; break; // 上下
                case PipeType.Elbow:
                    c[0] = c[1] = true; break; // 上右
                case PipeType.TJunction:
                    c[0] = c[1] = c[2] = true; break; // 上右下
                case PipeType.ValveOpen:
                    c[0] = c[2] = true; break; // Straightと同じ
            }
            return c;
        }

        public void Rotate()
        {
            if (IsLocked || _isAnimating) return;
            if (Type == PipeType.Source || Type == PipeType.Exit || Type == PipeType.Empty) return;

            int prevRot = RotationIndex;
            RotationIndex = (RotationIndex + 1) % 4;
            StartCoroutine(RotateAnim(prevRot * -90f, RotationIndex * -90f));
            OnTapped?.Invoke();
        }

        public void ToggleValve()
        {
            if (_isAnimating) return;
            if (Type != PipeType.ValveOpen && Type != PipeType.ValveClosed) return;

            IsValveOpen = !IsValveOpen;
            Type = IsValveOpen ? PipeType.ValveOpen : PipeType.ValveClosed;
            if (_sr != null)
                _sr.sprite = IsValveOpen ? SpriteValveOpen : SpriteValveClosed;
            StartCoroutine(ScalePulse());
            OnTapped?.Invoke();
        }

        private IEnumerator RotateAnim(float fromZ, float toZ)
        {
            _isAnimating = true;
            float t = 0f;
            float dur = 0.15f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float z = Mathf.Lerp(fromZ, toZ, t / dur);
                transform.localEulerAngles = new Vector3(0, 0, z);
                yield return null;
            }
            transform.localEulerAngles = new Vector3(0, 0, toZ);
            _isAnimating = false;
        }

        private IEnumerator ScalePulse()
        {
            _isAnimating = true;
            float t = 0f;
            float dur = 0.2f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float s = t < dur * 0.5f
                    ? Mathf.Lerp(1f, 1.3f, t / (dur * 0.5f))
                    : Mathf.Lerp(1.3f, 1f, (t - dur * 0.5f) / (dur * 0.5f));
                transform.localScale = Vector3.one * s;
                yield return null;
            }
            transform.localScale = Vector3.one;
            _isAnimating = false;
        }

        public void SetFlowColor(bool flowing)
        {
            if (_sr == null) return;
            _sr.color = flowing ? new Color(0.3f, 1f, 1f) : Color.white;
        }

        public void FlashError()
        {
            StartCoroutine(ErrorFlash());
        }

        public void ShakeLocked()
        {
            StartCoroutine(ShakeAnim());
        }

        private IEnumerator ErrorFlash()
        {
            if (_sr == null) yield break;
            _sr.color = new Color(1f, 0.3f, 0.3f);
            yield return new WaitForSeconds(0.3f);
            _sr.color = Color.white;
        }

        private IEnumerator ShakeAnim()
        {
            Vector3 orig = transform.localPosition;
            float dur = 0.3f;
            float mag = 0.05f;
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float ox = Mathf.Sin(t * 60f) * mag;
                transform.localPosition = orig + new Vector3(ox, 0, 0);
                yield return null;
            }
            transform.localPosition = orig;
        }

        public SpriteRenderer GetSpriteRenderer() => _sr;
    }
}
