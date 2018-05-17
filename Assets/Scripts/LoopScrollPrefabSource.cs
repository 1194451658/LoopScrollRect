using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
    [System.Serializable]

	// 指定go的pool来源，
	// 和pool操作
    public class LoopScrollPrefabSource 
    {
        public string prefabName;
        public int poolSize = 5;

        private bool inited = false;

		// 初始化和从pool中，取go
        public virtual GameObject GetObject()
        {
            if(!inited)
            {
                SG.ResourceManager.Instance.InitPool(prefabName, poolSize);
                inited = true;
            }
            return SG.ResourceManager.Instance.GetObjectFromPool(prefabName);
        }

		// go返回到pool中
		// 返回到pool中，还会调用ScrollCellReturn消息
        public virtual void ReturnObject(Transform go)
        {
            go.SendMessage("ScrollCellReturn", SendMessageOptions.DontRequireReceiver);
            SG.ResourceManager.Instance.ReturnObjectToPool(go.gameObject);
        }
    }
}
