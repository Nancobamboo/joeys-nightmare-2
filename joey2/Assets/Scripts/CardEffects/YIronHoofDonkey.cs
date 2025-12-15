using System.Collections;
using UnityEngine;

public class YIronHoofDonkey : YDefaultEffect
{
    public YIronHoofDonkey()
    {
        Id = ECardEffectId.IronHoofDonkey;
        AddEffectValue(EEffectType.QuickAttack, 1);
    }
}

