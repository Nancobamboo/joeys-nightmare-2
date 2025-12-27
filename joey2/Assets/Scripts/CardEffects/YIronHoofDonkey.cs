using System.Collections;
using UnityEngine;

public class YIronHoofDonkey : YDefaultEffect
{
    // 榴莲Donkey（反伤驴）：受击时反伤固定伤害（文案：反伤5点）
    private const int THORNS_DAMAGE = 5;

    public YIronHoofDonkey()
    {
        Id = ECardEffectId.IronHoofDonkey;

        // 注意：不要用 QuickAttack 来实现“反伤”。QuickAttack 会触发怪物按自身攻击力进行主动反击，
        // 与“反伤固定 5 点伤害”的设计不一致，并会导致手里剑等被动伤害看起来触发了怪物主动反击。
        AddEffectValue(EEffectType.ReflectDamage, THORNS_DAMAGE);
    }
}

