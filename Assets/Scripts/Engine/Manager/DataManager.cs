using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Enemy;
using Engine.Grid;
using Engine.Player;
using Engine.ScriptableObject;
using NINESOFT.TUTORIAL_SYSTEM;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Engine.Manager
{
    public class DataManager : MonoBehaviour
    {
        public List<StickmanCell> cells;
        public int levelIndex;
        public GameManager gameManager;
        public int[] sequenceMaterials;
        public static DataManager dataManager;
        public LevelData currentLevel;
        public int startRowCell;
        public int endRowCell;
        public PlayerInput _playerInput;
        public int winLevel;
        public List<String> LevelDataNames = new List<string>();
        public int stickmanCount;
        public EnemySpawnManager enemySpawnManager;
        public List<LevelData> levelDataList;
        public float stickmanCellOffset;
        public int levelCount;
        public string currentRandomLevel;
        public List<int> addedColors = new List<int>();
        public int removeLevelCount;
        public List<TargetCell> notBuyed = new List<TargetCell>();
        public int randomLevelIndex;
        public bool isTutorial;
        public List<LevelData> randomLevels = new List<LevelData>();
        public List<TowerData> towerDatas = new List<TowerData>();
        public float enemySpawnTime;
        public TutorialManager tutorialManager;
        public LevelType levelType;
        public int[] targetCellReqLevel;

        private void Awake()
        {
            Load();
            LoadRunTimeDatas();
            dataManager = this;
        }

        private void Start()
        {
            YsoCorp.GameUtils.YCManager.instance.OnGameStarted(levelIndex + 1);
            enemySpawnManager = EnemySpawnManager.enemySpawnManager;
        }

        private void LoadRunTimeDatas()
        {
            levelType = currentLevel.levelType;
            towerDatas = currentLevel.towerDatas;
            if (levelType == LevelType.AllInOneColor && !currentLevel.isSetManual)
            {
                var random = Random.Range(0, gameManager.playerInput.outlinedMaterials.Count);
                for (int i = 0; i < stickmanCount; i++)
                {
                    gameManager.randomStickmans.Add(random);
                }
            }
            else if (levelType == LevelType.AllInOtherColor)
            {
                for (int i = 0; i < stickmanCount; i++)
                {
                    var random = Random.Range(0, gameManager.playerInput.outlinedMaterials.Count);
                    if (!addedColors.Contains(random))
                    {
                        addedColors.Add(random);
                        gameManager.randomStickmans.Add(random);
                    }
                }
            }
            else if (levelType == LevelType.Default)
            {
                for (int i = 0; i < stickmanCount; i++)
                {
                    var random = Random.Range(0, gameManager.playerInput.outlinedMaterials.Count);
                    addedColors.Add(random);
                    gameManager.randomStickmans.Add(random);
                }
            }

            for (int i = 0; i < currentLevel.stickmanDatas.Count; i++)
            {
                for (int j = 0; j < cells.Count; j++)
                {
                    if (currentLevel.stickmanDatas[i].cellIndex == cells[j].index)
                    {
                        gameManager.availableStickmanCells.Add(cells[j]);
                        cells[j].stickmanData = currentLevel.stickmanDatas[i];
                        cells[j].isEnable = true;
                        cells[j].enabled = true;
                        break;
                    }
                }
            }

            for (int i = 0; i < towerDatas.Count; i++)
            {
                for (int j = 0; j < cells.Count; j++)
                {
                    if (towerDatas[i].positionIndex == cells[j].index)
                    {
                    }
                }
            }

            for (int i = 0; i < gameManager.availableStickmanCells.Count; i++)
            {
                gameManager.availableStickmanCells[i].readyForInitialize = true;
            }

            HideOtherCells(true);
        }

        public void FinishTutorial()
        {
            isTutorial = false;
        }

        public void Save()
        {
            PlayerPrefs.SetInt("LevelIndex", levelIndex);
            PlayerPrefs.SetInt("Money", _playerInput.money);
            PlayerPrefs.SetInt("RandomLevel", randomLevelIndex);
            PlayerPrefs.SetInt("Win", winLevel);
            PlayerPrefs.SetString("CurrentRandomLevel", currentRandomLevel);
            for (int i = 0; i < notBuyed.Count; i++)
            {
                PlayerPrefs.SetInt($"CellFree {i}", notBuyed[i].isFree);
            }

            for (var i = 0; i < LevelDataNames.Count; i++)
            {
                PlayerPrefs.SetString($"Random Level {i}", LevelDataNames[i]);
            }

            PlayerPrefs.SetInt("RemoveLevelCount", LevelDataNames.Count);

            PlayerPrefs.Save();
        }

        public void Load()
        {
            Time.timeScale = 1;
            _playerInput.money = PlayerPrefs.GetInt("Money", 0);
            levelIndex = PlayerPrefs.GetInt("LevelIndex", 0);
            randomLevelIndex = PlayerPrefs.GetInt("RandomLevel");
            winLevel = PlayerPrefs.GetInt("Win", 0);
            removeLevelCount = PlayerPrefs.GetInt("RemoveLevelCount", 0);
            currentRandomLevel = PlayerPrefs.GetString("CurrentRandomLevel");
            Debug.Log(randomLevelIndex + ": index");
            for (int i = 0; i < notBuyed.Count; i++)
            {
                if (targetCellReqLevel[i] <= levelIndex)
                {
                    notBuyed[i].OpenCell(1);
                }
            }

            if (removeLevelCount == randomLevels.Count)
            {
                removeLevelCount = 0;
            }

            for (var i = 0; i < removeLevelCount; i++)
            {
                var name = PlayerPrefs.GetString($"Random Level {i}");
                LevelDataNames.Add(name);
                for (var i1 = 0; i1 < randomLevels.Count; i1++)
                {
                    Debug.Log($"RANDOMlEVEL .{currentRandomLevel}. && .{name}. and boo {currentRandomLevel != name}");
                    if (randomLevels[i1].name == name && currentRandomLevel != name)
                    {
                        randomLevels.Remove(randomLevels[i1]);
                        break;
                    }
                }
            }

            if (levelIndex >= levelCount && winLevel == 1)
            {
                int randomLevel = Random.Range(0, randomLevels.Count);
                Debug.Log($"random lEVEL {randomLevel}");
                Debug.Log(!LevelDataNames.Contains(randomLevels[randomLevel].name));
                if (!LevelDataNames.Contains(randomLevels[randomLevel].name))
                {
                    LevelDataNames.Add(randomLevels[randomLevel].name);
                    randomLevelIndex = randomLevel;
                    currentLevel = randomLevels[randomLevelIndex];
                    currentRandomLevel = currentLevel.name;
                    randomLevels.Remove(randomLevels[randomLevelIndex]);
                }
                else
                {
                    Debug.Log($"random lEVEL {randomLevel} 2");
                    Debug.Log(!LevelDataNames.Contains(randomLevels[randomLevel].name) + "2");
                    randomLevel = Random.Range(0, randomLevels.Count);
                    if (!LevelDataNames.Contains(randomLevels[randomLevel].name))
                    {
                        LevelDataNames.Add(randomLevels[randomLevel].name);
                        randomLevelIndex = randomLevel;
                        currentLevel = randomLevels[randomLevelIndex];
                        randomLevels.Remove(randomLevels[randomLevelIndex]);
                    }
                }
            }
            else if (levelIndex >= levelDataList.Count && winLevel == 0)
            {
                currentLevel = randomLevels[randomLevelIndex];
            }
            else if (levelIndex < levelDataList.Count)
            {
                currentLevel = levelDataList[levelIndex];
            }

            sequenceMaterials = currentLevel.sequanceOfMaterials.ToArray();
            stickmanCount = currentLevel.stickmanCount;
            enemySpawnTime = currentLevel.enenySpawnTime;
            isTutorial = currentLevel.isTutorial;
            if (isTutorial)
            {
                tutorialManager.enabled = true;
                _playerInput.tapToPlay.gameObject.SetActive(true);
                tutorialManager.gameObject.SetActive(true);
            }
        }

        private void HideOtherCells(bool isHideAll)
        {
            if (!isHideAll)
            {
                for (int i = 0; i < cells.Count; i++)
                {
                    if (!gameManager.availableStickmanCells.Contains(cells[i]))
                    {
                        cells[i].gameObject.tag = "Untagged";
                        cells[i].sec.enabled = false;
                        cells[i].gameObject.GetComponent<BoxCollider>().enabled = false;
                        cells[i].enabled = false;
                    }
                }
            }
            else
            {
                for (int i = 0; i < cells.Count; i++)
                {
                    if (!gameManager.availableStickmanCells.Contains(cells[i]))
                    {
                        cells[i].gameObject.tag = "Untagged";
                        cells[i].gameObject.SetActive(false);
                        cells[i].gameObject.GetComponent<BoxCollider>().enabled = false;
                        cells[i].enabled = false;
                    }
                }
            }

            gameManager.CellIndexSortByIndex();
            gameManager.StickmanIndexSortByIndex();
        }

        public bool isRandomLevel()
        {
            bool isRandom = false;
            if (levelIndex >= levelCount)
            {
                isRandom = true;
            }

            return isRandom;
        }
    }
}