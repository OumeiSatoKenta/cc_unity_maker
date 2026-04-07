using UnityEngine;
using System.Collections.Generic;

namespace Game060v2_MeltIce
{
    public class LightRaySystem : MonoBehaviour
    {
        [SerializeField] Material _lightMaterial;

        MeltIceGameManager _manager;
        List<LineRenderer> _lineRenderers = new List<LineRenderer>();
        const int MaxBounces = 12;
        const float RayStep = 0.1f;

        public void SetBoard(MeltIceGameManager manager)
        {
            _manager = manager;
        }

        public void RecalculateLightPath()
        {
            if (_manager == null) return;

            // Clear existing lines
            foreach (var lr in _lineRenderers)
                if (lr != null) Destroy(lr.gameObject);
            _lineRenderers.Clear();

            var data = _manager.CurrentStageData;
            // Sun starting position (just outside the grid)
            Vector3 sunWorld = _manager.GridToWorld(data.SunGridPos);
            // Offset to be just outside
            Vector2 startDir = new Vector2(data.SunDir.x, -data.SunDir.y); // flip Y for world space

            // Start ray slightly outside grid
            Vector3 startPos = sunWorld;

            var hitIces = new HashSet<IceBlockController>();
            // Trace single primary ray (prisms split into 2)
            TraceSingleRay(startPos, startDir, hitIces, new List<Vector3>(), 0);

            _manager.UpdateIceHitStates(hitIces);
        }

        void TraceSingleRay(Vector3 startPos, Vector2 direction, HashSet<IceBlockController> hitIces,
            List<Vector3> points, int depth)
        {
            if (depth > MaxBounces) return;

            var pts = new List<Vector3>();
            pts.Add(startPos);

            Vector3 pos = startPos;
            Vector2 dir = direction.normalized;
            float cellSize = _manager.CellSize;
            int maxSteps = 30;

            for (int step = 0; step < maxSteps; step++)
            {
                pos += new Vector3(dir.x, dir.y, 0f) * cellSize;
                var gridPos = _manager.WorldToGrid(pos);

                // Out of bounds: terminate ray
                if (!_manager.IsValidGridPos(gridPos))
                {
                    pts.Add(pos);
                    break;
                }

                // Check wall
                if (_manager.HasWallAt(gridPos))
                {
                    pts.Add(pos);
                    break;
                }

                // Check prism: split into 2 directions
                if (_manager.HasPrismAt(gridPos))
                {
                    pts.Add(pos);
                    // Prism splits light into 2 perpendicular rays
                    Vector2 perpDir = new Vector2(-dir.y, dir.x);
                    TraceSingleRay(pos, dir, hitIces, null, depth + 1);
                    TraceSingleRay(pos, perpDir, hitIces, null, depth + 1);
                    break;
                }

                // Check mirror
                var mirror = _manager.GetMirrorAt(gridPos);
                if (mirror != null)
                {
                    pts.Add(pos);
                    Vector2 reflected = mirror.GetReflectedDirection(dir);
                    TraceSingleRay(pos, reflected, hitIces, null, depth + 1);
                    break;
                }

                // Check ice blocks
                foreach (var ice in _manager.GetIceBlocks())
                {
                    if (ice == null) continue;
                    if (ice.CurrentGridPos == gridPos)
                    {
                        hitIces.Add(ice);
                        pts.Add(pos);
                        // Ray continues through ice (doesn't stop)
                        break;
                    }
                }
            }

            if (pts.Count < 2) return;

            CreateLineRenderer(pts);
        }

        void CreateLineRenderer(List<Vector3> points)
        {
            var obj = new GameObject("LightRay");
            obj.transform.SetParent(transform);
            var lr = obj.AddComponent<LineRenderer>();
            lr.positionCount = points.Count;
            lr.SetPositions(points.ToArray());
            lr.startWidth = 0.08f;
            lr.endWidth = 0.06f;
            lr.sortingOrder = 10;
            lr.useWorldSpace = true;

            if (_lightMaterial != null)
            {
                lr.material = _lightMaterial;
            }
            else
            {
                // Default bright yellow unlit material
                lr.material = new Material(Shader.Find("Sprites/Default"));
            }
            lr.startColor = new Color(1f, 0.95f, 0.2f, 0.95f);
            lr.endColor = new Color(1f, 0.8f, 0.1f, 0.7f);

            _lineRenderers.Add(lr);
        }

        void OnDestroy()
        {
            foreach (var lr in _lineRenderers)
                if (lr != null) Destroy(lr.gameObject);
        }
    }
}
