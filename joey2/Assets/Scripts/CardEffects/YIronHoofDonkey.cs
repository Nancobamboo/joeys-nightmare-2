using System.Collections;
using UnityEngine;

public class YIronHoofDonkey : YCardEffect
{
    public YIronHoofDonkey()
    {
        Id = ECardEffectId.IronHoofDonkey;
        AddEffectValue(EEffectType.QuickAttack, 1);
    }
}

