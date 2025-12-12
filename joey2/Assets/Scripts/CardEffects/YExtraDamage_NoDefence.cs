// Scripts/CardEffects/Effects/YExtraDamage_NoDefence.cs
using System.Collections;
using UnityEngine;

public class YExtraDamage_NoDefence : YDefaultEffect
{
    public int baseExtra;

    public YExtraDamage_NoDefence(int baseExtra)
    {
        this.baseExtra = Mathf.Max(0, baseExtra);
        Id = ECardEffectId.ExtraDamage_NoDefence;
    }
}

