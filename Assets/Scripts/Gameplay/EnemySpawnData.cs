using System.Collections.Generic;
using Enums;
using UnityEngine;
using UnityEngine.AI;

namespace Gameplay {
    public static class EnemySpawnManager {
        public static readonly List<EnemySpawnData> EnemySpawnList = new();
    }

    [System.Serializable]
    public class EnemySpawnData {
        public string name;
        public GameObject enemyPrefab;
        public Vector3 spawnPosition;
        public Quaternion spawnRotation;
        public EnemyType enemyType;
    }
}