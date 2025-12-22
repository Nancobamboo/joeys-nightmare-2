// Scripts/CardEffects/YKnightSword_OnTop.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YKnightSword_OnTop : YDefaultEffect
{
    public YKnightSword_OnTop()
    {
        Id = ECardEffectId.KnightSword_OnTop;
    }

    public override float OnBecomeTopOfPile()
    {
        // Each time equipped: add 1 use
        if (CardControl != null && CardControl.CardData != null)
        {
            int beforeBonus = CardControl.CardData.durability;
            CardControl.CardData.durability++;
            Debug.Log($"[KnightSword] OnBecomeTopOfPile - Card: {CardControl.CardData.cardName}, UniqueId: {CardControl.CardData.UniqueId}, equipment bonus added: {beforeBonus} -> {CardControl.CardData.durability}");
            
            // Update card description to reflect new durability
            CardControl.UpdateDurabilityDescription();
        }
        else
        {
            Debug.LogWarning($"[KnightSword] OnBecomeTopOfPile - CardControl or CardData is null!");
        }
        return base.OnBecomeTopOfPile();
    }

    public override float OnEnterBag()
    {
        // OnBecomeTopOfPile will handle initialization
        if (CardControl != null && CardControl.CardData != null)
        {
            Debug.Log($"[KnightSword] OnEnterBag - Card: {CardControl.CardData.cardName}, UniqueId: {CardControl.CardData.UniqueId}, current durability: {CardControl.CardData.durability}");
        }
        else
        {
            Debug.LogWarning($"[KnightSword] OnEnterBag - CardControl or CardData is null!");
        }
        return base.OnEnterBag();
    }

    public override float OnUseFinished(bool isSkip)
    {
        // Decrease durability after attack
        if (CardControl != null && CardControl.CardData != null)
        {
            int oldDurability = CardControl.CardData.durability;
            CardControl.CardData.durability--;
            Debug.Log($"[KnightSword] OnUseFinished - Card: {CardControl.CardData.cardName}, UniqueId: {CardControl.CardData.UniqueId}, durability: {oldDurability} -> {CardControl.CardData.durability}");
            
            // Update card description to reflect new durability
            CardControl.UpdateDurabilityDescription();
            
            // If durability reaches 0, play destruction animation
            if (CardControl.CardData.durability <= 0)
            {
                Debug.Log($"[KnightSword] Durability depleted, playing destruction animation");
                return base.OnUseFinished(isSkip);
            }
        }
        else
        {
            Debug.LogWarning($"[KnightSword] OnUseFinished - CardControl or CardData is null!");
        }
        
        // Still has durability, skip animation
        return 0f;
    }

    public override bool ShouldKeepInBag()
    {
        // Keep card in bag if it still has durability
        int durability = CardControl?.CardData?.durability ?? 0;
        bool shouldKeep = durability > 0;
        Debug.Log($"[KnightSword] ShouldKeepInBag - Card: {CardControl?.CardData?.cardName}, UniqueId: {CardControl?.CardData?.UniqueId}, durability={durability}, shouldKeep={shouldKeep}");
        return shouldKeep;
    }
}

