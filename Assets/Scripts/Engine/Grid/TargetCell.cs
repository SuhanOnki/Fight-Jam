using System;
using System.Collections.Generic;
using DG.Tweening;
using Engine.Enemy;
using Engine.Manager;
using Engine.Player;
using TMPro;
using UnityEngine;

namespace Engine.Grid
{
    public class TargetCell : Cell
    {
        public TargetCellManager _targetCellManager;
        public EnemySpawnManager _enemySpawnManager;
        public GameObject locked;
        public PlayerInput _playerInput;
        public DataManager DataManager;
        public Stickman.Stickman _stickman;
        public bool _initialize;
        public int isFree;
        public TextMeshProUGUI priceText;
        public bool isObjectOn;
        public int price;
        public TargetCell NextTargetCell;

        public enum TargetCellType
        {
            Buyed,
            Default
        }

        private void Start()
        {
            if (priceText != null)
            {
                priceText.text = price.ToString();
            }

            _enemySpawnManager = EnemySpawnManager.enemySpawnManager;
            _targetCellManager = TargetCellManager.targetCellManager;
            if (!_targetCellManager.targetCells.Contains(this))
            {
                _targetCellManager.targetCells.Add(this);
            }

            _targetCellManager.SortByIndex();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponent<Stickman.Stickman>() && objectOn != null)
            {
                _stickman = other.gameObject.GetComponent<Stickman.Stickman>();
                if (_stickman == objectOn && _stickman._enemyController == null)
                {
                    _stickman.ActionPoseAnimation();
                    for (int i = 0; i < _enemySpawnManager.currentEnemies.Count; i++)
                    {
                        if (_enemySpawnManager.currentEnemies[i] != null &&
                            _enemySpawnManager.currentEnemies[i].targetObject == null)
                        {
                            SearchSameEnemy(_stickman);
                        }
                    }
                }
            }
        }

        public void OpenCell(int free)
        {
            if (free == 1)
            {
                Debug.Log(name);
                BuyTargetCell();
            }
            else
            {
                if (_playerInput.money >= price)
                {
                    BuyTargetCell();
                }
            }
        }

        private void BuyTargetCell()
        {
            enabled = true;

            Debug.Log(_targetCellManager);
            if (!_targetCellManager.targetCells.Contains(this))
            {
                _playerInput.targetMoney -= price;
                _targetCellManager.targetCells.Add(this);
            }

            isFree = 1;
            locked.SetActive(false);
            if (NextTargetCell != null)
            {
                DataManager.notBuyed.Add(NextTargetCell);
                NextTargetCell.gameObject.SetActive(true);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (_initialize)
            {
                if (other.gameObject.GetComponent<Stickman.Stickman>())
                {
                    _stickman = other.gameObject.GetComponent<Stickman.Stickman>();
                    if (_enemySpawnManager.currentEnemies.Count > 0)
                    {
                        if (_stickman != null && objectOn != null && _stickman._enemyController == null)
                        {
                            if (_stickman == objectOn)
                            {
                                _initialize = false;
                                SearchSameEnemy(objectOn);
                            }
                        }
                    }
                }
            }
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
                                var enemyDistance = Vector3.Distance(objectOn.transform.position,
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

        private void GoToEnemy(Stickman.Stickman stickman, EnemyController enemyController)
        {
            stickman.MoveAnimation(stickman.playerInput.moveSpeed);
            enemyController.targetObject = stickman;
            _enemySpawnManager.deadEnemyControllers.Add(enemyController);
            stickman._enemyController = enemyController;


            stickman.targetCell = null;
            objectOn = null;
            _stickman = null;
        }
    }
}