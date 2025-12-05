using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YWalkingChicken : YDefaultEffect
{
    public YWalkingChicken()
    {
        Id = ECardEffectId.WalkingChicken;
    }

    public override void SetData(UICardSimpleControl cardControl)
    {
        base.SetData(cardControl);
        if (CardControl != null)
        {
            CardControl.AddBuff(EBuffType.Counter, 2);
        }
    }

    public override int OnBuffValueChange(EBuffType buffType, int value)
    {
        if (buffType == EBuffType.Counter)
        {
            value--;
            if (value == 0)
            {
                YActionSystem.Instance.DispatchAction(EActionId.MoveEnvCardLeft, CardControl);
            }
        }
        return value;
    }
}

public partial class UIGamePhaseControl
{
    void MoveEnvCardLeft(object[] paraArray)
    {
        UICardSimpleControl cardControl = (UICardSimpleControl)paraArray[0];
        MoveEnvCardLeft(cardControl);
    }

    void MoveEnvCardLeft(UICardSimpleControl cardControl)
    {
        if (cardControl == null || m_EnvPanels == null || m_EnvPanels.Count == 0)
        {
            return;
        }

        int currentEnvIndex = cardControl.EnvIndex;
        int leftEnvIndex = currentEnvIndex - 1;

        if (leftEnvIndex < 0)
        {
            leftEnvIndex = m_EnvPanels.Count - 1;
        }

        if (m_EnvCardDict.TryGetValue(currentEnvIndex, out List<UICardSimpleControl> currentList))
        {
            currentList.Remove(cardControl);
        }

        cardControl.EnvIndex = leftEnvIndex;
        AddEnvCard(leftEnvIndex, cardControl);

        VerticalLayoutGroup targetParent = m_EnvPanels[leftEnvIndex];
        cardControl.CacheTrans.SetParent(targetParent.transform);
        //cardControl.CacheTrans.SetAsFirstSibling();

        cardControl.AddBuff(EBuffType.Counter, 2);
    }
}

