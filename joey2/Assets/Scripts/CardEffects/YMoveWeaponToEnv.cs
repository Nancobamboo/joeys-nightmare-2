using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Threading;

public class YMoveWeaponToEnv : YCardEffect
{
    public YMoveWeaponToEnv()
    {
        Id = ECardEffectId.MoveWeaponToEnv;
    }

    public override float UseSkill()
    {
        if (CardControl != null)
        {
            YActionSystem.Instance.DispatchAction(EActionId.AddEnvCardFromBag, CardControl);
        }
        return base.UseSkill();
    }

    public override float UseItem()
    {
        if (CardControl != null)
        {
            YActionSystem.Instance.DispatchAction(EActionId.AddEnvCardFromBag, CardControl);
        }
        return base.UseItem();
    }
}

public partial class UIGamePhaseControl
{
    async void AddEnvCardFromBag(UICardSimpleControl cardControl)
    {
        if (cardControl == null || cardControl.CardData == null)
        {
            return;
        }
        if (m_EnvPanels == null || m_EnvPanels.Count == 0)
        {
            return;
        }
        int randomIndex = Random.Range(0, m_EnvPanels.Count);
        VerticalLayoutGroup parent = m_EnvPanels[randomIndex];
        Transform effectRoot = GetEffectRoot(randomIndex);
        if (effectRoot == null)
        {
            return;
        }
        Card newCard = cardControl.CardData;
        m_CardDict[newCard.UniqueId] = newCard;
        UICardSimpleControl newCardControl = GetCardSimple(Asset.UIRoot, true);
        newCardControl.SetData(newCard, isEnv: true, envIndex: randomIndex);
        newCardControl.SetMoving(true);
        AddEnvCard(randomIndex, newCardControl);

        Vector3 startWorldPos = cardControl.CacheTrans.position;
        Vector3 startScale = cardControl.CacheTrans.localScale;
        Vector3 endWorldPos = effectRoot.position;
        Vector3 endScale = Vector3.one;
        float duration = 0.2f;

        await MoveCardToEnvAnimation(newCardControl, startWorldPos, endWorldPos, startScale, endScale, duration, parent);

        newCardControl.CacheTrans.SetParent(parent.transform);
        newCardControl.CacheTrans.localPosition = Vector3.zero;
        newCardControl.CacheTrans.localScale = Vector3.one;
        newCardControl.CacheTrans.localEulerAngles = Vector3.zero;
        parent.enabled = true;
        newCardControl.SetMoving(false);
    }

    private async UniTask MoveCardToEnvAnimation(UICardSimpleControl cardControl, Vector3 startPos, Vector3 endPos, Vector3 startScale, Vector3 endScale, float duration, VerticalLayoutGroup layout)
    {
        layout.enabled = false;
        cardControl.CacheTrans.SetParent(Asset.UIRoot);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            cardControl.CacheTrans.position = Vector3.Lerp(startPos, endPos, t);
            cardControl.CacheTrans.localScale = Vector3.Lerp(startScale, endScale, t);

            await UniTask.Yield();
        }
    }
}

