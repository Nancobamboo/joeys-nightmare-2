using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YApeWine : YCardEffect
{
    public YApeWine()
    {
        Id = ECardEffectId.ApeWine;
    }

    public override float UseItem()
    {
		YActionSystem.Instance.DispatchAction(EActionId.DoubleLastWeaponAttack, CardControl);
		return 0f;
    }
}
