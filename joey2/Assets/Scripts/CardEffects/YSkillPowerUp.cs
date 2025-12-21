using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class YSkillPowerUp : YCardEffect
{
  public int baseExtra;
  public YSkillPowerUp(int baseExtra)
  {
    Id = ECardEffectId.SkillPowerUp;
    this.baseExtra = baseExtra;
  }

  public override float UseSkill()
  {
    if (CardControl != null && CardControl.gameObject != null)
    {
      var vfxNames = new List<EVFXName> { };
      float maxDelayTime = CardControl.PlayVFX(vfxNames, ECardAnimName.UI_Carditem_dunpai, EVFXLife.SelfLife);

      // 激活技能伤害加成效果
      YActionSystem.Instance.DispatchAction(EActionId.SkillPowerUpActivate, baseExtra);

      return 0.3f;
    }
    return base.UseSkill();
  }
}

public partial class UIGamePhaseControl
{
  // 技能伤害加成值（当前关卡内有效）
  private int m_SkillDamageBonus = 0;
  // 标记是否需要延迟清除加成
  private bool m_SkillDamageBonusPendingClear = false;

  // 定义哪些 EEffectType 是技能伤害类型，会受到技能伤害加成的影响
  private static readonly HashSet<EEffectType> SkillDamageTypes = new HashSet<EEffectType>
  {
    EEffectType.FireBall,
    EEffectType.Electric,
    EEffectType.IceMagic,
    // 如果需要添加新的技能伤害类型，只需在这里添加即可
  };

  void SkillPowerUpActivate(object[] paraArray)
  {
    int bonusValue = (int)paraArray[0];
    m_SkillDamageBonus += bonusValue;
    m_SkillDamageBonusPendingClear = false; // 新增加成时取消待清除状态
    Debug.Log($"SkillPowerUp: Skill damage bonus increased by {bonusValue}, total bonus: {m_SkillDamageBonus}");
  }

  /// <summary>
  /// 检查是否是技能伤害类型
  /// </summary>
  private bool IsSkillDamageType(EEffectType effectType)
  {
    return SkillDamageTypes.Contains(effectType);
  }

  /// <summary>
  /// 获取应用技能伤害加成后的最终伤害
  /// 加成会在同一技能的所有伤害应用后延迟清除
  /// </summary>
  private int ApplySkillDamageBonus(int baseDamage, EEffectType effectType)
  {
    if (IsSkillDamageType(effectType) && m_SkillDamageBonus > 0)
    {
      int finalDamage = baseDamage + m_SkillDamageBonus;
      Debug.Log($"SkillPowerUp: Applied bonus {m_SkillDamageBonus} to {effectType}, final damage: {finalDamage}");
      
      // 标记需要延迟清除，并启动延迟清除任务
      if (!m_SkillDamageBonusPendingClear)
      {
        m_SkillDamageBonusPendingClear = true;
        ClearSkillDamageBonusDelayed().Forget();
      }
      
      return finalDamage;
    }
    return baseDamage;
  }

  /// <summary>
  /// 延迟清除技能伤害加成（等待当前帧结束后清除）
  /// </summary>
  private async UniTaskVoid ClearSkillDamageBonusDelayed()
  {
    // 等待当前帧结束
    await UniTask.Yield();
    
    if (m_SkillDamageBonusPendingClear)
    {
      Debug.Log($"SkillPowerUp: Clearing skill damage bonus {m_SkillDamageBonus}");
      m_SkillDamageBonus = 0;
      m_SkillDamageBonusPendingClear = false;
    }
  }
}
