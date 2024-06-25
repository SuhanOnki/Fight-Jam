using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Grid;
using Engine.Manager;
using Engine.Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Engine.Enemy
{
    public class EnemySpawnManager : MonoBehaviour
    {
        public static EnemySpawnManager enemySpawnManager;
        public List<Transform> spawnPoint;
        public int enemyCount;
        public PoolableObject enemyPrefab;
        public GameObject targetObject;
        public int enemySpawnCount;
        public List<EnemyController> deadEnemyControllers = new List<EnemyController>();
        public float timer;
        public List<EnemyController> currentEnemies;
        public int enemyIndex;
        private GameObject _enemyParent;
        public List<String> dieAnimation = new List<string>();
        public ObjectPool enemyPooler;
        private PlayerInput _playerInput;
        public bool canSpawn;
        public List<int> isSetManual;
        public List<int> colorIndexes;
        public List<int> sequanceMaterial = new List<int>();
        public int waveindex;
        public DataManager _dataManager;
        private int _randomPunch;
        public bool tutorial;
        public TargetCellManager _targetCellManager;
        public int spawnPointIndex;
        public float _timer;
        public float enemySpeed;
        public GameManager gameManager;

        private void Awake()
        {
            enemySpawnManager = this;
        }

        void Start()
        {
            _dataManager = DataManager.dataManager;
            _targetCellManager = TargetCellManager.targetCellManager;
            sequanceMaterial = _dataManager.sequenceMaterials.ToList();
            timer = _dataManager.enemySpawnTime;
            _timer = timer;
            _playerInput = PlayerInput.playerInput;
            _enemyParent = new GameObject();
            _enemyParent.name = $"Enemy Parent";
            enemyCount = _dataManager.stickmanCount + _dataManager.currentLevel.towerSpawnTime;
            enemyPooler = ObjectPool.CreateInstance(enemyPrefab,
                _dataManager.stickmanCount + _dataManager.currentLevel.towerSpawnTime,
                spawnPoint[0].transform,
                null, false);
            enemySpeed = _dataManager.currentLevel.enemySpeed;
            waveindex = Random.Range(0, sequanceMaterial.Count);
            if (!tutorial && _dataManager.isTutorial)
            {
                tutorial = true;
            }
        }

        void Update()
        {
            if (_timer <= 0 && enemyCount > 0 && _playerInput.startToSpawnEnemies && enemySpawnCount > 0 && canSpawn &&
                !_dataManager.isTutorial)
            {
                _timer = timer;
                if (enemySpawnCount == 0)
                {
                    canSpawn = false;
                }

                enemyCount--;
                enemySpawnCount--;
                var obj = enemyPooler.GetObject();
                if (obj.gameObject.GetComponent<EnemyController>())
                {
                    var enemy = obj.gameObject.GetComponent<EnemyController>();
                    enemy.enemyQueue = targetObject;
                    enemy._enemySpawnManager = this;
                    enemy.speed = enemySpeed / 1.5f;
                    if (spawnPointIndex >= spawnPoint.Count)
                    {
                        spawnPointIndex = 0;
                    }

                    enemy.transform.SetParent(spawnPoint[spawnPointIndex]);

                    int randomPosition = Random.Range(0, _targetCellManager.targetCells.Count);
                    enemy.randomDieAnimation = Random.Range(0, dieAnimation.Count);
                    enemy.transform.localPosition = Vector3.zero;
                    enemy.rigidbody = enemy.gameObject.GetComponent<Rigidbody>();
                    enemy.transform.position =
                        new Vector3(spawnPoint[spawnPointIndex].transform.position.x,
                            enemy.transform.position.y, enemy.transform.position.z);
                    currentEnemies.Add(enemy);
                    enemy.enemyQueue = _targetCellManager.targetCells[spawnPointIndex].gameObject;
                    spawnPointIndex++;
                    if (!tutorial)
                    {
                        enemy.skinnedMeshRenderer.material =
                            gameManager.playerInput.outlinedMaterials[sequanceMaterial[waveindex]];
                    }

                    enemy.transform.localRotation = Quaternion.Euler(180, 0, 180);
                    enemy.transform.SetParent(_enemyParent.transform);
                }
            }


            _timer -= Time.deltaTime;
        }
    }
}