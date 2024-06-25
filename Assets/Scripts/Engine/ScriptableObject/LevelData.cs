using System.Collections.Generic;
using Engine.Grid;
using Engine.Tower;
using UnityEngine;
using Utils;

namespace Engine.ScriptableObject
{
    [CreateAssetMenu()]
    public class LevelData : UnityEngine.ScriptableObject
    {
        public int levelIndex;
        public int stickmanCount;
        public int towerSpawnTime;
        public bool isTutorial;
        public float enenySpawnTime;
        public float enemySpeed;
        public LevelType levelType;
        public LevelDifficulty LevelDifficulty;
        public bool isSetManual;
        public List<int> sequanceOfMaterials;
        public List<StickmanData> stickmanDatas;
        public int envIndex;
        public List<TowerData> towerDatas;
    }

    public enum LevelType
    {
        Default,
        AllInOneColor,
        AllInOtherColor
    }

    public enum LevelDifficulty
    {
        Default,
        Boss
    }
}