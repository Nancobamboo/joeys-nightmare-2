// Scripts/CardEffects/Effects/HealPlayer_OnPlay.cs
using System.Collections;
using UnityEngine;

public class HealPlayer_OnPlay : ICardEffect
{
    public int healAmount; // CSV 传入的治疗量

    public HealPlayer_OnPlay(int healAmount)
    {
        this.healAmount = Mathf.Max(0, healAmount);
    }

    public string Id => "HealPlayer_OnPlay";
    public bool MatchTrigger(CardTrigger trigger) => trigger == CardTrigger.OnPlay;

    public IEnumerator Execute(CardEffectContext ctx)
    {
        if (healAmount <= 0) yield break;

        // Calculate new health (don't exceed max health)
        int newHealth = Mathf.Min(
            PData.Instance.playerHealth + healAmount,
            PData.Instance.playerMaxHealth
        );

        // Update player health
        PData.Instance.SetPlayerHP(newHealth);

        // TODO: Play heal VFX/sound
        yield return null;
    }
}

