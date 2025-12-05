using System.Collections.Generic;
using UnityEngine;

public class YPolishStupidDonkey : YDefaultEffect
{
    public YPolishStupidDonkey()
    {
        Id = ECardEffectId.PolishStupidDonkey;
    }

    public override float OnDead()
    {
        if (CardControl != null)
        {
            DataSystem.Instance.AddCoin(10);
        }
        return base.OnDead();
    }
}

