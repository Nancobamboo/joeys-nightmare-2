// Scripts/CardEffects/CardEffectRegistry.cs
using System;
using System.Collections.Generic;

public static class CardEffectRegistry
{
    private static readonly Dictionary<string, Func<ICardEffect>> factory = new()
    {
        { "DealRandomEnemyEqualToAttack_OnTop", () => new DealRandomEnemyEqualToAttack_OnTop() },
        { "LifeSteal_OnDealDamage", () => new LifeSteal_OnDealDamage() },
    };

    public static ICardEffect Create(string id)
    {
        return factory.TryGetValue(id, out var f) ? f() : null;
    }
}