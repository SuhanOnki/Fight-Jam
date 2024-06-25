using System;
using System.Collections.Generic;

namespace Utils
{
    [Serializable]
    public class CellDataStore
    {
        public List<bool> history;
        public List<bool> stickmanMysteryTimes;
        public List<ColorType> colorTypes;
        public List<bool> isSetManual;
    }
}