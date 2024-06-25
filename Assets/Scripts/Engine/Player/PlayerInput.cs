using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DG.Tweening;
using Engine.Enemy;
using Engine.Grid;
using Engine.Manager;
using Engine.Tower;
using Mono.CSharp;
using NINESOFT.TUTORIAL_SYSTEM;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Engine.Player
{
    public class PlayerInput : MonoBehaviour
    {
        public static PlayerInput playerInput;
        public GameManager _gameManager;
        public TargetCellManager _targetCellManager;
        private EnemySpawnManager _enemySpawnManager;
        public int money;
        private int _stageIndex;
        public int targetMoney;
        public ObjectPool stickmanPooler;
        public DataManager _dataManager;
        public float spawnTime;
        private float _spawnTime;
        public Stickman.Stickman stickman;
        public bool startToSpawnEnemies;
        public int moveSpeed;
        public List<Stickman.Stickman> stickmansSpawnedFromTower;
        public List<Material> towerSpawnStickmans;
        private UIManager _uiManager;
        private bool isAttack;
        public float stickmanMoveSpeed;
        public GameObject stickmanPoint;
        public Transform stickmanWayPoint;
        public int randomPunch;
        public Vector3 towerSpawnPositionOffset;
        public int summonEarnedMoney;
        private bool isTutorial;
        public int towerCount;
        public int stickmanCount;
        public Material mysteryMaterial;
        public bool isLocked;
        public int stickmanIndex;
        public GameObject tapToPlay;
        public GameObject stickmanParent;
        public List<StickmanCell> notCheckedStickmanCells = new List<StickmanCell>();
        public Animator stickmanKingAnimator;
        public List<Material> outlinedMaterials;
        public ObjectPool towerPooler;
        public List<TowerController> towerControllersList = new List<TowerController>();
        public TowerController towerPrefab;
        private GameObject _towerParent;
        public List<String> attackingAnimationName = new List<string>();
        public List<Material> colorChangeStickmans = new List<Material>();
        public List<Stickman.Stickman> stickmansColorChange = new List<Stickman.Stickman>();
        public List<Stickman.Stickman> mysteryStickmans = new List<Stickman.Stickman>();

        private void Awake()
        {
            playerInput = this;
        }


        void Start()
        {
            isLocked = true;
            _dataManager = DataManager.dataManager;
            _uiManager = UIManager.uiManager;
            _gameManager = GameManager.gameManager;
            _targetCellManager = TargetCellManager.targetCellManager;
            _enemySpawnManager = EnemySpawnManager.enemySpawnManager;
            _towerParent = new GameObject();
            _towerParent.name = "Tower Parent";
            _uiManager.moneyText.text = $"{money}";
            _uiManager.levelText.text = $"Level {_dataManager.levelIndex + 1}";
            targetMoney = money;
            stickmanParent = new GameObject();
            stickmanParent.name = $"Stickman Parent";
            stickmanPooler = ObjectPool.CreateInstance(stickman,
                _dataManager.stickmanCount * 2 + _dataManager.currentLevel.towerDatas.Count *
                _dataManager.currentLevel.towerSpawnTime,
                stickmanPoint.transform,
                null, false);
            towerPooler = ObjectPool.CreateInstance(towerPrefab, _dataManager.currentLevel.towerDatas.Count,
                _towerParent.transform,
                null, false);
            _spawnTime = spawnTime;
            stickmanCount = _dataManager.stickmanCount;
            towerCount = _dataManager.currentLevel.towerDatas.Count;
            for (int i = 0; i < towerCount; i++)
            {
                for (int j = 0; j < _dataManager.cells.Count; j++)
                {
                    if (_dataManager.towerDatas[i].positionIndex == _dataManager.cells[j].index)
                    {
                        var tower = towerPooler.GetObject();
                        var position = _dataManager.cells[j].transform.position;
                        position = new Vector3(position.x + towerSpawnPositionOffset.x,
                            position.y + towerSpawnPositionOffset.y, position.z + towerSpawnPositionOffset.z);
                        tower.transform.position = position;
                        tower.transform.localRotation = Quaternion.Euler(-90, 0, 90);
                        var towerObject = tower.gameObject.GetComponent<TowerController>();
                        towerObject.towerData = _dataManager.towerDatas[i];
                        for (int k = 0; k < _dataManager.cells.Count; k++)
                        {
                            if (_dataManager.cells[k].index == _dataManager.towerDatas[i].spawntargetCell)
                            {
                                towerObject.targetSpawnCell = _dataManager.cells[k];
                                towerObject.targetSpawnCell.towerController = towerObject;
                                towerControllersList.Add(towerObject);
                                break;
                            }
                        }

                        break;
                    }
                }
            }
        }

        void Update()
        {
            _uiManager.moneyText.text = $"{money}";
            if (money != targetMoney)
            {
                var moneyf = Mathf.MoveTowards(money, targetMoney, Time.deltaTime * 30);
                money = (int)moneyf;
            }

            if (_spawnTime <= 0 && stickmanCount > 0)
            {
                _spawnTime = spawnTime;
                stickmanCount--;
                /*if (!isAttack)
                {
                    isAttack = true;
                    KingOrderAnimation();
                }
                */

                var stickmanObject = stickmanPooler.GetObject();
                stickmanObject.transform.localPosition = Vector3.zero;
                var availableStickmanCells = _gameManager.availableStickmanCells.Where(cell => cell.objectOn == null);
                var list = availableStickmanCells.OrderBy(cell => cell.index);
                var stickmanPoolable = stickmanObject.gameObject.GetComponent<Stickman.Stickman>();
                stickmanPoolable.playerInput = this;
                stickmanPoolable.transform.SetParent(stickmanParent.transform);
                if (list.Count() > 0)
                {
                    if (list.Count() == 1)
                    {
                        stickmanPoolable.startToCheckCells = true;
                    }

                    stickmanPoolable.cell = list.ElementAt(0);
                    /*if (randomPunch == 0)
                    {
                        randomPunch = 1;
                    }
                    else if (randomPunch == 1)
                    {
                        randomPunch = 0;
                    }
                    */
                    randomPunch = Random.Range(0, attackingAnimationName.Count);

                    stickmanPoolable.name = $"Stickman {stickmanIndex}";
                    stickmanPoolable.randomPunch = randomPunch;
                    stickmanPoolable.cell.objectOn = stickmanPoolable;

                    stickmanPoolable.colorType =
                        stickmanPoolable.cell.stickmanData.colorType;

                    stickmanIndex++;
                    if (towerControllersList.Count > 0)
                    {
                        for (var i = 0; i < towerControllersList.Count; i++)
                        {
                            stickmanPoolable.transform.position = new Vector3(
                                towerControllersList[i].transform.position.x,
                                1,
                                towerControllersList[i].transform.position.z);
                            MoveToCell(stickmanPoolable, stickmanPoolable.cell, true);
                        }
                    }
                    else
                    {
                        MoveToCell(stickmanPoolable, stickmanPoolable.cell, false);
                    }
                }
            }

            _spawnTime -= Time.deltaTime;

            if (Input.touchCount > 0)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began && !isLocked)
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit raycastHit;
                    if (Physics.Raycast(ray, out raycastHit))
                    {
                        if (raycastHit.collider.gameObject.GetComponent<Stickman.Stickman>())
                        {
                            var component = raycastHit.collider.gameObject.GetComponent<Stickman.Stickman>();
                            /*
                            var list = _targetCellManager.targetCells.Where(cell => cell.objectOn == null);*/
                            if (isTutorial && component != _gameManager.stickmanList[_stageIndex])
                            {
                                return;
                            }

                            if (component.isYupli && component.ropeCount >= 1)
                            {
                                return;
                            }

                            TargetCell targetCell = new TargetCell();
                            targetCell = SearchClosestTargetCell(component, targetCell);
                            if (targetCell != null)
                            {
                                if (component.isFree /*&& !isLocked*/ && component.targetCell == null)
                                {
                                    component.targetCell = targetCell;
                                    component.targetCell.objectOn = component;
                                    /*
                                                                        if (component.targetCell == null)
                                                                        {
                                                                            component.targetCell = targetCell;
                                                                            component.targetCell.objectOn = component;
                                                                        }*/
                                    component.cell.canSpawn = true;
                                    if (component.cell.towerController != null)
                                    {
                                        if (!component.cell.towerController.isAdded)
                                        {
                                            component.cell.towerController.isAdded = true;
                                            _enemySpawnManager.sequanceMaterial.Add(component.cell.towerController
                                                .towerData.sequenceMaterials);
                                        }
                                    }

                                    CheckNeighborObject(component.cell, component);
                                }
                                else
                                {
                                    if (!component.isFree)
                                    {
                                        component.angryEffect.Play();
                                    }
                                }
                            }
                            /*if (component.isFree)
                            {
                                var freeCellList = component.cell.freeCellOfNeighbor.Where(cell =>
                                    cell.objectOn == null && !cell.bannedStickman.Contains(component)).ToList();
                                if (freeCellList.Count > 0)
                                {
                                    for (var i = 0; i < freeCellList.Count; i++)
                                    {
                                        if (freeCellList[i].stickmanCellType == Cell.StickmanCellType.Forward)
                                        {
                                            component.pathToTarget.Add(freeCellList[i]);
                                            break;
                                        }
                                        var newFreeCellList = freeCellList[i].freeCellOfNeighbor.Where(cell => cell.objectOn == null && cell.)
                                    }
                                }
                            }*/
                        }
                    }
                }
            }
        }

        public TargetCell SearchClosestTargetCell(Stickman.Stickman targetStickman, TargetCell targetCell)
        {
            float closestDistance = float.MaxValue;
            var list = _targetCellManager.targetCells.Where(cell => cell.objectOn == null).ToList();
            for (int i = 0; i < list.Count; i++)
            {
                var distance = Vector3.Distance(list[i].transform.position,
                    targetStickman.transform.position);
                if (closestDistance > distance)
                {
                    closestDistance = distance;
                    targetCell = list[i];
                }
            }

            return targetCell;
        }

        public void KingOrderAnimation()
        {
            if (stickmanKingAnimator != null)
            {
                stickmanKingAnimator.SetTrigger("Order");
                stickmanKingAnimator.SetBool("Idle", false);
            }
        }

        public void KingIdleAnimation()
        {
            stickmanKingAnimator.SetBool("Idle", true);
        }

        public void ChangeBoxCollider(int index)
        {
            TutorialStage stage = TutorialManager.Instance.Tutorials[0].Stages[index];
            _stageIndex = stage.StageIndex;
        }


        private void CheckStickmanMovable(Stickman.Stickman stickmanTarget)
        {
            if (!stickmanTarget.isFree || stickmanTarget.cell == null)
                return;

            HandleForwardStickman(stickmanTarget);

            if (stickmanTarget.cell.stickmanCellType == Cell.StickmanCellType.Back)
            {
                HandleBackwardStickman(stickmanTarget);
            }
        }

        public void HandleForwardStickman(Stickman.Stickman stickmanTarget)
        {
            if (stickmanTarget.cell.stickmanCellType != Cell.StickmanCellType.Forward)
            {
                return;
            }

            if (_dataManager.isTutorial)
            {
                TutorialManager.Instance.StageCompleted(0, _stageIndex);
                Debug.Log(_stageIndex);
                tapToPlay.SetActive(false);
            }

            MoveStickmanToTargetCell(stickmanTarget);
        }

        public void HandleBackwardStickman(Stickman.Stickman stickmanTarget)
        {
            List<StickmanCell> availableCells = GetAvailableCells(stickmanTarget);
            if (availableCells.Count == 0)
            {
                stickmanTarget.IdleAnimation();
                stickmanTarget.isWaiting = true;
                return;
            }

            var freeList = availableCells.OrderByDescending(cell => cell.freeCellOfNeighbor.Count).ToList();
            StickmanCell closestCell = new StickmanCell();
            float minDistance = float.MaxValue;
            for (var i = 0; i < freeList.Count; i++)
            {
                float distance = Vector3.Distance(freeList[i].transform.position,
                    stickmanTarget.targetCell.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestCell = freeList[i];
                }

                if (freeList[i].stickmanCellType == Cell.StickmanCellType.Forward)
                {
                    closestCell = freeList[i];
                }
            }


            /*StickmanCell closestCell = new StickmanCell();
            Debug.Log(
                $"cell {availableCells.Count} cell name {stickmanTarget.cell.name} stickman {stickmanTarget.name}");
            closestCell = FindClosestCell(stickmanTarget, availableCells, closestCell);*/


            if (closestCell != null && closestCell.stickmanCellType == Cell.StickmanCellType.Back)
            {
                MoveStickmanToCell(stickmanTarget, closestCell);
            }
            else if (closestCell.stickmanCellType == Cell.StickmanCellType.Forward)
            {
                MoveStickmanToCell(stickmanTarget, closestCell);
            }
        }


        private void MoveStickmanToTargetCell(Stickman.Stickman stickmanTarget)
        {
            if (stickmanTarget.targetCell != null)
            {
                if (stickmanTarget.cell != null)
                {
                    stickmanTarget.cell.objectOn = null;
                    //_targetCellManager.CheckMatchesStickman();
                    SearchSameEnemy(stickmanTarget);
                    if (stickmanTarget._enemyController != null)
                    {
                        stickmanTarget.targetCell.objectOn = null;
                        stickmanTarget.targetCell._stickman = null;
                        stickmanTarget.targetCell = null;
                        return;
                    }

                    stickman.MoveAnimation(moveSpeed);
                    Move(stickmanTarget, stickmanTarget.targetCell);
                }
            }
        }

        public void MoveToCell(Stickman.Stickman _stickman, Cell cell, bool isSpawnedFromTower)
        {
            var position = new Vector3(cell.transform.position.x, cell.transform.position.y + 0.7f,
                cell.transform.position.z);
            Animator component = _stickman.GetComponent<Animator>();
            var isTied = _stickman.cell.stickmanData.isYuplenen
                ? true
                : false;
            float speed = !isSpawnedFromTower
                ? (stickmanMoveSpeed / 5) + stickmanMoveSpeed
                : Mathf.Abs((stickmanMoveSpeed / 5) - stickmanMoveSpeed);
            _stickman.transform.DOMove(position, speed).SetSpeedBased()
                .SetEase(Ease.Linear)
                .OnComplete((
                    () =>
                    {
                        _stickman.transform.DOLocalRotate(Vector3.zero, 0.2f).SetEase(Ease.Linear);
                        if (isTied)
                        {
                            _stickman.IdleRopeAnimation();
                        }
                        else
                        {
                            _stickman.IdleAnimation();
                        }

                        if (_stickman.isMystery)
                        {
                            _stickman.mysterySign.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                            _stickman.mysterySign.SetActive(true);
                            _stickman.mysterySign.transform.DOScale(2.2f, 0.2f).SetEase(Ease.Linear);
                            _stickman.mysterySign.transform.localRotation = Quaternion.Euler(new Vector3(60, 0, 0));
                            //_stickman.mysterySign.transform.LookAt(-Camera.main.transform.position);
                        }

                        if (_dataManager.currentLevel.isTutorial && !isTutorial)
                        {
                            isTutorial = true;
                            TutorialManager.Instance.StageStarted(0, 0);
                        }

                        if (cell.gameObject.GetComponent<StickmanCell>().stickmanCellType ==
                            Cell.StickmanCellType.Forward)
                        {
                            _stickman.cell.SearchNeighbors();
                        }
                        else if (cell.gameObject.GetComponent<StickmanCell>().stickmanCellType ==
                                 Cell.StickmanCellType.Back)
                        {
                            notCheckedStickmanCells.Add(_stickman.cell);
                        }

                        if (_stickman.startToCheckCells)
                        {
                            //KingIdleAnimation();
                            startToSpawnEnemies = true;
                            isLocked = false;
                            _stickman.startToCheckCells = false;
                            foreach (var stickmanCell in notCheckedStickmanCells)
                            {
                                stickmanCell.SearchNeighbors();
                            }
                        }
                    }));
            var direction = cell.transform.position -
                            _stickman.transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction);
            _stickman.transform.DORotateQuaternion(rotation, 0.3f);
            if (isTied)
            {
                _stickman.RunWithRopeAnimation();
            }
            else
            {
                _stickman.MoveAnimation(speed);
            }
        }

        private void Move(Stickman.Stickman stickman, Cell cell)
        {
            var position = new Vector3(cell.transform.position.x, cell.transform.position.y + 0.9f,
                cell.transform.position.z);

            stickman.MoveAnimation(moveSpeed);
            stickman.transform.DOMove(position, moveSpeed)
                .SetSpeedBased().OnComplete(() =>
                {
                    stickman.transform.DORotate(Vector3.zero, 0.2f).SetEase(Ease.Linear);
                    stickman.targetCell.isObjectOn = true;
                    if (cell.gameObject.GetComponent<TargetCell>())
                    {
                    }
                    else
                    {
                        CheckStickmanMovable(stickman);
                        for (int i = 0; i < cell.gameObject.GetComponent<StickmanCell>().stickmanCells.Count; i++)
                        {
                            Stickman.Stickman objectOn =
                                cell.gameObject.GetComponent<StickmanCell>().stickmanCells[i].objectOn;
                            if (objectOn != null)
                            {
                                if (objectOn.isWaiting)
                                {
                                    CheckStickmanMovable(objectOn);
                                }
                            }
                        }
                    }
                }).SetEase(Ease.Linear);
            var direction = cell.transform.position -
                            stickman.transform.position;
            Quaternion rotation = Quaternion.LookRotation(direction);
            stickman.transform.DORotateQuaternion(rotation, 0.3f);
        }

        public void SearchSameEnemy(Stickman.Stickman stickman)
        {
            if (_enemySpawnManager.currentEnemies.Count > 0 && stickman != null)
            {
                List<EnemyController> enemies = new List<EnemyController>();
                for (int i = 0; i < _enemySpawnManager.currentEnemies.Count; i++)
                {
                    if (_enemySpawnManager.currentEnemies[i] != null)
                    {
                        EnemyController enemyController = _enemySpawnManager.currentEnemies[i];
                        if (enemyController.targetObject == null)
                        {
                            if (enemyController.skinnedMeshRenderer.material.color ==
                                stickman.skinnedMeshRenderer.material.color && enemyController.isReadyForFight)
                            {
                                enemies.Add(enemyController);
                            }
                        }
                    }

                    if (_enemySpawnManager.currentEnemies.Count - 1 == i)
                    {
                        if (enemies.Count > 1)
                        {
                            float distance = float.MaxValue;
                            for (int j = 0; j < enemies.Count; j++)
                            {
                                var enemyDistance = Vector3.Distance(stickman.transform.position,
                                    enemies[j].transform.position);
                                if (enemyDistance < distance)
                                {
                                    distance = enemyDistance;
                                    stickman._enemyController = enemies[j];
                                }
                            }

                            if (stickman._enemyController != null)
                            {
                                GoToEnemy(stickman, stickman._enemyController);
                            }
                        }
                        else if (enemies.Count == 1)
                        {
                            var enemyController = enemies[0];
                            GoToEnemy(stickman, enemyController);
                        }
                    }
                }
            }
        }


        private void GoToEnemy(Stickman.Stickman stickmanObject, EnemyController enemyController)
        {
            stickmanObject.MoveAnimation(moveSpeed);

            enemyController.targetObject = stickmanObject;
            _enemySpawnManager.deadEnemyControllers.Add(enemyController);
            stickmanObject._enemyController = enemyController;
        }

        private static void RemoveNonNullableCellObjectOn(Cell cell)
        {
            StickmanCell component = cell.gameObject.GetComponent<StickmanCell>();
            for (int i = 0; i < component.stickmanCells.Count; i++)
            {
                if (component.stickmanCells[i] != null)
                {
                    for (int j = 0; j < component.stickmanCells[i].freeCellOfNeighbor.Count; j++)
                    {
                        if (component.stickmanCells[i].freeCellOfNeighbor.Contains(component))
                        {
                            component.stickmanCells[i].freeCellOfNeighbor.Remove(component);
                        }
                    }
                }
            }
        }

        private List<StickmanCell> GetAvailableCells(Stickman.Stickman stickman)
        {
            var availableCell = stickman.cell.stickmanCells
                .Where(cell => cell.objectOn == null && !cell.bannedStickman.Contains(stickman))
                .Distinct()
                .ToList();

            return availableCell;
        }


        private StickmanCell FindClosestCell(Stickman.Stickman stickman, List<StickmanCell> cells,
            StickmanCell closestCell)
        {
            float minDistance = float.MaxValue;
            foreach (var cell in cells)
            {
                float distance = Vector3.Distance(cell.transform.position, stickman.targetCell.transform.position);

                if (distance < minDistance)
                {
                    closestCell = cell;
                    minDistance = distance;
                }


                if (cell.stickmanCellType == Cell.StickmanCellType.Forward)
                {
                    closestCell = cell;
                    break;
                }
            }

            return closestCell;
        }

        private void MoveStickmanToCell(Stickman.Stickman stickman, StickmanCell cell)
        {
            stickman.cell.bannedStickman.Add(stickman);
            stickman.bannedStickmanCell.Add(stickman.cell);
            stickman.cell.objectOn = null;
            stickman.cell = null;
            stickman.cell = cell; /*
            stickman.cell.objectOn = stickman;*/
            Move(stickman, stickman.cell);
        }


        public void CheckTargetStickmen()
        {
            if (_enemySpawnManager.currentEnemies != null)
            {
                var list = _targetCellManager.targetCells.Where(cell => cell.objectOn != null && cell.isObjectOn);
                int max = Mathf.Max(list.Count(), _enemySpawnManager.currentEnemies.Count);
                bool isMathced = true;
                if (list.Count() == _targetCellManager.targetCells.Count)
                {
                    var equalStickman = _gameManager.CheckEqualsStickman();
                    if (equalStickman <= 0 && _enemySpawnManager.enemySpawnCount <= 0)
                    {
                        var enemies =
                            _enemySpawnManager.currentEnemies.Where(controller => controller.targetObject == null);
                        if (_enemySpawnManager.enemyCount > 0 && enemies.Count() <= 0)
                        {
                            return;
                        }

                        _uiManager.GameOver();
                    }
                }
            }
        }

        public void ChangeMaterialToOutlined(StickmanCell stickmanCell)
        {
            if (stickmanCell.objectOn != null)
            {
                stickmanCell.objectOn.skinnedMeshRenderer.sharedMaterial = stickmanCell.objectOn.bodyColor;
                stickmanCell.objectOn.mysterySign.SetActive(false);
            }
        }

        public void ChangeMaterialToOrigin(StickmanCell stickmanCell)
        {
            if (stickmanCell.objectOn != null)
            {
                if (!stickmanCell.objectOn.isFree)
                {
                    stickmanCell.objectOn.skinnedMeshRenderer.material = stickmanCell.objectOn.bodyColor;
                }
            }
        }

        private bool IsMathced(int i, IEnumerable<TargetCell> list, bool isMathced)
        {
            var enemyColor = _enemySpawnManager.currentEnemies[i].skinnedMeshRenderer.materials[1].color;
            var stickmanColor = list.ElementAt(i).objectOn.skinnedMeshRenderer.materials[1].color;
            if (enemyColor == stickmanColor)
            {
                isMathced = true;
            }

            return isMathced;
        }

        public void CheckNeighborObject(StickmanCell cell, Stickman.Stickman stickman)
        {
            for (int i = 0; i < cell.stickmanCells.Count; i++)
            {
                if (cell.stickmanCells[i].objectOn != null)
                {
                    if (!cell.stickmanCells[i].objectOn.isFree && !cell.stickmanCells[i].objectOn.isYupli)
                    {
                        if (cell.towerController != null)
                        {
                            if (cell.towerController.isSpawnFinished)
                            {
                                cell.stickmanCells[i].objectOn.isFree = true;
                                cell.stickmanCells[i].isFree = true;

                                if (cell.stickmanCells[i].objectOn.isMystery)
                                {
                                    cell.stickmanCells[i].objectOn.isMystery = false;
                                    mysteryStickmans.Remove(cell.stickmanCells[i].objectOn);
                                }

                                if (cell.stickmanCells[i].objectOn.exMystery)
                                {
                                    cell.stickmanCells[i].objectOn.bodyColor =
                                        cell.stickmanCells[i].objectOn.mysteryOutPutMaterial;
                                }

                                ChangeMaterialToOutlined(cell.stickmanCells[i]);
                                if (!cell.stickmanCells[i].freeCellOfNeighbor.Contains(cell))
                                {
                                    cell.stickmanCells[i].freeCellOfNeighbor.Add(cell);
                                }
                            }
                        }
                        else
                        {
                            cell.stickmanCells[i].objectOn.isFree = true;
                            cell.stickmanCells[i].isFree = true;

                            if (cell.stickmanCells[i].objectOn.isMystery)
                            {
                                cell.stickmanCells[i].objectOn.isMystery = false;
                                mysteryStickmans.Remove(cell.stickmanCells[i].objectOn);
                            }

                            if (cell.stickmanCells[i].objectOn.exMystery)
                            {
                                cell.stickmanCells[i].objectOn.bodyColor =
                                    cell.stickmanCells[i].objectOn.mysteryOutPutMaterial;
                            }

                            ChangeMaterialToOutlined(cell.stickmanCells[i]);
                            if (!cell.stickmanCells[i].freeCellOfNeighbor.Contains(cell))
                            {
                                cell.stickmanCells[i].freeCellOfNeighbor.Add(cell);
                            }
                        }
                    }
                    else
                    {
                        if (cell.stickmanCells[i].objectOn.ropeCount >= 1)
                        {
                            cell.stickmanCells[i].objectOn.ropeCount--;
                            int count = cell.stickmanCells[i].objectOn.ropeCount;
                            cell.stickmanCells[i].objectOn.UnTie(count);
                            if (cell.stickmanCells[i].objectOn.ropeCount <= 0 && cell.stickmanCells[i].objectOn.isYupli)
                            {
                                cell.stickmanCells[i].objectOn.isYupli = false;
                                cell.stickmanCells[i].objectOn.isFree = true;
                                cell.stickmanCells[i].isFree = true;
                                ChangeMaterialToOutlined(cell.stickmanCells[i]);
                                cell.stickmanCells[i].objectOn.RopeFreeAnimation();
                                /*
                                if (cell.stickmanCells[i].objectOn.isMystery)
                                {
                                    cell.stickmanCells[i].objectOn.skinnedMeshRenderer.material =
                                        cell.stickmanCells[i].objectOn.originMaterial;
                                }
                                */

                                if (!cell.stickmanCells[i].freeCellOfNeighbor.Contains(cell))
                                {
                                    cell.stickmanCells[i].freeCellOfNeighbor.Add(cell);
                                }
                            }
                        }
                    }
                }
                else if (cell.stickmanCells[i].objectOn == null)
                {
                    if (!cell.stickmanCells[i].freeCellOfNeighbor.Contains(cell))
                    {
                        cell.stickmanCells[i].freeCellOfNeighbor.Add(cell);
                    }
                }
            }

            CheckStickmanMovable(stickman);
        }

        public void AddFreeNeighbor(StickmanCell stickmanCell)
        {
            if (stickmanCell != null)
            {
                if (stickmanCell.objectOn == null)
                {
                    for (int i = 0; i < stickmanCell.stickmanCells.Count; i++)
                    {
                        if (stickmanCell.stickmanCells[i] != null)
                        {
                            if (!stickmanCell.stickmanCells[i].freeCellOfNeighbor.Contains(stickmanCell))
                            {
                                stickmanCell.stickmanCells[i].freeCellOfNeighbor.Add(stickmanCell);
                            }
                        }
                    }
                }
            }
        }
    }
}