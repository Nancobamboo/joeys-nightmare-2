using System.Collections;
using UnityEngine;

public class YNoCounterAttack : YDefaultEffect
{
    public YNoCounterAttack()
    {
        Id = ECardEffectId.NoCounterAttack;
        AddEffectValue(EEffectType.NoCounterAttack, 1);
    }
}

