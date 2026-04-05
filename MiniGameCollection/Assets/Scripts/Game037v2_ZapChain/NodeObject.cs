using System;
using System.Collections;
using UnityEngine;

namespace Game037v2_ZapChain
{
    public enum NodeType
    {
        Normal,
        Obstacle,
        Moving,
        Timed
    }

    public class NodeObject : MonoBehaviour
    {
        public NodeType NodeType { get; private set; }
        public bool IsConnected { get; private set; }
        public bool IsActive { get; private set; }

        [SerializeField] SpriteRenderer _renderer;

        Sprite _normalSprite;
        Sprite _connectedSprite;
        Sprite _activeSprite;

        // Moving node
        Vector3 _orbitCenter;
        float _orbitRadius;
        float _orbitAngle;
        float _orbitSpeed;

        // Timed node
        float _timedDuration;
        float _timedRemaining;
        bool _timedActive;
        public event Action OnTimedExpired;

        public void Initialize(NodeType type, Sprite normalSprite, Sprite connectedSprite, Sprite activeSprite)
        {
            NodeType = type;
            _normalSprite = normalSprite;
            _connectedSprite = connectedSprite;
            _activeSprite = activeSprite;

            if (_renderer == null) _renderer = GetComponent<SpriteRenderer>();
            _renderer.sprite = _normalSprite;

            if (type == NodeType.Obstacle)
            {
                _renderer.color = Color.white;
            }
        }

        public void SetupMoving(Vector3 center, float radius, float startAngle, float speed)
        {
            _orbitCenter = center;
            _orbitRadius = radius;
            _orbitAngle = startAngle;
            _orbitSpeed = speed;
        }

        public void StartTimed(float duration)
        {
            _timedDuration = duration;
            _timedRemaining = duration;
            _timedActive = true;
        }

        void Update()
        {
            if (NodeType == NodeType.Moving && !IsConnected)
            {
                _orbitAngle += _orbitSpeed * Time.deltaTime;
                float x = _orbitCenter.x + Mathf.Cos(_orbitAngle) * _orbitRadius;
                float y = _orbitCenter.y + Mathf.Sin(_orbitAngle) * _orbitRadius;
                transform.position = new Vector3(x, y, 0f);
            }

            if (_timedActive && !IsConnected)
            {
                _timedRemaining -= Time.deltaTime;
                // Color shifts to red as time runs out
                float ratio = _timedRemaining / _timedDuration;
                _renderer.color = Color.Lerp(Color.red, Color.white, ratio);

                if (_timedRemaining <= 0f)
                {
                    _timedActive = false;
                    OnTimedExpired?.Invoke();
                }
            }
        }

        public void SetActive(bool active)
        {
            IsActive = active;
            if (!IsConnected)
            {
                _renderer.sprite = active ? _activeSprite : _normalSprite;
            }
        }

        public void SetConnected()
        {
            IsConnected = true;
            IsActive = false;
            _timedActive = false;
            _renderer.sprite = _connectedSprite;
            StartCoroutine(PulseEffect());
        }

        IEnumerator PulseEffect()
        {
            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                float scale = 1f + 0.3f * Mathf.Sin(t / 0.2f * Mathf.PI);
                transform.localScale = Vector3.one * scale;
                yield return null;
            }
            transform.localScale = Vector3.one;
        }

        public void FlashRed()
        {
            StartCoroutine(RedFlash());
        }

        IEnumerator RedFlash()
        {
            _renderer.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            _renderer.color = Color.white;
        }

        public void ResetNode()
        {
            IsConnected = false;
            IsActive = false;
            _renderer.sprite = _normalSprite;
            _renderer.color = Color.white;
            transform.localScale = Vector3.one;
        }
    }
}
