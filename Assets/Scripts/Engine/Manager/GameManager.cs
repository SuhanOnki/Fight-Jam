using System.Collections.Generic;
using System.Linq;
using Engine.Enemy;
using Engine.Grid;
using Engine.Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Engine.Manager
{
    public class GameManager : MonoBehaviour
    {
        public List<Stickman.Stickman> stickmanList = new List<Stickman.Stickman>();
        public static GameManager gameManager;
        public DataManager dataManager;
        private UIManager _uiManager;
        public List<int> randomStickmans;
        public int randomStickmanIndex;
        public TargetCellManager targetCell;
        public PlayerInput playerInput;
        public List<StickmanCell> availableStickmanCells = new List<StickmanCell>();
        public List<Material> randomMaterials;
        public UIManager uiManager;
        public int[] priceOfBoosters;
        public EnemySpawnManager enemySpawnManager;
        public List<EnemyController> enemyControllers = new List<EnemyController>();

        private void Awake()
        {
            gameManager = this;
            Application.targetFrameRate = 60;
        }

        private void Start()
        {
            targetCell = TargetCellManager.targetCellManager;
            dataManager = DataManager.dataManager;
            _uiManager = UIManager.uiManager;
            playerInput = PlayerInput.playerInput;
            enemySpawnManager = EnemySpawnManager.enemySpawnManager;
        }


        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                dataManager.winLevel = 0;
                dataManager.Save();
            }
        }

        private void OnApplicationQuit()
        {
            if (dataManager != null)
            {
                dataManager.winLevel = 0;
                dataManager.Save();
            }
        }

        public void NextLevel()
        {
            playerInput.targetMoney += 50;
            playerInput.money = playerInput.targetMoney;
            dataManager.levelIndex++;
            if (dataManager.isRandomLevel())
            {
                dataManager.winLevel = 1;
            }
            else
            {
                dataManager.winLevel = 0;
            }

            YsoCorp.GameUtils.YCManager.instance.OnGameFinished(true);
            dataManager.Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void StickmanIndexSortByIndex()
        {
            var list = stickmanList.OrderBy(stickman => stickman.index);
            stickmanList = list.ToList();
        }

        public void CellIndexSortByIndex()
        {
            var list = availableStickmanCells.OrderBy(cell => cell.index);
            availableStickmanCells = list.ToList();
        }

        public void ChangeColorToOriginAllMysteryStickman(int buttonIndex)
        {
            if (playerInput.targetMoney >= priceOfBoosters[buttonIndex])
            {
                /*var animator = uiManager.boosterButtons[buttonIndex].gameObject.GetComponent<Animator>();
                animator.SetTrigger("Pressed");*/
                playerInput.targetMoney -= priceOfBoosters[buttonIndex];
                for (var i = 0; i < playerInput.mysteryStickmans.Count; i++)
                {
                    playerInput.mysteryStickmans[i].isMystery = false;
                    playerInput.ChangeMaterialToOrigin(playerInput.mysteryStickmans[i].cell);
                    playerInput.mysteryStickmans[i].mysterySign.SetActive(false);
                }

                playerInput.mysteryStickmans.Clear();
                if (playerInput.mysteryStickmans.Count <= 0)
                {
                    uiManager.boosterButtons[buttonIndex].gameObject.GetComponent<Button>().interactable = false;
                }

                CheckBooster();
            }
        }

        private void CheckBooster()
        {
            for (var i = 0; i < priceOfBoosters.Length; i++)
            {
                if (playerInput.targetMoney < priceOfBoosters[i])
                {
                    uiManager.boosterButtons[i].SetActive(false);
                    uiManager.inActiveBoosters[i].SetActive(true);
                }
            }
        }

        public void ColorChanger(int buttonIndex)
        {
            if (playerInput.targetMoney >= priceOfBoosters[buttonIndex])
            {
                playerInput.targetMoney -= priceOfBoosters[buttonIndex];

                for (var i = 0; i < gameManager.stickmanList.Count; i++)
                {
                    if (gameManager.stickmanList[i].targetCell == null && !gameManager.stickmanList[i].isMystery &&
                        gameManager.stickmanList[i]._enemyController == null)
                    {
                        playerInput.colorChangeStickmans.Add(gameManager.stickmanList[i].bodyColor);
                        playerInput.stickmansColorChange.Add(gameManager.stickmanList[i]);
                    }
                }

                int count = playerInput.colorChangeStickmans.Count;

                for (int i = 0; i < count; i++)
                {
                    int randomColorIndex = Random.Range(0, playerInput.colorChangeStickmans.Count);
                    playerInput.stickmansColorChange[i].skinnedMeshRenderer.material =
                        playerInput.colorChangeStickmans[randomColorIndex];

                    playerInput.stickmansColorChange[i].bodyColor = playerInput.colorChangeStickmans[randomColorIndex];
                    playerInput.colorChangeStickmans.RemoveAt(randomColorIndex);
                }

                playerInput.stickmansColorChange.Clear();
                CheckBooster();
            }
        }

        public void EnemySpeedDecreaser(int buttonIndex)
        {
            if (playerInput.targetMoney >= priceOfBoosters[buttonIndex])
            {
                bool decrease = false;
                bool isAgree = false;
                var list = enemySpawnManager.currentEnemies;
                for (var i = 0; i < list.Count; i++)
                {
                    if (list[i].isSpeedDownBoosterUsed == false)
                    {
                        list[i].speed /= 2;
                        list[i].isSpeedDownBoosterUsed = true;
                        enemyControllers.Add(list[i]);
                        if (!decrease)
                        {
                            playerInput.targetMoney -= priceOfBoosters[buttonIndex];
                            uiManager.boosterButtons[buttonIndex].gameObject.GetComponent<Button>().interactable =
                                false;
                            decrease = true;
                        }
                    }
                }

                CheckBooster();
            }
        }

        public void PlayAgain()
        {
            dataManager.winLevel = 0;
            RestartLevel(true);
        }

        public void RestartLevel(bool hasWon)
        {
            dataManager.winLevel = 0;
            Debug.Log(dataManager.winLevel);
            YsoCorp.GameUtils.YCManager.instance.OnGameFinished(hasWon);
            dataManager.Save();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public int CheckEqualsStickman()
        {
            List<EnemyController> enemyControllers = new List<EnemyController>();
            for (int i = 0; i < targetCell.targetCells.Count; i++)
            {
                for (int j = 0; j < targetCell.enemySpawnManager.currentEnemies.Count; j++)
                {
                    TargetCell targetCellTargetCell = targetCell.targetCells[i];
                    if (targetCellTargetCell.objectOn != null)
                    {
                        EnemyController enemyController = targetCell.enemySpawnManager.currentEnemies[j];
                        if (targetCellTargetCell.objectOn.skinnedMeshRenderer.material.color ==
                            enemyController.skinnedMeshRenderer.material.color && enemyController.targetObject == null)
                        {
                            if (!enemyControllers.Contains(enemyController))
                            {
                                enemyControllers.Add(enemyController);
                            }
                        }
                    }
                }
            }

            return enemyControllers.Count;
        }

        public int CheckFreeTargetCell()
        {
            var targetCellList = targetCell.targetCells.Where(cell => cell.objectOn == null).ToList();
            return targetCellList.Count;
        }

        public void CheckMatchStickman()
        {
            var equalCount = CheckEqualsStickman();
            Debug.Log(equalCount);
            var freeTargetCellCount = CheckFreeTargetCell();
            Debug.Log($"Free {freeTargetCellCount}");
            if (equalCount <= 0 && freeTargetCellCount <= 0)
            {
                Debug.Log($"equalCount <= 0");
                _uiManager.GameOver();
            }
        }
    }
}