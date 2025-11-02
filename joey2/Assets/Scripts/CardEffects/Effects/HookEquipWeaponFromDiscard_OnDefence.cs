// Scripts/CardEffects/Effects/HookEquipWeaponFromDiscard_OnDefence.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookEquipWeaponFromDiscard_OnDefence : ICardEffect
{
    public string Id => "HookEquipWeaponFromDiscard_OnDefence";

    public bool MatchTrigger(CardTrigger trigger) => trigger == CardTrigger.UseDefence;

    public IEnumerator Execute(CardEffectContext ctx)
    {
        var battleManager = BattleManager.Instance;
        if (battleManager == null || battleManager.attackPanel == null)
        {
            yield break;
        }

        if (battleManager.usedCardList == null || battleManager.usedCardList.Count == 0)
        {
            yield break;
        }

        List<GameObject> usedWeapons = new List<GameObject>();
        foreach (var candidate in battleManager.usedCardList)
        {
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

        var selected = usedWeapons[Random.Range(0, usedWeapons.Count)];

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

        Vector3 targetScale = new Vector3(0.8f, 0.8f, 1f);
        Transform panel = battleManager.attackPanel;
        GameObject referenceCard = null;
        if (panel != null)
        {
            for (int i = panel.childCount - 1; i >= 0; i--)
            {
                var child = panel.GetChild(i).gameObject;
                if (child == selected)
                {
                    continue;
                }
                if (child.activeInHierarchy)
                {
                    referenceCard = child;
                    break;
                }
            }
        }
        if (referenceCard != null)
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


