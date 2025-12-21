// Scripts/CardEffects/YKnightShield_OnTop.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YKnightShield_OnTop : YCardEffect
{
    private float lastDefenceTime = 0f; // Track last defence time to prevent duplicate audio

    public YKnightShield_OnTop()
    {
        Id = ECardEffectId.KnightShield_OnTop;
    }

    public override float OnBecomeTopOfPile()
    {
        // Each time equipped: add 1 use
        if (CardControl != null && CardControl.CardData != null)
        {
            int beforeBonus = CardControl.CardData.durability;
            CardControl.CardData.durability++;
            Debug.Log($"[KnightShield] OnBecomeTopOfPile - Card: {CardControl.CardData.cardName}, UniqueId: {CardControl.CardData.UniqueId}, equipment bonus added: {beforeBonus} -> {CardControl.CardData.durability}");
            
            // Update card description to reflect new durability
            CardControl.UpdateDurabilityDescription();
        }
        else
        {
            Debug.LogWarning($"[KnightShield] OnBecomeTopOfPile - CardControl or CardData is null!");
        }
        return base.OnBecomeTopOfPile();
    }

    public override float OnEnterBag()
    {
        // OnBecomeTopOfPile will handle initialization
        if (CardControl != null && CardControl.CardData != null)
        {
            Debug.Log($"[KnightShield] OnEnterBag - Card: {CardControl.CardData.cardName}, UniqueId: {CardControl.CardData.UniqueId}, current durability: {CardControl.CardData.durability}");
        }
        else
        {
            Debug.LogWarning($"[KnightShield] OnEnterBag - CardControl or CardData is null!");
        }
        return base.OnEnterBag();
    }

    public override float UseDefence(bool isOverflow = false)
    {
        int durability = CardControl?.CardData?.durability ?? 0;
        Debug.Log($"[KnightShield] UseDefence - Card: {CardControl?.CardData?.cardName}, UniqueId: {CardControl?.CardData?.UniqueId}, isOverflow={isOverflow}, durability={durability}");
        
        if (CardControl != null && CardControl.gameObject != null)
        {
            // Prevent duplicate audio within 0.1 seconds
            float currentTime = Time.time;
            bool shouldPlayAudio = (currentTime - lastDefenceTime) > 0.1f;
            
            if (shouldPlayAudio)
            {
                lastDefenceTime = currentTime;
            }
            else
            {
                Debug.LogWarning($"[KnightShield] Skipping duplicate defence audio (time diff: {currentTime - lastDefenceTime})");
            }
            
            // Choose VFX based on whether defence was broken through
            List<EVFXName> vfxNames;
            if (isOverflow)
            {
                vfxNames = new List<EVFXName> { EVFXName.VFX_Dunsui };
            }
            else
            {
                vfxNames = new List<EVFXName> { EVFXName.VFX_Dun };
            }
            float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);
            
            if (shouldPlayAudio)
            {
                SFX.PlayAudio("Audio/SFX/Battle/Defence", 1.0f, 0f);
                Debug.Log($"[KnightShield] Defence audio played");
            }
            
            return maxDelayTime > 0f ? maxDelayTime : base.UseDefence(isOverflow);
        }
        return base.UseDefence(isOverflow);
    }

    public override float OnUseFinished(bool isSkip)
    {
        // Decrease durability after defending
        if (CardControl != null && CardControl.CardData != null)
        {
            int oldDurability = CardControl.CardData.durability;
            CardControl.CardData.durability--;
            Debug.Log($"[KnightShield] OnUseFinished - Card: {CardControl.CardData.cardName}, UniqueId: {CardControl.CardData.UniqueId}, durability: {oldDurability} -> {CardControl.CardData.durability}");
            
            // Update card description to reflect new durability
            CardControl.UpdateDurabilityDescription();
            
            // If durability reaches 0, play destruction animation
            if (CardControl.CardData.durability <= 0)
            {
                Debug.Log($"[KnightShield] Durability depleted, playing destruction animation");
                return base.OnUseFinished(isSkip);
            }
        }
        else
        {
            Debug.LogWarning($"[KnightShield] OnUseFinished - CardControl or CardData is null!");
        }
        
        // Still has durability, skip animation
        return 0f;
    }

    public override bool ShouldKeepInBag()
    {
        // Keep card in bag if it still has durability
        int durability = CardControl?.CardData?.durability ?? 0;
        bool shouldKeep = durability > 0;
        Debug.Log($"[KnightShield] ShouldKeepInBag - Card: {CardControl?.CardData?.cardName}, UniqueId: {CardControl?.CardData?.UniqueId}, durability={durability}, shouldKeep={shouldKeep}");
        return shouldKeep;
    }
}

