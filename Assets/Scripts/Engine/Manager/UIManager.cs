using System;
using Engine.Player;
using Engine.ScriptableObject;
using TMPro;
using UnityEngine;

namespace Engine.Manager
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager uiManager;
        public TextMeshProUGUI fpsCounter;
        private int _lastFrameIndex;
        private float[] _frameDeltaTimeArray;
        public GameObject gameOver;
        public GameObject winPanel;
        public PlayerInput _playerInput;
        public TextMeshProUGUI moneyText;
        public TextMeshProUGUI levelText;
        private DataManager _dataManager;
        public TextMeshProUGUI summonEarndedMoney;
        public TextMeshProUGUI summonEarndedMoneygameOver;
        public TextMeshProUGUI gameOverLevelCompleteText;
        public TextMeshProUGUI winLevelCompleteText;
        public TextMeshProUGUI gameOverLevelCompleteTextOutline;
        public TextMeshProUGUI winLevelCompleteTextOutline;
        public ParticleSystem[] confetti;
        public GameObject[] boosterButtons;
        public BoosterLocked[] boosterLocked;
        public GameObject[] lockedBoosters;
        public GameManager gameManager;
        public GameObject[] inActiveBoosters;
        public TextMeshProUGUI[] boostersPrice;

        private void Awake()
        {
            uiManager = this;
            _frameDeltaTimeArray = new float[50];
        }

        private void Start()
        {
            _dataManager = DataManager.dataManager;
            InitializeBoosters();
        }

        void Update()
        {
            _frameDeltaTimeArray[_lastFrameIndex] = Time.deltaTime;
            _lastFrameIndex = (_lastFrameIndex + 1) % _frameDeltaTimeArray.Length;
            //fpsCounter.text = $"{Mathf.RoundToInt(CalculateFPS())}:FPS";
        }

        public void InitializeBoosters()
        {
            for (int i = 0; i < boosterLocked.Length; i++)
            {
                if (boosterLocked[i].reqLevel <= _dataManager.levelIndex)
                {
                    lockedBoosters[i].SetActive(false);
                    boosterButtons[i].SetActive(true);
                }

                if (gameManager.priceOfBoosters[i] > _playerInput.money &&
                    boosterButtons[i].activeInHierarchy)
                {
                    boosterButtons[i].SetActive(false);
                    inActiveBoosters[i].SetActive(true);
                }
            }
        } /*
        private float CalculateFPS()
        {
            float total = 0f;
            foreach (float deltaTime in _frameDeltaTimeArray)
            {
                total += deltaTime;
            }

            return _frameDeltaTimeArray.Length / total;
        }*/

        public void GameOver()
        {
            Time.timeScale = 0;
            _playerInput.targetMoney += 20;
            _playerInput.money = _playerInput.targetMoney;
            gameOverLevelCompleteText.text = $"LEVEL {_dataManager.levelIndex + 1} FAILED!";
            gameOverLevelCompleteTextOutline.text = $"LEVEL {_dataManager.levelIndex + 1} FAILED!";
            summonEarndedMoneygameOver.text = $"{20}";
            _dataManager.winLevel = 0;
            gameOver.SetActive(true);
            winPanel.SetActive(false);
        }

        public void Win()
        {
            _playerInput.money = _playerInput.targetMoney;
            summonEarndedMoney.text = $"{50}";
            winLevelCompleteText.text = $"LEVEL {_dataManager.levelIndex + 1} COMPLETE!";
            if (_dataManager.isRandomLevel())
            {
                _dataManager.winLevel = 1;
            }
            else
            {
                _dataManager.winLevel = 0;
            }

            winLevelCompleteTextOutline.text = $"LEVEL {_dataManager.levelIndex + 1} COMPLETE!";
            winPanel.SetActive(true);
            gameOver.SetActive(false);
        }
    }
}