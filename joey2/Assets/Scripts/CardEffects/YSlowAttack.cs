using System.Collections;
using UnityEngine;

public class YSlowAttack : YDefaultEffect
{
    public YSlowAttack()
    {
        Id = ECardEffectId.SlowAttack;
        AddEffectValue(EEffectType.SlowAttack, 1);
    }
}

