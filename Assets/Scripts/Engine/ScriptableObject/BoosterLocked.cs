using UnityEngine;

namespace Engine.ScriptableObject
{
    [CreateAssetMenu()]
    public class BoosterLocked : UnityEngine.ScriptableObject
    {
        public int reqLevel;
        public string boosterName;
        public int boosterIndex;
    }
}
