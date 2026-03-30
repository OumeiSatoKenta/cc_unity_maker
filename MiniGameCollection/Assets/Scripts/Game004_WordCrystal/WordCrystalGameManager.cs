using UnityEngine;
using UnityEngine.Events;

namespace Game004_WordCrystal
{
    public class WordCrystalGameManager : MonoBehaviour
    {
        [SerializeField] private CrystalManager _crystalManager;
        [SerializeField] private WordCrystalUI _ui;

        public static readonly string[] WordList =
        {
            "CAT", "DOG", "RUN", "SUN", "FUN", "HAT", "MAP", "JAR", "BOX",
            "CUP", "PAN", "BAT", "RAT", "MAT", "SAT", "FAT", "PAT",
            "HIT", "SIT", "BIT", "FIT", "WIT", "PIT", "KIT", "LIT",
            "HOP", "TOP", "MOP", "COP", "POP",
            "PLAY", "STAY", "CLAY", "GRAY", "PRAY", "TRAY",
            "BLUE", "CLUE", "GLUE", "TRUE", "FLUE",
            "GAME", "FAME", "NAME", "SAME", "TAME", "CAME", "DAME", "LAME",
            "FIRE", "HIRE", "TIRE", "WIRE", "SIRE",
            "STAR", "SCAR", "CHAR",
            "WORD", "CORD", "FORD", "LORD", "HORN", "CORN", "BORN", "TORN",
            "FISH", "DISH", "WISH",
            "DROP", "STOP", "SHOP", "CHOP", "CROP",
            "TREE", "FREE", "KNEE", "FLEE",
            "CAKE", "LAKE", "MAKE", "RAKE", "TAKE", "WAKE", "BAKE", "FAKE",
            "BIKE", "HIKE", "LIKE", "MIKE",
            "BONE", "CONE", "LONE", "TONE", "ZONE",
            "BRAIN", "TRAIN", "CHAIN", "PLAIN", "GRAIN",
            "STONE", "PHONE", "CLONE", "DRONE",
            "LIGHT", "NIGHT", "RIGHT", "TIGHT", "FIGHT", "MIGHT", "SIGHT",
            "BREAD", "TREAD", "DREAD",
            "HEART", "START", "SMART", "CHART",
            "BOUND", "FOUND", "HOUND", "MOUND", "ROUND", "SOUND",
            "SLEEP", "STEEP", "SWEEP", "CREEP",
            "BLOOM", "BROOM", "GROOM",
            "BRAVE", "GRAVE", "CRAVE", "SLAVE", "CRANE",
            "MAGIC", "BASIC", "MUSIC",
            "HAPPY", "PUPPY", "SUNNY", "FUNNY",
            "CHESS", "BLESS", "DRESS", "PRESS",
            "PLANT", "SLANT", "GRANT", "CHANT",
            "DRINK", "BLINK", "BRINK", "THINK",
            "PLACE", "GRACE", "TRACE", "SPACE",
            "FLAME", "BLAME", "FRAME",
            "SPEED", "GREED", "CREED",
            "CLOUD", "PROUD", "CROWD", "SHROUD",
            "SPORT", "SHORT", "SNORT",
            "NORTH", "FORTH",
            "SWORD", "WORLD"
        };

        private float _timeLeft = 60f;
        private int _score;
        private bool _isPlaying;

        public bool IsPlaying => _isPlaying;

        public UnityEvent<int> OnScoreChanged = new();
        public UnityEvent<float> OnTimeChanged = new();
        public UnityEvent OnGameOver = new();

        private void Start() => StartGame();

        public void StartGame()
        {
            _timeLeft = 60f;
            _score = 0;
            _isPlaying = true;
            _crystalManager.GenerateRound();
            OnScoreChanged?.Invoke(_score);
            OnTimeChanged?.Invoke(_timeLeft);
        }

        private void Update()
        {
            if (!_isPlaying) return;
            _timeLeft -= Time.deltaTime;
            if (_timeLeft <= 0f)
            {
                _timeLeft = 0f;
                _isPlaying = false;
                OnTimeChanged?.Invoke(_timeLeft);
                OnGameOver?.Invoke();
                return;
            }
            OnTimeChanged?.Invoke(_timeLeft);
        }

        public bool SubmitWord(string word)
        {
            if (!_isPlaying) return false;
            word = word.ToUpper();
            bool valid = System.Array.Exists(WordList, w => w == word);
            if (valid)
            {
                _score += word.Length * 10;
                OnScoreChanged?.Invoke(_score);
                _crystalManager.GenerateRound();
            }
            return valid;
        }

        public int GetScore() => _score;

        public void RestartGame()
        {
            _ui.HideGameOverPanel();
            StartGame();
        }

        public void LoadMenu()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("TopMenu");
        }
    }
}
