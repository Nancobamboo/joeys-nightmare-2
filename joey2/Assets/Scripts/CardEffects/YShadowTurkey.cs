using Cysharp.Threading.Tasks;
using UnityEngine;
using System.Threading;

/// <summary>
/// 暗影Turkey：
/// - 出现/成为“怪物卡堆最上方（环境堆顶）”时：主动攻击一次
/// - 配置来自 card_info.csv: ShadowTurkey
/// </summary>
public class YShadowTurkey : YDefaultEffect
{
    // 纯“卡牌游戏事件”实现：不使用 Time/轮询。
    // 通过监听会影响环境堆叠的 Action，在 Action 发生后自检自己是否变成堆顶。

    private bool m_IsSubscribed;
    private bool m_WasTop;
    private bool m_TriggeredWhileTop;
    private System.Action<object[]> m_OnEnvMaybeChanged;
    private CancellationTokenSource m_InitialTopCheckCts;

    public YShadowTurkey()
    {
        Id = ECardEffectId.ShadowTurkey;
    }

    public override void SetData(UICardSimpleControl cardControl)
    {
        base.SetData(cardControl);
        ResetState();
        EnsureSubscribed();
        ScheduleInitialTopCheck();
    }

    public override float OnDead()
    {
        Unsubscribe();
        return base.OnDead();
    }

    public override float OnRemoveCard()
    {
        Unsubscribe();
        return base.OnRemoveCard();
    }

    private void ResetState()
    {
        m_WasTop = false;
        m_TriggeredWhileTop = false;
        CancelInitialTopCheck();
    }

    private void ScheduleInitialTopCheck()
    {
        // 开局发牌/摆放环境牌不是 Action 驱动的。
        // 为了让“开局就已经在最上层”的暗影turkey也能自动触发一次，
        // 在下一帧（发牌流程结束后）做一次性自检。
        CancelInitialTopCheck();
        m_InitialTopCheckCts = new CancellationTokenSource();
        InitialTopCheckNextFrameAsync(m_InitialTopCheckCts.Token).Forget();
    }

    private void CancelInitialTopCheck()
    {
        if (m_InitialTopCheckCts != null)
        {
            if (!m_InitialTopCheckCts.IsCancellationRequested)
            {
                m_InitialTopCheckCts.Cancel();
            }
            m_InitialTopCheckCts.Dispose();
            m_InitialTopCheckCts = null;
        }
    }

    private async UniTaskVoid InitialTopCheckNextFrameAsync(CancellationToken token)
    {
        await UniTask.NextFrame(token);
        // 可能已被对象池复用/替换 effect：统一走保护逻辑
        TryTriggerOnBecomeTop();
    }

    private void EnsureSubscribed()
    {
        if (m_IsSubscribed) return;
        if (YActionSystem.Instance == null) return;

        m_OnEnvMaybeChanged ??= OnEnvMaybeChanged;

        // 只监听“可能改变环境堆叠/堆顶”的动作（无需任何时间轮询）
        EActionId[] ids =
        {
            EActionId.MoveEnvCardLeft,
            EActionId.MoveCard,
            EActionId.TakeEnemyDamage,
            EActionId.TakeAllEnemyDamage,
            EActionId.BoomEnvCard,
            EActionId.SwapEnvCard,
            EActionId.SwapTopTwoEnvCards,
            EActionId.SwapEnvCardWithRandom,
            EActionId.AddEnvCardFromBag,
            EActionId.AddCardToEnv,
            EActionId.AddCardToSpecifiedEnv,
            EActionId.AddCardsToEnv,
            EActionId.AddCardsToEnvByCardId,
            EActionId.KillSkeletonMonster,
            EActionId.DealSplashDamage,
        };

        for (int i = 0; i < ids.Length; i++)
        {
            YActionSystem.Instance.RegistAction(ids[i], m_OnEnvMaybeChanged);
        }

        m_IsSubscribed = true;

        // 不在这里“立即触发”：初始化发牌阶段不是 Action 驱动，避免在发牌过程中误触发。
        // 正式进入战斗后的任意一次 Action 会触发自检。
    }

    private void Unsubscribe()
    {
        if (!m_IsSubscribed) return;
        if (YActionSystem.Instance == null) { m_IsSubscribed = false; return; }
        if (m_OnEnvMaybeChanged == null) { m_IsSubscribed = false; return; }

        CancelInitialTopCheck();

        EActionId[] ids =
        {
            EActionId.MoveEnvCardLeft,
            EActionId.MoveCard,
            EActionId.TakeEnemyDamage,
            EActionId.TakeAllEnemyDamage,
            EActionId.BoomEnvCard,
            EActionId.SwapEnvCard,
            EActionId.SwapTopTwoEnvCards,
            EActionId.SwapEnvCardWithRandom,
            EActionId.AddEnvCardFromBag,
            EActionId.AddCardToEnv,
            EActionId.AddCardToSpecifiedEnv,
            EActionId.AddCardsToEnv,
            EActionId.AddCardsToEnvByCardId,
            EActionId.KillSkeletonMonster,
            EActionId.DealSplashDamage,
        };

        for (int i = 0; i < ids.Length; i++)
        {
            YActionSystem.Instance.UnRegistAction(ids[i], m_OnEnvMaybeChanged);
        }

        m_IsSubscribed = false;
    }

    private void OnEnvMaybeChanged(object[] _)
    {
        // 处理对象池复用/旧实例残留：如果我已经不是当前 CardEffect，就自我卸载
        if (CardControl == null || CardControl.gameObject == null)
        {
            Unsubscribe();
            return;
        }
        if (!CardControl.gameObject.activeSelf)
        {
            Unsubscribe();
            return;
        }
        if (CardControl.CardEffect != this)
        {
            Unsubscribe();
            return;
        }

        TryTriggerOnBecomeTop();
    }

    private void TryTriggerOnBecomeTop()
    {
        // 处理对象池复用/旧实例残留：如果我已经不是当前 CardEffect，就自我卸载
        if (CardControl == null || CardControl.gameObject == null)
        {
            Unsubscribe();
            return;
        }
        if (!CardControl.gameObject.activeSelf)
        {
            Unsubscribe();
            return;
        }
        if (CardControl.CardEffect != this)
        {
            Unsubscribe();
            return;
        }

        if (JoeyGameControl.Instance == null) return;
        if (CardControl == null || CardControl.CardData == null) return;
        if (!CardControl.IsEnv || CardControl.CardType != ECardType.monster) return;

        int envIndex = CardControl.EnvIndex;
        bool isTop = envIndex >= 0 && JoeyGameControl.Instance.IsCardOnTop(CardControl, envIndex);

        if (!isTop)
        {
            // 被盖住/离开堆顶：允许下次再次露头触发
            m_WasTop = false;
            m_TriggeredWhileTop = false;
            return;
        }

        if (!m_WasTop)
        {
            // 刚刚露头
            m_WasTop = true;
            m_TriggeredWhileTop = false;
        }

        if (m_TriggeredWhileTop) return;

        int attack = CardControl.CardData.currentAttack;
        if (attack <= 0) return;

        m_TriggeredWhileTop = true;
        JoeyGameControl.Instance.QueueAction(EActionId.TakePlayerDamage, attack, CardControl, envIndex);
    }
}


