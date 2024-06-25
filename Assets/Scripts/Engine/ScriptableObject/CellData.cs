using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Engine.ScriptableObject
{
    [CreateAssetMenu()]
    public class CellData : UnityEngine.ScriptableObject
    {
        public String cellName;
        public List<bool> history;
        public List<bool> stickmanMysteryTimes;
        public List<bool> isSetManual;
    }
}