using System;
using System.Collections.Generic;
using Engine.Grid;

namespace Utils
{
    [Serializable]
    public class TowerData
    {
        public int spawntargetCell;
        public List<ColorType> colorTypes;
        public bool isSetManual;
        public int positionIndex;
        public List<int> stickmanCellsOnTheForward;
        public int sequenceMaterials;
    }
}