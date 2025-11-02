// Scripts/CardEffects/Effects/HookEquipWeaponFromDiscard_OnPlay.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookEquipWeaponFromDiscard_OnPlay : ICardEffect
{
    public string Id => "HookEquipWeaponFromDiscard_OnPlay";

    public bool MatchTrigger(CardTrigger trigger) => trigger == CardTrigger.OnPlay;

    public IEnumerator Execute(CardEffectContext ctx)
    {
        var battleManager = BattleManager.Instance;
        if (battleManager == null || battleManager.attackPanel == null)
        {
            yield break;
        }

        // Collect all used attack cards that can be re-equipped
        List<GameObject> usedWeapons = new List<GameObject>();
        for (int i = 0; i < battleManager.usedCardList.Count; i++)
        {
            var candidate = battleManager.usedCardList[i];
            if (candidate == null)
            {
                continue;
            }

            var display = candidate.GetComponent<CardDisplay>();
            if (display == null || display.card == null)
            {
                continue;
            }

            if (display.card.type == "attack")
            {
                usedWeapons.Add(candidate);
            }
        }

        if (usedWeapons.Count == 0)
        {
            yield break;
        }

        // Pick one used weapon at random
        var selected = usedWeapons[Random.Range(0, usedWeapons.Count)];

        // Move it back to the attack pile
        CardHelper.MoveCard(
            cardGO: selected,
            fromCardList: battleManager.usedCardList,
            toCardList: battleManager.attackCardList,
            state: CardState.Inactive,
            position: CardPosition.Bag
        );

        selected.transform.SetParent(battleManager.attackPanel, false);
        selected.transform.SetAsLastSibling();
        selected.SetActive(true);

        // Reset layout related components to ensure the card is visible again
        var layoutElement = selected.GetComponent<UnityEngine.UI.LayoutElement>();
        if (layoutElement != null)
        {
            layoutElement.ignoreLayout = false;
        }

        var canvasGroup = selected.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        selected.transform.localRotation = Quaternion.identity;

        // Try to align scale with existing attack card; fallback to 0.8 when none exists
        Vector3 targetScale = new Vector3(0.8f, 0.8f, 1f);
        GameObject referenceCard = UIGridHelper.GetCardListOrderIndex0(battleManager.attackPanel);
        if (referenceCard != null && referenceCard != selected)
        {
            targetScale = referenceCard.transform.localScale;
        }
        selected.transform.localScale = targetScale;
        selected.transform.localPosition = Vector3.zero;

        var selectedDisplay = selected.GetComponent<CardDisplay>();
        if (selectedDisplay != null && selectedDisplay.card != null)
        {
            selectedDisplay.ShowCard();
        }

        BattleManager.Instance.StartCoroutine(VFX.PlayAnimator(selected, "UI_Carditem_pailai"));

        UIGridHelper.RefreshPanel(battleManager.attackPanel);

        yield return null;
    }
}


