using System;
using System.Collections.Generic;
using DG.Tweening;
using Engine.Enemy;
using Engine.Grid;
using Engine.Manager;
using Engine.Player;
using Engine.Tower;
using QFSW.QC.Actions;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace Engine.Stickman
{
    public class Stickman : PoolableObject
    {
        public GameManager _gameManager;
        public Animator _animator;
        public int index;
        public int randomPunch;
        public SkinnedMeshRenderer skinnedMeshRenderer;
        public StickmanCell cell;
        public bool isMystery;
        public bool isReadyForMove;
        public PlayerInput playerInput;
        public bool isOnPosition;
        public List<StickmanCell> pathToTarget = new List<StickmanCell>();
        public bool isWaiting;
        public ParticleSystem fightLight;
        public Rigidbody _rigidbody;
        public bool isSpawnFromTower;
        public ParticleSystem angryEffect;
        public bool exMystery;
        public ColorType colorType;
        public bool isFree;
        public EnemyController _enemyController;
        public bool startToCheckCells;
        public Material mysteryOutPutMaterial;
        public List<StickmanCell> bannedStickmanCell;
        public Vector3 target;
        public GameObject glow;

        public Material bodyColor;
        public int ropeCount;
        public TargetCell targetCell;
        public bool isYupli;
        public Material originMaterial;
        public GameObject mysterySign;
        public GameObject[] rope;

        void Start()
        {
            _animator = GetComponent<Animator>();
            _gameManager = GameManager.gameManager;
            playerInput = PlayerInput.playerInput;
            _gameManager.stickmanList.Add(this);
            _gameManager.StickmanIndexSortByIndex();
            isMystery = cell.stickmanData.isMystery;
            exMystery = isMystery;
            isYupli = cell.stickmanData.isYuplenen;

            if (isMystery)
            {
                playerInput.mysteryStickmans.Add(this);
            }

            if (isYupli)
            {
                ropeCount = 2;
                TieStickman();
            }

            if (isSpawnFromTower)
            {
                return;
            }

            Initialize();
        }

        public void Initialize()
        {
            if (cell._dataManager.currentLevel.isSetManual && !isMystery)
            {
                originMaterial = skinnedMeshRenderer.material;
                bodyColor = playerInput
                    .outlinedMaterials[(int)cell.stickmanData.colorType];
                skinnedMeshRenderer.sharedMaterial = _gameManager.randomMaterials[(int)cell.stickmanData.colorType];
                playerInput._dataManager.enemySpawnManager.colorIndexes.Add((int)cell.stickmanData.colorType);
                playerInput._dataManager.enemySpawnManager.isSetManual.Add(1);
            }
            else if (cell._dataManager.currentLevel.isSetManual && isMystery)
            {
                originMaterial = skinnedMeshRenderer.material;
                bodyColor = _gameManager.randomMaterials[(int)cell.stickmanData.colorType];
                mysteryOutPutMaterial = playerInput.outlinedMaterials[(int)cell.stickmanData.colorType];
                skinnedMeshRenderer.sharedMaterial = playerInput.mysteryMaterial;
                playerInput._dataManager.enemySpawnManager.colorIndexes.Add((int)cell.stickmanData.colorType);
                playerInput._dataManager.enemySpawnManager.isSetManual.Add(1);
            }
            else
            {
                if (!cell._dataManager.currentLevel.isSetManual && !isMystery)
                {
                    playerInput._dataManager.enemySpawnManager.isSetManual.Add(0);
                    skinnedMeshRenderer.sharedMaterial =
                        _gameManager.randomMaterials[_gameManager.randomStickmans[_gameManager.randomStickmanIndex]];
                    bodyColor = playerInput
                        .outlinedMaterials[_gameManager.randomStickmans[_gameManager.randomStickmanIndex]];
                }
                else if (!cell._dataManager.currentLevel.isSetManual && isMystery)
                {
                    originMaterial = skinnedMeshRenderer.material;
                    originMaterial.color = _gameManager
                        .randomMaterials[_gameManager.randomStickmans[_gameManager.randomStickmanIndex]].color;
                    playerInput._dataManager.enemySpawnManager.isSetManual.Add(0);
                    skinnedMeshRenderer.material =
                        playerInput.mysteryMaterial;
                    bodyColor = playerInput
                        .outlinedMaterials[_gameManager.randomStickmans[_gameManager.randomStickmanIndex]];
                }
            }

            _gameManager.randomStickmanIndex++;
        }

        private void TieStickman()
        {
            for (var i = 0; i < rope.Length; i++)
            {
                rope[i].SetActive(true);
            }
        }

        public void UnTie(int i)
        {
            rope[i].SetActive(false);
        }

        public void ChangeTargetRotation()
        {
            if (_enemyController != null)
            {
                target = _enemyController.transform.position;
            }
        }

        private void Update()
        {
            if (_enemyController != null)
            {
                var position1 = transform.position;
                var enemyPosition = new Vector3(_enemyController.transform.position.x,
                    _enemyController.transform.position.y, _enemyController.transform.position.z - 7.5f);
                var direction = enemyPosition -
                                position1;
                if (randomPunch == 2 || randomPunch == 4)
                {
                    enemyPosition.z -= 1.5f;
                }

                if (randomPunch == 7 || randomPunch == 8 || randomPunch == 9)
                {
                    enemyPosition.z -= 2.5f;
                }

                Quaternion rotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation,
                    Time.deltaTime * 5);
                transform.position = Vector3.MoveTowards(transform.position, enemyPosition,
                    Time.deltaTime * playerInput.moveSpeed);
            }
        }

        public void MoveAnimation(float moveSpeed)
        {
            var speed = _rigidbody.velocity.magnitude / moveSpeed;
            var animator = GetComponent<Animator>();
            animator.SetFloat("Speed", speed);
            animator.SetBool("Run", true);
            animator.SetBool("Idle", false);
            animator.SetBool("ActionPose", false);
            animator.SetBool("IdleRope", false);
            animator.SetBool("RunWithRope", false);
        }

        public void RunWithRopeAnimation()
        {
            var speed = _rigidbody.velocity.magnitude / playerInput.stickmanMoveSpeed;
            var animator = GetComponent<Animator>();
            animator.SetFloat("Speed", speed);
            animator.SetBool("RunWithRope", true);
            animator.SetBool("Run", false);
            animator.SetBool("Idle", false);
            animator.SetBool("ActionPose", false);
        }

        public void IdleAnimation()
        {
            _animator.SetBool("Run", false);
            _animator.SetBool("Idle", true);
            _animator.SetBool("ActionPose", false);
            _animator.SetBool("IdleRope", false);
            _animator.SetBool("RunWithRope", false);
        }

        public void RopeFreeAnimation()
        {
            _animator.SetBool("Run", false);
            _animator.SetBool("Idle", false);
            _animator.SetBool("ActionPose", false);
            _animator.SetBool("IdleRope", false);
            _animator.SetBool("RunWithRope", false);
            _animator.SetTrigger("RopeFree");
        }

        public void PunchLeft()
        {
            _animator.SetBool("Run", false);
            _animator.SetBool("Idle", false);
            _animator.SetBool("ActionPose", false);
            _animator.SetBool("IdleRope", false);
            _animator.SetBool("RunWithRope", false);
            _animator.SetTrigger("PunchLeft");
        }

        public void PunchRight()
        {
            _animator.SetBool("Run", false);
            _animator.SetBool("Idle", false);
            _animator.SetBool("ActionPose", false);
            _animator.SetBool("IdleRope", false);
            _animator.SetBool("RunWithRope", false);
            _animator.SetTrigger("PunchRight");
        }

        public void IdleRopeAnimation()
        {
            _animator.SetBool("Run", false);
            _animator.SetBool("RunWithRope", false);
            _animator.SetBool("Idle", false);
            _animator.SetBool("IdleRope", true);
            _animator.SetBool("ActionPose", false);
        }

        public void ActionPoseAnimation()
        {
            _animator.SetBool("Run", false);
            _animator.SetBool("Idle", false);
            _animator.SetBool("ActionPose", true);
        }

        public void PunchAnimation()
        {
            GetComponent<BoxCollider>().enabled = false;

            _animator.SetBool("Run", false);
            _animator.SetBool("Idle", false);
            _animator.SetBool("ActionPose", false);

            _animator.SetTrigger(playerInput.attackingAnimationName[randomPunch]);
        }

        public void SplashEffect()
        {
            fightLight.Play();
        }

        public void PunchLeftAction()
        {
            _animator.SetBool("Run", false);
            _animator.SetBool("Idle", false);
            _animator.SetBool("ActionPose", false);
            _animator.SetTrigger("PunchLeftAction");
        }

        public void PunchRightAction()
        {
            _animator.SetBool("Run", false);
            _animator.SetBool("Idle", false);
            _animator.SetBool("ActionPose", false);
            _animator.SetTrigger("PunchRightAction");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.GetComponent<EnemyController>())
            {
                var enemy = other.gameObject.GetComponent<EnemyController>();
                if (enemy == _enemyController)
                {
                    enemy.enemyQueue = null;

                    enemy.gameObject.GetComponent<BoxCollider>().enabled = false;
                    enemy.IdleAnimation();
                    transform.DOKill();

                    PunchAnimation();
                }
            }
        }

        public void EnemyDie()
        {
            if (_enemyController != null)
            {
                _enemyController.originColor = _enemyController.skinnedMeshRenderer.material.color;
                //_enemyController.skinnedMeshRenderer.material.DOColor(Color.white, 0.05f).SetEase(Ease.InFlash);
                _enemyController.skinnedMeshRenderer.material.color = Color.white;
                _enemyController.DieAnimation();
                SplashEffect();
                Invoke("SmootlyChangeToOriginColor",0.2f);

            }

            _gameManager.stickmanList.Remove(this);
        }

        private void SmootlyChangeToOriginColor()
        {
            _enemyController.skinnedMeshRenderer.material.DOColor(_enemyController.originColor,0.2f).SetEase(Ease.OutFlash);
        }
    }
}