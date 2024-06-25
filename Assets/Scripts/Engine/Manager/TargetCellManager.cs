using System;
using System.Collections.Generic;
using System.Linq;
using Engine.Enemy;
using Engine.Grid;
using UnityEngine;
using Utils;

namespace Engine.Manager
{
    public class TargetCellManager : MonoBehaviour
    {
        public List<TargetCell> targetCells;
        public static TargetCellManager targetCellManager;
        public EnemySpawnManager enemySpawnManager;

        private void Awake()
        {
            targetCellManager = this;
        }

        private void Start()
        {
        }

        public void CheckMatchesStickman()
        {
            List<int> stickmanTypes = new List<int>(6);
            for (var i = 0; i < targetCells.Count; i++)
            {
                if (targetCells[i].objectOn.colorType == ColorType.Green)
                {
                    stickmanTypes[0]++;
                }
                else if (targetCells[i].objectOn.colorType == ColorType.Pink)
                {
                    stickmanTypes[1]++;
                }
                else if (targetCells[i].objectOn.colorType == ColorType.Red)
                {
                    stickmanTypes[2]++;
                }
                else if (targetCells[i].objectOn.colorType == ColorType.Yellow)
                {
                    stickmanTypes[3]++;
                }
                else if (targetCells[i].objectOn.colorType == ColorType.OpenBlue)
                {
                    stickmanTypes[4]++;
                }
                else if (targetCells[i].objectOn.colorType == ColorType.BetweenYellowAndRed)
                {
                    stickmanTypes[5]++;
                }
            }
            for (var i = 0; i < stickmanTypes.Count; i++)
            {
                if (stickmanTypes[i] >= 3)
                {
                    switch (i)
                    {
                        case 0:
                            Debug.Log(ColorType.Green);
                            break;
                        case 1:
                            Debug.Log(ColorType.Pink);
                            break;
                        case 2:
                            Debug.Log(ColorType.Red);
                            break;
                        case 3:
                            Debug.Log(ColorType.Yellow);
                            break;
                        case 4:
                            Debug.Log(ColorType.OpenBlue);
                            break;
                        case 5:
                            Debug.Log(ColorType.BetweenYellowAndRed);
                            break;
                    }
                }
            }
        }

        public TargetCell SearchFreeCell()
        {
            var freeCellList = targetCells.Where(cell => cell.objectOn == null);
            var comfortableCellList = freeCellList.OrderBy(cell => cell.index);
            return comfortableCellList.ElementAt(0);
        }

        public void SortByIndex()
        {
            var list = targetCells.OrderBy(cell => cell.index);
            targetCells = list.ToList();
        }
    }
}