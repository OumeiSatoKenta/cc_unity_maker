using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game090_StarshipCrew
{
    public class CrewManager : MonoBehaviour
    {
        [SerializeField, Tooltip("ゲーム管理")] private StarshipCrewGameManager _gameManager;
        [SerializeField, Tooltip("クルースプライト")] private Sprite _crewSprite;
        [SerializeField, Tooltip("宇宙船スプライト")] private Sprite _starshipSprite;
        [SerializeField, Tooltip("クルー追加コスト")] private int _recruitCost = 8;
        [SerializeField, Tooltip("ミッションコスト")] private int _missionCost = 15;

        private bool _isActive;
        private int _crewCount;
        private int _completedMissions;
        private float _incomeTimer;

        public void StartGame()
        {
            _isActive = true;
            _crewCount = 1;
            _completedMissions = 0;
            _incomeTimer = 0f;

            var ship = new GameObject("Starship");
            ship.transform.position = new Vector3(0f, 1f, 0f);
            var sr = ship.AddComponent<SpriteRenderer>();
            sr.sprite = _starshipSprite; sr.sortingOrder = 2;
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;
            if (_crewCount > 0) _incomeTimer += Time.deltaTime;
        }

        public void RecruitCrew()
        {
            int cost = _recruitCost + _crewCount * 5;
            if (_gameManager.TrySpend(cost))
            {
                _crewCount++;
                // Visual: add crew member
                var obj = new GameObject($"Crew_{_crewCount}");
                obj.transform.position = new Vector3(Random.Range(-2f, 2f), Random.Range(-3f, -1f), 0f);
                var sr = obj.AddComponent<SpriteRenderer>();
                sr.sprite = _crewSprite; sr.sortingOrder = 3;
                obj.transform.localScale = Vector3.one * 0.5f;
                sr.color = Color.HSVToRGB(Random.Range(0f, 1f), 0.3f, 1f);
            }
        }

        public void StartMission()
        {
            if (_crewCount < 2) return;
            int cost = _missionCost + _completedMissions * 10;
            if (_gameManager.TrySpend(cost))
            {
                _completedMissions++;
            }
        }

        public int AutoIncome
        {
            get
            {
                if (_crewCount <= 0) return 0;
                if (_incomeTimer >= 2f) { _incomeTimer -= 2f; return _crewCount; }
                return 0;
            }
        }

        public int CrewCount => _crewCount;
        public int CompletedMissions => _completedMissions;
        public int NextRecruitCost => _recruitCost + _crewCount * 5;
        public int NextMissionCost => _missionCost + _completedMissions * 10;
    }
}
