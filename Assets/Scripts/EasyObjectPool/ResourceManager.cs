using UnityEngine;
using System.Collections.Generic;
using System.Linq;


namespace SG
{
    [DisallowMultipleComponent]
    [AddComponentMenu("")]

	// 实际上是用来管理多个Pool的
    public class ResourceManager : MonoBehaviour
    {
        //obj pool
        private Dictionary<string, Pool> poolDict = new Dictionary<string, Pool>();

        private static ResourceManager mInstance = null;

		// 创建GameObject，ResourceManager，并挂载脚本
        public static ResourceManager Instance
        {
            get
            {
                if (mInstance == null)
                {
                    GameObject GO = new GameObject("ResourceManager", typeof(ResourceManager));
                    // Kanglai: if we have `GO.hideFlags |= HideFlags.DontSave;`, we will encounter Destroy problem when exit playing
                    // However we should keep using this in Play mode only!
                    mInstance = GO.GetComponent<ResourceManager>();
                    if (Application.isPlaying)
                    {
                        DontDestroyOnLoad(mInstance.gameObject);
                    }
                    else
                    {
                        Debug.LogWarning("[ResourceManager] You'd better ignore ResourceManager in Editor mode");
                    }
                }

                return mInstance;
            }
        }

		// poolName：池子名称，同时也是要缓存的go的名称
		// 新建的pool，会挂载在自己的下面
        public void InitPool(string poolName, int size, PoolInflationType type = PoolInflationType.DOUBLE)
        {
            if (poolDict.ContainsKey(poolName)) {
                return;
            }
            else {
				// 加载要缓存的go模板
                GameObject pb = Resources.Load<GameObject>(poolName);
                if (pb == null)
                {
                    Debug.LogError("[ResourceManager] Invalide prefab name for pooling :" + poolName);
                    return;
                }
				// 参数顺序；
				// poolName
				// poolObjectPrefab
				// rootPoolObj
				// initialCount
				// type
                poolDict[poolName] = new Pool(poolName, pb, gameObject, size, type);
            }
        }

        /// <summary>
        /// Returns an available object from the pool 
        /// OR null in case the pool does not have any object available & can grow size is false.
        /// </summary>
        /// <param name="poolName"></param>
        /// <returns></returns>

		// 从pool中取go
        public GameObject GetObjectFromPool(string poolName, bool autoActive = true, int autoCreate = 0)
        {
            GameObject result = null;

            if (!poolDict.ContainsKey(poolName) && autoCreate > 0)
            {
                InitPool(poolName, autoCreate, PoolInflationType.INCREMENT);
            }

            if (poolDict.ContainsKey(poolName))
            {
                Pool pool = poolDict[poolName];
                result = pool.NextAvailableObject(autoActive);
                //scenario when no available object is found in pool
#if UNITY_EDITOR
                if (result == null)
                {
                    Debug.LogWarning("[ResourceManager]:No object available in " + poolName);
                }
#endif
            }
#if UNITY_EDITOR
            else
            {
                Debug.LogError("[ResourceManager]:Invalid pool name specified: " + poolName);
            }
#endif
            return result;
        }

        /// <summary>
        /// Return obj to the pool
        /// </summary>
        /// <param name="go"></param>
		// 将go放回到pool中
        public void ReturnObjectToPool(GameObject go)
        {
            PoolObject po = go.GetComponent<PoolObject>();
            if (po == null)
            {
#if UNITY_EDITOR
                Debug.LogWarning("Specified object is not a pooled instance: " + go.name);
#endif
            }
            else
            {
                Pool pool = null;
                if (poolDict.TryGetValue(po.poolName, out pool))
                {
                    pool.ReturnObjectToPool(po);
                }
#if UNITY_EDITOR
                else
                {
                    Debug.LogWarning("No pool available with name: " + po.poolName);
                }
#endif
            }
        }

        /// <summary>
        /// Return obj to the pool
        /// </summary>
        /// <param name="t"></param>

		// 同ReturnObjectToPool，
		// 区分了，Transform SetParent onEnable的情况
        public void ReturnTransformToPool(Transform t)
        {
            if (t == null)
            {
#if UNITY_EDITOR
                Debug.LogError("[ResourceManager] try to return a null transform to pool!");
#endif
                return;
            }
            //set gameobject active flase to avoid a onEnable call when set parent
            t.gameObject.SetActive(false);
            t.SetParent(null, false);
            ReturnObjectToPool(t.gameObject);
        }
    }
}
