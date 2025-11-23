using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YDartScroll : YCardEffect
{
    public YDartScroll()
    {
        Id = ECardEffectId.DartScroll;
    }

    public override float UseItem()
    {
        int count = 3;
        while (count > 0)
        {
            Debug.Log("Add dart to env: " + count);
            YActionSystem.Instance.DispatchAction(EActionId.AddCardToEnv, CardControl, "1003");
            count--;
        }
		return 0f;
    }
}

