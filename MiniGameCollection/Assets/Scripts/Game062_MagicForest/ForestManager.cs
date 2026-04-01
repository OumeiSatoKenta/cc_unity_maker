using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game062_MagicForest
{
    public class ForestManager : MonoBehaviour
    {
        [SerializeField, Tooltip("小さい木")] private Sprite _treeSmallSprite;
        [SerializeField, Tooltip("大きい木")] private Sprite _treeBigSprite;
        [SerializeField, Tooltip("成長速度")] private float _growSpeed = 0.5f;
        [SerializeField, Tooltip("自動拡散間隔")] private float _spreadInterval = 5f;

        private Camera _mainCamera;
        private bool _isActive;
        private List<TreeData> _trees = new List<TreeData>();
        private float _spreadTimer;

        private class TreeData
        {
            public GameObject Obj;
            public SpriteRenderer Sr;
            public float Growth; // 0=sapling, 1=full
            public bool IsGrown;
        }

        private void Awake() { _mainCamera = Camera.main; }

        public void StartGame()
        {
            _isActive = true;
            _spreadTimer = _spreadInterval;

            // Plant initial tree
            PlantTree(new Vector2(0f, -1f));
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;

            // Handle tap to grow nearest tree
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector3 mp = Mouse.current.position.ReadValue();
                mp.z = -_mainCamera.transform.position.z;
                Vector2 wp = _mainCamera.ScreenToWorldPoint(mp);

                TreeData nearest = FindNearest(wp, 2f);
                if (nearest != null && !nearest.IsGrown)
                {
                    GrowTree(nearest, 0.2f);
                }
                else if (nearest == null)
                {
                    // Plant new tree if no nearby tree
                    PlantTree(wp);
                }
            }

            // Natural growth
            foreach (var t in _trees)
            {
                if (!t.IsGrown)
                {
                    GrowTree(t, _growSpeed * 0.1f * Time.deltaTime);
                }
            }

            // Auto spread
            _spreadTimer -= Time.deltaTime;
            if (_spreadTimer <= 0f)
            {
                _spreadTimer = _spreadInterval;
                AutoSpread();
            }
        }

        private void PlantTree(Vector2 pos)
        {
            var obj = new GameObject($"Tree_{_trees.Count}");
            obj.transform.position = pos;
            var sr = obj.AddComponent<SpriteRenderer>();
            sr.sprite = _treeSmallSprite;
            sr.sortingOrder = 3;
            obj.transform.localScale = Vector3.one * 0.3f;

            var data = new TreeData { Obj = obj, Sr = sr, Growth = 0f, IsGrown = false };
            _trees.Add(data);
        }

        private void GrowTree(TreeData tree, float amount)
        {
            tree.Growth = Mathf.Min(tree.Growth + amount, 1f);
            float scale = Mathf.Lerp(0.3f, 1f, tree.Growth);
            tree.Obj.transform.localScale = Vector3.one * scale;

            if (tree.Growth >= 0.6f && tree.Sr.sprite != _treeBigSprite)
            {
                tree.Sr.sprite = _treeBigSprite;
            }

            if (tree.Growth >= 1f)
            {
                tree.IsGrown = true;
            }
        }

        private void AutoSpread()
        {
            var grownTrees = _trees.FindAll(t => t.IsGrown);
            if (grownTrees.Count == 0) return;

            var parent = grownTrees[Random.Range(0, grownTrees.Count)];
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float dist = Random.Range(1f, 2f);
            Vector2 newPos = (Vector2)parent.Obj.transform.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * dist;

            // Check not too close to existing trees
            if (FindNearest(newPos, 0.8f) == null)
            {
                PlantTree(newPos);
            }
        }

        private TreeData FindNearest(Vector2 pos, float maxDist)
        {
            TreeData nearest = null;
            float bestDist = maxDist;
            foreach (var t in _trees)
            {
                if (t.Obj == null) continue;
                float d = Vector2.Distance(pos, t.Obj.transform.position);
                if (d < bestDist) { bestDist = d; nearest = t; }
            }
            return nearest;
        }

        public int TreeCount => _trees.Count;
    }
}
