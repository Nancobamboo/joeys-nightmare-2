using System.Collections.Generic;
using UnityEngine;

public static class UIGridHelper
{


	public static List<GameObject> GetCardsAsCardListOrder(Transform panel)
	{
		var list = new List<GameObject>(panel.childCount);
		for (int i = panel.childCount - 1; i >= 0; i--)
		{
			var go = panel.GetChild(i).gameObject;
			if (go.activeInHierarchy) list.Add(go);
		}
		return list;
	}

    public static GameObject GetCardListOrderIndex0(Transform panel)
    {
        // 视觉顺序反转后的第0个 -> 等价于“最后一个激活的视觉子物体”
        for (int i = panel.childCount - 1; i >= 0; i--)
        {
            var go = panel.GetChild(i).gameObject;
            if (go.activeInHierarchy) return go;
        }
        return null;
    }


}