using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YDartScroll : YCardEffect
{
    public YDartScroll()
    {
        Id = ECardEffectId.DartScroll;
    }

    public override float UseItem()
    {
        YActionSystem.Instance.DispatchAction(EActionId.AddCardsToEnvByCardId, CardControl, "1003", 2);
        return 0f;
    }
}

public partial class UIGamePhaseControl
{
    public void AddCardsToEnvByCardId(UICardSimpleControl cardControl, string cardId, int count)
    {
        if (m_EnvPanels == null || m_EnvPanels.Count == 0)
        {
            return;
        }

        // 选择不重复的 env index，优先选择没有牌的
        List<int> selectedIndices = SelectUniqueEnvIndices(count);

        for (int i = 0; i < selectedIndices.Count; i++)
        {
            Card card = GData.Instance.GetCardConfigById(cardId).Clone();
            
            // 从 EnvCardDict 获取已增强的属性值（如 PermanentBoostAttack 增加的攻击力）
            DataJoeyPlayer dataJoeyPlayer = DataSystem.Instance.GetDataJoeyPlayer();
            Card enhancedCard = dataJoeyPlayer.GetEnvCardDictData(cardId);
            if (enhancedCard != null)
            {
                card.SetAttack(enhancedCard.currentAttack);
                card.SetDefence(enhancedCard.currentDefence);
            }
            
            // Apply difficulty effects to monsters in Env mode
            if (JoeyGameControl.Instance != null && JoeyGameControl.Instance.GameMode == EGameMode.Env && card.GetCardType() == ECardType.monster)
            {
                ApplyEnvDifficultyToMonster(card);
            }
            
            int envIndex = selectedIndices[i];

            VerticalLayoutGroup parent = m_EnvPanels[envIndex];
            UICardSimpleControl newCardControl = GetCardSimple(parent.transform, true);
            newCardControl.SetData(card, isEnv: true, envIndex: envIndex);
            AddEnvCard(envIndex, newCardControl);
            newCardControl.PlayVFX(new List<EVFXName>(), ECardAnimName.UI_Carditem_pailai, EVFXLife.CardLife);
        }

		// Update monster buffs after new cards are added (for BadMonkey/MonkeyKing attack updates)
		for (int i = 0; i < m_EnvPanels.Count; i++)
		{
			UICardSimpleControl lastCard = GetLastEnvCard(i);
			if (lastCard != null && lastCard.CardType == ECardType.monster)
			{
				// Only update cards with UpdateAttack buff to avoid affecting Counter-based effects
				if (lastCard.GetBuffValue(EBuffType.UpdateAttack) > 0)
				{
					lastCard.UpdateBuffValue();
				}
			}
		}
    }

    private List<int> SelectUniqueEnvIndices(int count)
    {
        List<int> result = new List<int>();
        List<int> emptyIndices = new List<int>();
        List<int> nonEmptyIndices = new List<int>();

        // 分类 env index：空的和非空的
        for (int i = 0; i < m_EnvPanels.Count; i++)
        {
            if (!m_EnvCardDict.TryGetValue(i, out List<UICardSimpleControl> cardList) || cardList == null || cardList.Count == 0)
            {
                emptyIndices.Add(i);
            }
            else
            {
                nonEmptyIndices.Add(i);
            }
        }

        // 打乱顺序
        ShuffleList(emptyIndices);
        ShuffleList(nonEmptyIndices);

        // 优先从空的 env 中选择
        foreach (int index in emptyIndices)
        {
            if (result.Count >= count) break;
            result.Add(index);
        }

        // 如果空的不够，再从非空的 env 中选择
        foreach (int index in nonEmptyIndices)
        {
            if (result.Count >= count) break;
            result.Add(index);
        }

        return result;
    }

    private void ShuffleList(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
