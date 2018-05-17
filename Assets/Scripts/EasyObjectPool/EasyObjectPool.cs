/* 
 * Unless otherwise licensed, this file cannot be copied or redistributed in any format without the explicit consent of the author.
 * (c) Preet Kamal Singh Minhas, http://marchingbytes.com
 * contact@marchingbytes.com
 */
// modified version by Kanglai Qian
using UnityEngine;
using System.Collections.Generic;

namespace SG
{
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    public class PoolObject : MonoBehaviour
    {
        public string poolName;

        //defines whether the object is waiting in pool or is in use
		// 标记是否在pool中，还是在使用中
        public bool isPooled;
    }

    public enum PoolInflationType
    {
        /// When a dynamic pool inflates, add one to the pool.
        INCREMENT,
        /// When a dynamic pool inflates, double the size of the pool
        DOUBLE
    }

	// 注：Pool不是MonoBehaviour
    class Pool
    {
        private Stack<PoolObject> availableObjStack = new Stack<PoolObject>();

        //the root obj for unused obj
        private GameObject rootObj;
        private PoolInflationType inflationType;
        private string poolName;
        private int objectsInUse = 0;

		// poolObjectPrefab: 池子原始prefab对象
		// poolName: 池子的名称，会创建名称为poolName+"Pool"对应的GameObject
		// rootPoolObj：新创建的pool的rootObj的父亲
        public Pool(string poolName, GameObject poolObjectPrefab, GameObject rootPoolObj, int initialCount, PoolInflationType type)
        {
            if (poolObjectPrefab == null)
            {
#if UNITY_EDITOR
                Debug.LogError("[ObjPoolManager] null pool object prefab !");
#endif
                return;
            }
            this.poolName = poolName;
            this.inflationType = type;
            this.rootObj = new GameObject(poolName + "Pool");
            this.rootObj.transform.SetParent(rootPoolObj.transform, false);

            // In case the origin one is Destroyed, we should keep at least one
			// 为了防止池子模板丢失
			// 至少会创建1个模板的实例
			// 并且添加PoolObject控件
            GameObject go = GameObject.Instantiate(poolObjectPrefab);
            PoolObject po = go.GetComponent<PoolObject>();
            if (po == null) {
                po = go.AddComponent<PoolObject>();
            }
            po.poolName = poolName;

			// 将创建出来的此实例
			// 添加到池子内
            AddObjectToPool(po);

            //populate the pool
            populatePool(Mathf.Max(initialCount, 1));
        }

        //o(1)
		// 将po添加到池子中,rootObj下面
		// 会设置PoolObject的名字为poolName
        private void AddObjectToPool(PoolObject po)
        {
            //add to pool
            po.gameObject.SetActive(false);
            po.gameObject.name = poolName;
            availableObjStack.Push(po);
            po.isPooled = true;
            //add to a root obj
            po.gameObject.transform.SetParent(rootObj.transform, false);
        }

		// 向池子中，存入多少新的对象的实例
        private void populatePool(int initialCount)
        {
            for (int index = 0; index < initialCount; index++)
            {
                PoolObject po = GameObject.Instantiate(availableObjStack.Peek());
                AddObjectToPool(po);
            }
        }

        //o(1)
		// 从pool中，取
		// autoActive: 从池中取出对象的时候，是否自动设置active
        public GameObject NextAvailableObject(bool autoActive)
        {
            PoolObject po = null;

			// 如果池子中有
            if (availableObjStack.Count > 1)
            {
                po = availableObjStack.Pop();
            }
            else
            {
				// 池子中没有
				// INCREMENT类型：不够的时候一个一个的增长
				// DOUBLE类型：将池子的容纳量*2
                int increaseSize = 0;
                //increment size var, this is for info purpose only
                if (inflationType == PoolInflationType.INCREMENT) {
                    increaseSize = 1;
                } else if (inflationType == PoolInflationType.DOUBLE) {
                    increaseSize = availableObjStack.Count + Mathf.Max(objectsInUse, 0);
                }

#if UNITY_EDITOR
                Debug.Log(string.Format("Growing pool {0}: {1} populated", poolName, increaseSize));
#endif
				// 创建新的对象
                if (increaseSize > 0)
                {
                    populatePool(increaseSize);
                    po = availableObjStack.Pop();
                }
            }

            GameObject result = null;
            if (po != null)
            {
                objectsInUse++;
                po.isPooled = false;
                result = po.gameObject;
                if (autoActive)
                {
                    result.SetActive(true);
                }
            }

            return result;
        }

        //o(1)
		// 放回pool中
        public void ReturnObjectToPool(PoolObject po)
        {
			// 检查是否是属于此池子
            if (poolName.Equals(po.poolName))
            {
                objectsInUse--;
                /* we could have used availableObjStack.Contains(po) to check if this object is in pool.
                 * While that would have been more robust, it would have made this method O(n) 
                 */
                if (po.isPooled)
                {
#if UNITY_EDITOR
                    Debug.LogWarning(po.gameObject.name + " is already in pool. Why are you trying to return it again? Check usage.");
#endif
                }
                else
                {
                    AddObjectToPool(po);
                }
            }
            else
            {
                Debug.LogError(string.Format("Trying to add object to incorrect pool {0} {1}", po.poolName, poolName));
            }
        }
    }
}
