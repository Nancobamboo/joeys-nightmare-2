// Scripts/CardEffects/CardEffectCore.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardTrigger
{
    OnBecomeTopOfPile,      // 刚到堆顶
    OnPlay,                 // 被使用
    OnDealDamage,           // 造成伤害后
    OnTakeDamage,           // 受到伤害后
    OnKill,                 // 击杀后
    OnTurnStart_Player,     // 我方回合开始
    OnTurnEnd_Player,       // 我方回合结束
    OnTurnStart_Enemy,      // 敌方回合开始
    OnTurnEnd_Enemy        // 敌方回合结束
}

public struct CardEffectContext
{
    public GameObject source;           // 触发效果的卡
    public GameObject target;           // 目标（可能为空）
    public int value;                   // 伤害/治疗等值
    public Dictionary<string, object> extra; // 可扩展参数
}

public interface ICardEffect
{
    string Id { get; }
    bool MatchTrigger(CardTrigger trigger);
    IEnumerator Execute(CardEffectContext ctx);
}