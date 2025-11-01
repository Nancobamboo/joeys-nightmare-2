// Scripts/CardEffects/Effects/DoubleAttack_OnPlay.cs
using System.Collections;
using UnityEngine;

public class DoubleAttack_OnPlay : ICardEffect
{
    public string Id => "DoubleAttack_OnPlay";
    public bool MatchTrigger(CardTrigger trigger) => trigger == CardTrigger.OnPlay;

    // Double attack state tracking
    private static GameObject lastAttackTarget = null;
    private static bool isInDoubleAttackSequence = false;
    private static int pendingAttacksInSequence = 0;
    private static bool isSubscribed = false;

    public IEnumerator Execute(CardEffectContext ctx)
    {
        PData.Instance.nextAttackPlayTwoCards = true;
        
        if (!isSubscribed)
        {
            GameEvents.OnAttackInitiated += OnAttackInitiated;
            GameEvents.OnAttackPreFinish += OnAttackPreFinish;
            GameEvents.OnDamageComplete += OnDamageComplete;
            isSubscribed = true;
        }
        
        yield return null;
    }

    private static void OnAttackInitiated(GameObject attackCardGO, GameObject targetCardGO)
    {
        if (PData.Instance.nextAttackPlayTwoCards && !isInDoubleAttackSequence)
        {
            lastAttackTarget = targetCardGO;
            isInDoubleAttackSequence = true;
            pendingAttacksInSequence = 2;
        }
    }

    private static void OnAttackPreFinish(GameObject attackerCardGO)
    {
        if (isInDoubleAttackSequence)
        {
            pendingAttacksInSequence--;
            
            if (pendingAttacksInSequence > 0 && lastAttackTarget != null)
            {
                GameObject nextAttackCard = FindNextAttackCard(attackerCardGO);
                if (nextAttackCard != null && lastAttackTarget != null && nextAttackCard.activeInHierarchy)
                {
                    PData.Instance.nextAttackPlayTwoCards = false;
                    GameEvents.RaiseCardFinished(cardGO: attackerCardGO);
                    BattleManager.Instance.StartCoroutine(PerformSecondAttackSequence(nextAttackCard, lastAttackTarget, attackerCardGO));
                    return;
                }
                else
                {
                    ResetState();
                }
            }
        }
        
        GameEvents.RaiseCardFinished(cardGO: attackerCardGO);
    }

    private static GameObject FindNextAttackCard(GameObject previousAttackCard)
    {
        GameObject nextAttackCard = null;
        Transform attackPanel = BattleManager.Instance.attackPanel;
        
        for (int i = attackPanel.childCount - 1; i >= 0; i--)
        {
            var go = attackPanel.GetChild(i).gameObject;
            if (go.activeInHierarchy && go != previousAttackCard)
            {
                var cardDisplay = go.GetComponent<CardDisplay>();
                if (cardDisplay != null && cardDisplay.card != null && cardDisplay.card.state != CardState.Used)
                {
                    nextAttackCard = go;
                    break;
                }
            }
        }
        
        if (nextAttackCard == null && previousAttackCard != null)
        {
            if (previousAttackCard.transform.parent == attackPanel && previousAttackCard.activeInHierarchy)
            {
                var cardDisplay = previousAttackCard.GetComponent<CardDisplay>();
                if (cardDisplay != null && cardDisplay.card != null)
                {
                    nextAttackCard = previousAttackCard;
                }
            }
        }
        
        return nextAttackCard;
    }

    private static IEnumerator PerformSecondAttackSequence(GameObject nextAttackCard, GameObject target, GameObject previousCard)
    {
        yield return new WaitForSeconds(1.2f);
        
        Transform attackPanel = BattleManager.Instance.attackPanel;
        
        if (nextAttackCard == previousCard)
        {
            if (nextAttackCard.transform.parent != attackPanel || !nextAttackCard.activeInHierarchy)
            {
                nextAttackCard = FindNextAttackCard(null);
                if (nextAttackCard == null)
                {
                    ResetState();
                    yield break;
                }
            }
            else
            {
                var cardDisplay = nextAttackCard.GetComponent<CardDisplay>();
                if (cardDisplay != null && cardDisplay.card != null && cardDisplay.card.state == CardState.Used)
                {
                    cardDisplay.card.state = CardState.Active;
                }
            }
        }
        
        if (target != null && target.activeInHierarchy && nextAttackCard != null && nextAttackCard.activeInHierarchy)
        {
            BattleManager.Instance.UseAttack(nextAttackCard, target);
        }
        else
        {
            ResetState();
        }
    }

    private static void OnDamageComplete(GameObject enemy, bool monsterAttack = false)
    {
        if (monsterAttack) return;
        
        if (isInDoubleAttackSequence && pendingAttacksInSequence <= 0)
        {
            ResetState();
            if (enemy != null && enemy.activeInHierarchy)
            {
                BattleManager.Instance.StartCoroutine(VFX.PlayMonsterHit(cardGO: enemy));
            }
        }
    }

    private static void ResetState()
    {
        lastAttackTarget = null;
        isInDoubleAttackSequence = false;
        pendingAttacksInSequence = 0;
        PData.Instance.nextAttackPlayTwoCards = false;
    }

    public static bool IsInDoubleAttackSequence()
    {
        return isInDoubleAttackSequence;
    }
}

