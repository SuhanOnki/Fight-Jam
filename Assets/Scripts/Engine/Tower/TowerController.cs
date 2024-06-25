using System.Collections.Generic;
using System.Linq;
using Engine.Grid;
using Engine.Player;
using Engine.ScriptableObject;
using TMPro;
using UnityEngine;
using Utils;

namespace Engine.Tower
{
    public class TowerController : PoolableObject
    {
        public StickmanCell targetSpawnCell;
        public TextMeshProUGUI spawnCountTextMeshProUGUI;
        public TowerData towerData;
        public bool isAdded;
        public int spawnCount;
        public List<StickmanCell> _stickmanCellsOnTheForward = new List<StickmanCell>();
        public ObjectPool stickmanPooler;
        public bool isSpawnFinished;
        private PlayerInput _playerInput;
        private List<Material> outlineMaterials = new List<Material>();
        public List<ColorType> _colorTypes;

        void Start()
        {
            spawnCount = towerData.colorTypes.Count;
            _playerInput = PlayerInput.playerInput;
            stickmanPooler = _playerInput.stickmanPooler;
            spawnCount = towerData.colorTypes.Count;
            _colorTypes = towerData.colorTypes;
            spawnCountTextMeshProUGUI.text = spawnCount.ToString();
            InitializeStickmans();
            for (var i = 0; i < towerData.stickmanCellsOnTheForward.Count; i++)
            {
                for (var i1 = 0; i1 < _playerInput._dataManager.cells.Count; i1++)
                {
                    if (towerData.stickmanCellsOnTheForward[i] == _playerInput._dataManager.cells[i1].index &&
                        _playerInput._dataManager.cells[i1].enabled)
                    {
                        _stickmanCellsOnTheForward.Add(_playerInput._dataManager.cells[i1]);
                    }
                }
            }
        }

        void Update()
        {
            if (targetSpawnCell.objectOn == null && spawnCount >= 1 && targetSpawnCell.canSpawn)
            {
                spawnCount--;
                spawnCountTextMeshProUGUI.text = spawnCount.ToString();
                var stickman = stickmanPooler.GetObject();
                var stickmanObject = stickman.gameObject.GetComponent<Stickman.Stickman>();
                stickmanObject.isSpawnFromTower = true;
                stickmanObject.cell = targetSpawnCell;
                targetSpawnCell.objectOn = stickmanObject;
                var position1 = transform.position;
                Vector3 position = new Vector3(position1.x, position1.y - 10, position1.z);
                stickmanObject.transform.position = position;
                stickmanObject.transform.SetParent(_playerInput.stickmanParent.transform);
                if (_playerInput.randomPunch == 0)
                {
                    _playerInput.randomPunch = 1;
                }
                else if (_playerInput.randomPunch == 1)
                {
                    _playerInput.randomPunch = 0;
                }

                int randomStickman = 0;
                stickmanObject.skinnedMeshRenderer.sharedMaterial =
                    _playerInput.towerSpawnStickmans[randomStickman];
                _playerInput.towerSpawnStickmans.RemoveAt(randomStickman);
                stickmanObject.name = $"Stickman {_playerInput.stickmanIndex}";
                stickmanObject.playerInput = _playerInput;
                stickmanObject.playerInput.randomPunch = _playerInput.randomPunch;


                stickmanObject.bodyColor = outlineMaterials[randomStickman];
                outlineMaterials.RemoveAt(randomStickman);
                Debug.Log(stickmanObject.bodyColor.name);

                _playerInput.stickmanIndex++;
                if (targetSpawnCell.isFree)
                {
                    stickmanObject.isFree = targetSpawnCell.isFree;
                    _playerInput.ChangeMaterialToOutlined(stickmanObject.cell);
                }

                _playerInput.MoveToCell(stickmanObject, targetSpawnCell, true);
                if (spawnCount == 0)
                {
                    targetSpawnCell.canSpawn = false;
                    isSpawnFinished = true;
                }
            }
        }

        public void InitializeStickmans()
        {
            if (towerData.isSetManual)
            {
                for (var i = 0; i < towerData.colorTypes.Count; i++)
                {
                    _playerInput._dataManager.enemySpawnManager.colorIndexes.Add((int)towerData.colorTypes[i]);
                    _playerInput._dataManager.enemySpawnManager.isSetManual.Add(1);
                    _playerInput.towerSpawnStickmans.Add(
                        _playerInput._gameManager.randomMaterials[(int)towerData.colorTypes[i]]);
                }


                for (int i = 0; i < towerData.colorTypes.Count; i++)
                {
                    outlineMaterials.Add(_playerInput.outlinedMaterials[(int)towerData.colorTypes[i]]);
                }
            }
            else
            {
                _playerInput._dataManager.enemySpawnManager.isSetManual.Add(0);
            }
        }
    }
}