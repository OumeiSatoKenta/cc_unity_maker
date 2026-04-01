using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace Game088_AlchemyPet
{
    public class AlchemyManager : MonoBehaviour
    {
        [SerializeField, Tooltip("フラスコスプライト")] private Sprite _flaskSprite;
        [SerializeField, Tooltip("ペットスプライト")] private Sprite _petSprite;

        private bool _isActive;
        private int[] _elements; // 0=fire, 1=water, 2=earth
        private HashSet<int> _discoveredPets = new HashSet<int>();
        private int _selectedA = -1;
        private int _selectedB = -1;
        private float _gatherTimer;

        private static readonly string[] ElementNames = { "火", "水", "土" };
        // Recipes: pair -> pet id
        private static readonly int[,] PetTable = {
            { 0, 1, 2 }, // fire+fire=0, fire+water=1, fire+earth=2
            { 1, 3, 4 }, // water+fire=1, water+water=3, water+earth=4
            { 2, 4, -1 }, // earth+fire=2, earth+water=4, earth+earth=invalid
        };

        public void StartGame()
        {
            _isActive = true;
            _elements = new int[] { 3, 3, 3 };
            _gatherTimer = 0f;
        }

        public void StopGame() { _isActive = false; }

        private void Update()
        {
            if (!_isActive) return;
            _gatherTimer += Time.deltaTime;
            if (_gatherTimer >= 3f)
            {
                _gatherTimer = 0f;
                _elements[Random.Range(0, 3)]++;
            }
        }

        public void SelectElement(int index)
        {
            if (!_isActive || index < 0 || index >= 3) return;
            if (_elements[index] <= 0) return;

            if (_selectedA < 0) { _selectedA = index; }
            else if (_selectedB < 0) { _selectedB = index; TryAlchemy(); }
        }

        public void ClearSelection() { _selectedA = -1; _selectedB = -1; }

        private void TryAlchemy()
        {
            int a = _selectedA, b = _selectedB;
            int petId = PetTable[a, b];
            if (petId >= 0)
            {
                _elements[a]--;
                _elements[b]--;
                _discoveredPets.Add(petId);
            }
            _selectedA = -1; _selectedB = -1;
        }

        public string GetElementText()
        {
            return $"{ElementNames[0]}: {_elements[0]}  {ElementNames[1]}: {_elements[1]}  {ElementNames[2]}: {_elements[2]}";
        }

        public int DiscoveredPets => _discoveredPets.Count;
    }
}
