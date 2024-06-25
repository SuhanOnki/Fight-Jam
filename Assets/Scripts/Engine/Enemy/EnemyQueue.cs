using System;
using Engine.Grid;
using UnityEngine;

namespace Engine.Enemy
{
    public class EnemyQueue : Cell
    {
        private EnemySpawnManager _enemySpawnManager;

        private void Start()
        {
            _enemySpawnManager = EnemySpawnManager.enemySpawnManager;
        }
    }
}