using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Manager;
using Engine.ScriptableObject;
using Engine.Tower;
using Mono.CSharp;
using UnityEngine;
using Utils;

namespace Engine.Grid
{
    public class StickmanCell : Cell
    {
        public List<Vector3> _direction = new List<Vector3>();
        public List<Stickman.Stickman> bannedStickman = new List<Stickman.Stickman>();
        public List<StickmanCell> stickmanCells = new List<StickmanCell>();
        public StickmanCellType stickmanCellType;
        public StickmanCell forwardCell;
        public bool isFree;
        public bool canSpawn;
        public List<bool> spawnHistory;
        public TowerController towerController;

        public BoxCollider sec;
        public bool readyForInitialize;
        public bool isEnable;
        public DataManager _dataManager;
        public StickmanData stickmanData;
        public List<int> pathCount = new List<int>();
        public List<bool> mysteryHistory;

        public CellData cellData;
        public List<StickmanCell> freeCellOfNeighbor = new List<StickmanCell>();


        private void Start()
        {
            _dataManager = DataManager.dataManager;
            index = Convert.ToInt32(name);
            _direction.Add(Vector3.right);
            _direction.Add(-Vector3.right);
            _direction.Add(Vector3.forward);
            _direction.Add(-Vector3.forward);
        }

        public void SearchNeighbors()
        {
            for (int i = 0; i < _direction.Count; i++)
            {
                Ray ray = new Ray(transform.position, _direction[i]);
                RaycastHit raycastHit;

                if (Physics.Raycast(ray, out raycastHit))
                {
                    if (raycastHit.collider.gameObject.CompareTag("StickmanCell"))
                    {
                        StickmanCell stickmanCell = raycastHit.transform.gameObject.GetComponent<StickmanCell>();
                        if (stickmanCell != this && !stickmanCells.Contains(stickmanCell))
                        {
                            if (i == 2)
                            {
                                forwardCell = stickmanCell;
                            }

                            stickmanCells.Add(stickmanCell);
                        }
                    }
                }
                else
                {
                    if (i == 2 && stickmanCellType == StickmanCellType.Forward ||
                        i == 0 && stickmanCellType == StickmanCellType.Forward ||
                        i == 1 && stickmanCellType == StickmanCellType.Forward)
                    {
                        if (objectOn != null)
                        {
                            objectOn.isFree = true;
                            isFree = true;

                            objectOn.isReadyForMove = true;
                        }
                    }
                }

                if (i == _direction.Count - 1)
                {
                    CheckSide();
                }
            }
        }

        private void Update()
        {
            if (readyForInitialize)
            {
                readyForInitialize = false;
                SearchNeighbors();
            }
        }

        public void CheckBannedList(Stickman.Stickman stickman)
        {
            if (bannedStickman.Count > 0)
            {
                var miss = bannedStickman.Where(stickman => stickman == null);
                if (miss.Count() > 0)
                {
                    for (int i = 0; i < miss.Count(); i++)
                    {
                        bannedStickman.Remove(miss.ElementAt(i));
                    }
                }

                if (bannedStickman.Contains(stickman))
                {
                    bannedStickman.Remove(stickman);
                }
            }
        }

        public void CheckSide()
        {
            if (stickmanCells.Count > 0)
            {
                if (forwardCell != null)
                {
                    stickmanCellType = StickmanCellType.Back;
                }
                else if (forwardCell == null)
                {
                    stickmanCellType = StickmanCellType.Forward;
                }

                for (int j = 0; j < stickmanCells.Count; j++)
                {
                    if (stickmanCells[j].objectOn == null || stickmanCellType == StickmanCellType.Forward)
                    {
                        if (objectOn != null)
                        {
                            objectOn.isFree = true;
                            isFree = true;
                            objectOn.isReadyForMove = true;
                            objectOn.playerInput.ChangeMaterialToOutlined(this);
                        }
                    }
                    else
                    {
                        if (objectOn != null && stickmanCellType == StickmanCellType.Back)
                        {
                            objectOn.isFree = false;
                            isFree = false;
                        }
                    }

                    if (stickmanCells.Count - 1 == j)
                    {
                        _dataManager.transform.position = new Vector3(
                            transform.position.x + _dataManager.stickmanCellOffset,
                            transform.position.y, transform.position.z);
                    }
                }
            }
            else if (stickmanCells.Count == 0)
            {
                stickmanCellType = StickmanCellType.Forward;
            }
        }

        public void SwitchColor(Stickman.Stickman stickman)
        {
            if (stickmanData.colorType == ColorType.None)
            {
                return;
            }

            for (int i = 0; i < _dataManager.gameManager.randomMaterials.Count; i++)
            {
                if ((int)objectOn.colorType == i)
                {
                    stickman.skinnedMeshRenderer.sharedMaterial = _dataManager.gameManager.randomMaterials[i];

                    stickman.bodyColor = stickman.playerInput
                        .outlinedMaterials[i];
                    _dataManager.enemySpawnManager.colorIndexes.Add(i);
                    break;
                }
            }
        }
    }
}