using System;
using System.Linq;
using Engine.Grid;
using Engine.Manager;
using QFSW.QC.Extras;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Engine.Enemy
{
    public class EnemyController : PoolableObject
    {
        public Animator _animator;
        public GameObject enemyQueue;
        public Vector3 targetPosition;
        public bool isAccelerationZone;
        public Stickman.Stickman targetObject;
        public ParticleSystem fallingEffect;
        public bool isMystery;
        public Rigidbody rigidbody;
        public bool isDie;
        public int randomDieAnimation;
        public Color originColor;
        public bool moving;
        public int randomPunch;
        public bool isSpeedDownBoosterUsed;

        public GameObject glow;
        public bool isReadyForFight;
        public float distance;
        public EnemySpawnManager _enemySpawnManager;
        private UIManager _uiManager;
        public float speed;
        private GameManager _gameManager;
        public SkinnedMeshRenderer skinnedMeshRenderer;

        private void Start()
        {
            _animator = GetComponent<Animator>();
            _gameManager = GameManager.gameManager;
            if (enemyQueue != null)
            {
                targetPosition = new Vector3(transform.position.x, enemyQueue.transform.position.y + 0.5f,
                    enemyQueue.transform.position.z + 5f);
            }

            _uiManager = UIManager.uiManager; /*
            int random = Random.Range(0, _gameManager.randomStickmans.Count);
            int randomV2 = Random.Range(0, _enemySpawnManager.colorIndexes.Count);*/
            if (_enemySpawnManager.tutorial)
            {
                _enemySpawnManager.tutorial = false;
                skinnedMeshRenderer.material =
                    _gameManager.stickmanList[0].skinnedMeshRenderer.material;
            } /*
            else
            {
                if (!isMystery &&
                    _enemySpawnManager.isSetManual[_enemySpawnManager.enemyIndex] == 0)
                {
                    skinnedMeshRenderer.material =
                        _gameManager.playerInput.outlinedMaterials[_gameManager.randomStickmans[random]];
                    _gameManager.randomStickmans.RemoveAt(random);
                }
                else if (!isMystery &&
                         _enemySpawnManager.isSetManual[_enemySpawnManager.enemyIndex] == 1)
                {
                    skinnedMeshRenderer.material =
                        _gameManager.playerInput.outlinedMaterials[
                            _enemySpawnManager.colorIndexes[randomV2]];
                    _enemySpawnManager.colorIndexes.RemoveAt(randomV2);
                }
                /*
                else
                {
                    skinnedMeshRenderer.material =
                        _gameManager.randomMaterials[_gameManager.randomStickmans[random]];
                }#1#
            }*/

            _enemySpawnManager.enemyIndex++;
        }

        private void Update()
        {
            if (enemyQueue != null)
            {
                if (!moving)
                {
                    moving = true;
                    MoveAnimation();
                }

                if (isAccelerationZone)
                {
                    speed = Mathf.Lerp(speed, _enemySpawnManager.enemySpeed, Time.deltaTime / 0.15f);
                }

                var animationSpeed = rigidbody.velocity.magnitude / speed;
                if (_gameManager.dataManager.isTutorial)
                {
                    transform.position =
                        Vector3.MoveTowards(transform.position, targetPosition,
                            Time.deltaTime * speed);
                    _animator.SetFloat("Speed", animationSpeed);
                }
                else
                {
                    transform.position =
                        Vector3.MoveTowards(transform.position, targetPosition,
                            Time.deltaTime * speed);
                    _animator.SetFloat("Speed", animationSpeed);
                }
            }
            else
            {
                if (_animator.GetBool("Walk"))
                {
                    IdleAnimation();
                }
            }
        }

        public void PlayParticle()
        {
            fallingEffect.Play();
            Die();
        }

        public void Die()
        {
            _enemySpawnManager.currentEnemies.Remove(this);
            glow.SetActive(false);
            skinnedMeshRenderer.gameObject.SetActive(false);
            if (targetObject != null)
            {
                targetObject.glow.SetActive(false);
                _enemySpawnManager._dataManager._playerInput.stickmanPooler.ReturnObjectToPool(targetObject);
            }
        }

        private void ReturnObject()
        {
            if (_enemySpawnManager.deadEnemyControllers.Count == 3)
            {
                _enemySpawnManager.sequanceMaterial.RemoveAt(_enemySpawnManager.waveindex);
                _enemySpawnManager.waveindex = Random.Range(0, _enemySpawnManager.sequanceMaterial.Count);
                _enemySpawnManager.enemySpawnCount = 3;
                _enemySpawnManager.canSpawn = true;
                _enemySpawnManager.deadEnemyControllers.Clear();
            }

            _enemySpawnManager.enemyPooler.ReturnObjectToPool(this);
        }

        private void CheckWinCondision()
        {
            if (_enemySpawnManager.enemyCount <= 0 && _enemySpawnManager.currentEnemies.Count <= 0)
            {
                _gameManager.uiManager.Win();
            }
        }

        public void DieAnimation()
        {
            for (int i = 0; i < _gameManager.availableStickmanCells.Count; i++)
            {
                _gameManager.availableStickmanCells[i].CheckBannedList(targetObject);
            }

            isDie = true;
            _animator.SetBool("Walk", false);
            _animator.SetBool("Idle", false);
            _animator.SetTrigger(_enemySpawnManager.dieAnimation[randomDieAnimation]);
        }

        public void TerminateParticle()
        {
            Invoke("ReturnObject", 0.1f);
            Invoke("CheckWinCondision", 0.1f);
        }


        public void MoveAnimation()
        {
            _animator.SetBool("Walk", true);
            _animator.SetBool("Idle", false);
        }

        public void IdleAnimation()
        {
            _animator.SetBool("Walk", false);
            _animator.SetBool("Idle", true);
        }

        public void PunchAnimation()
        {
            _animator.SetBool("Walk", false);
            _animator.SetBool("Idle", false);
            _animator.SetTrigger("PunchRight");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("WarZone"))
            {
                isReadyForFight = true;
                for (int i = 0; i < _enemySpawnManager._targetCellManager.targetCells.Count; i++)
                {
                    if (_enemySpawnManager._targetCellManager.targetCells[i].objectOn != null)
                    {
                        _enemySpawnManager._targetCellManager.targetCells[i]._initialize = true;
                    }
                }
            }

            if (other.gameObject.CompareTag("GameOverZone"))
            {
                GameOver();
                /*
                var list = _gameManager.stickmanList.Where(stickman => stickman._enemyController != null).ToList();
                for (var i = 0; i < list.Count; i++)
                {
                    list[i]._enemyController = null;
                    list[i].IdleAnimation();
                }*/

                enemyQueue = null;
            }

            if (other.gameObject.CompareTag("EnemyAccelerationZone"))
            {
                isAccelerationZone = true;
            }
        }

        public void GameOver()
        {
            _uiManager.GameOver();
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("EnemyAccelerationZone"))
            {
                isAccelerationZone = false;
            }
        }
    }
}