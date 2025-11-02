// Scripts/CardEffects/CardEffectRegistry.cs
using System;
using System.Collections.Generic;

public static class CardEffectRegistry
{
    public static Dictionary<string, Func<ICardEffect>> factory = new()
    {
        { "DealRandomEnemyEqualToAttack_OnTop", () => new DealRandomEnemyEqualToAttack_OnTop() },
        { "HookEquipWeaponFromDiscard_OnPlay", () => new HookEquipWeaponFromDiscard_OnPlay() },
        { "DoubleAttack_OnPlay", () => new DoubleAttack_OnPlay() }
    };

    // 新增：解析带参数的 token，例如 "ExtraDamage_OnDealDamage:3"
    public static ICardEffect CreateWithArgs(string token)
    {
        if (string.IsNullOrEmpty(token)) return null;
        var parts = token.Split(':');
        var id = parts[0].Trim();

        if (id == "ExtraDamage_OnDealDamage")
        {
            int baseExtra = 0;
            if (parts.Length > 1) int.TryParse(parts[1], out baseExtra);
            return new ExtraDamage_OnDealDamage(baseExtra);
        }
        else if (id == "LifeSteal_OnDealDamage")
        {
            int baseExtra = 0;
            if (parts.Length > 1) int.TryParse(parts[1], out baseExtra);
            return new LifeSteal_OnDealDamage(baseExtra);
        }
        else if (id == "HealPlayer_OnPlay")
        {
            int healAmount = 0;
            if (parts.Length > 1) int.TryParse(parts[1], out healAmount);
            return new HealPlayer_OnPlay(healAmount);
        }
        else if (id == "BounceToRandomEnemy_OnDealDamage")
        {
            int bounceCount = 0;
            if (parts.Length > 1) int.TryParse(parts[1], out bounceCount);
            return new BounceToRandomEnemy_OnDealDamage(bounceCount);
        }
        else if (id == "ExtraDamage_NoDefence")
        {
            int baseExtra = 0;
            if (parts.Length > 1) int.TryParse(parts[1], out baseExtra);
            return new ExtraDamage_NoDefence(baseExtra);
        }
        else if (id == "DealDamage_UseDefence")
        {
            int baseExtra = 0;
            if (parts.Length > 1) int.TryParse(parts[1], out baseExtra);
            return new DealDamage_UseDefence(baseExtra);
        }

        // 走原有无参分发
        return Create(id);
    }

    public static ICardEffect Create(string id)
    {
        return factory.TryGetValue(id, out var f) ? f() : null;
    }
}