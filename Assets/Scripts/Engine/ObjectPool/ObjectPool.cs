using System;
using System.Collections.Generic;
using Engine.Enemy;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[Serializable]
public class ObjectPool
{
    private PoolableObject _prefab;
    private int _size;
    public List<PoolableObject> _availableObjectsPool;

    private ObjectPool(PoolableObject prefab, int size)
    {
        this._prefab = prefab;
        this._size = size;
        _availableObjectsPool = new List<PoolableObject>(size);
    }

    public static ObjectPool CreateInstance(PoolableObject prefab, int size, Transform counterTop,
        PoolableObject[] arrayList, bool isArray)
    {
        ObjectPool pool = new ObjectPool(prefab, size);

        pool.CreateObjects(counterTop.gameObject, counterTop, arrayList, prefab, isArray);

        return pool;
    }

    private void CreateObjects(GameObject parent, Transform spawnPos, PoolableObject[] arrayList,
        PoolableObject gameObject, bool isArray)
    {
        if (isArray)
        {
            for (int i = 0; i < _size; i++)
            {
                int random = Random.Range(0, arrayList.Length - 1);
                PoolableObject poolableObject =
                    GameObject.Instantiate(arrayList[random], spawnPos.position,
                        Quaternion.identity, parent.transform);
                poolableObject.Parent = this;
                poolableObject.transform.localPosition = Vector3.zero;
                poolableObject.gameObject.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < _size; i++)
            {
                PoolableObject poolableObject =
                    GameObject.Instantiate(gameObject, Vector3.zero, Quaternion.identity, parent.transform);
                poolableObject.Parent = this;
                poolableObject.transform.localPosition = Vector3.zero;
                poolableObject.gameObject.SetActive(false);
            }
        }
    }

    public PoolableObject GetObject()
    {
        if (_availableObjectsPool.Count >= 1)
        {
            PoolableObject instance = _availableObjectsPool[0];

            if (instance != null && !instance.gameObject.activeInHierarchy)
            {
                _availableObjectsPool.Remove(_availableObjectsPool[0]);
                instance.gameObject.SetActive(true);
            }

            return instance;
        }
        else
        {
            return null;
        }
    }

    public void ReturnObjectToPool(PoolableObject Object)
    {
        if (!_availableObjectsPool.Contains(Object))
        {
            _availableObjectsPool.Add(Object);
            Object.gameObject.SetActive(false);
            if (Object.gameObject.GetComponent<EnemyController>())
            {
                var enemy = Object.gameObject.GetComponent<EnemyController>();
                if (enemy != null)
                {
                    if (enemy.isDie)
                    {
                        enemy.enemyQueue = null;
                        enemy._enemySpawnManager._dataManager.gameManager.enemyControllers.Remove(enemy);
                        if (enemy._enemySpawnManager._dataManager.gameManager.enemyControllers.Count <= 0)
                        {
                            enemy._enemySpawnManager._dataManager.gameManager.uiManager.boosterButtons[1].gameObject
                                .GetComponent<Button>().interactable = true;
                        }

                        enemy.skinnedMeshRenderer.gameObject.SetActive(false);
                        enemy.enabled = false;
                    }
                }
            }
        }
    }
}