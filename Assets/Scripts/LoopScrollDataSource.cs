using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
	// 基类
	// 用来定义数据的来源
    public abstract class LoopScrollDataSource
    {
        public abstract void ProvideData(Transform transform, int idx);
    }

	// 直接向条目，传递index
	public class LoopScrollSendIndexSource : LoopScrollDataSource
    {
		public static readonly LoopScrollSendIndexSource Instance = new LoopScrollSendIndexSource();

		LoopScrollSendIndexSource(){}

        public override void ProvideData(Transform transform, int idx)
        {
            transform.SendMessage("ScrollCellIndex", idx);
        }
    }

	// 会预先将数据保存在数组
	// 向条目传递数据，是根据index从数组中取出后的
	public class LoopScrollArraySource<T> : LoopScrollDataSource
    {
        T[] objectsToFill;

		public LoopScrollArraySource(T[] objectsToFill)
        {
            this.objectsToFill = objectsToFill;
        }

        public override void ProvideData(Transform transform, int idx)
        {
            transform.SendMessage("ScrollCellContent", objectsToFill[idx]);
        }
    }
}
