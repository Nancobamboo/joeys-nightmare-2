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
    private bool m_AttackScheduled;
    private bool m_WasTop;
    private CancellationTokenSource m_MonitorCts;

    public YShadowTurkey()
    {
        Id = ECardEffectId.ShadowTurkey;
    }

    public override void SetData(UICardSimpleControl cardControl)
    {
        base.SetData(cardControl);

        // 只在自己内部做“是否成为堆顶”的自检，不依赖外部通知，避免影响其它卡
        StartMonitor();
    }

    public override float OnBecomeTopOfPile()
    {
        // 保留该入口：如果未来某处确实调用了 OnBecomeTopOfPile，也走同一套触发逻辑
        TryTriggerAttackIfNowTop();
        return base.OnBecomeTopOfPile();
    }

    public override float OnDead()
    {
        StopMonitor();
        return base.OnDead();
    }

    public override float OnRemoveCard()
    {
        StopMonitor();
        return base.OnRemoveCard();
    }

    private void StartMonitor()
    {
        StopMonitor();
        m_WasTop = false;
        m_MonitorCts = new CancellationTokenSource();
        MonitorTopStateAsync(m_MonitorCts.Token).Forget();
    }

    private void StopMonitor()
    {
        if (m_MonitorCts != null && !m_MonitorCts.IsCancellationRequested)
        {
            m_MonitorCts.Cancel();
            m_MonitorCts.Dispose();
        }
        m_MonitorCts = null;
        m_WasTop = false;
        m_AttackScheduled = false;
    }

    private async UniTaskVoid MonitorTopStateAsync(CancellationToken token)
    {
        // 轻量轮询：只对 ShadowTurkey 生效，用来捕捉“上面的怪移动走后，我露出成为顶牌”这种瞬间
        // 间隔不用太小，避免无意义开销；0.05s 足够“看起来即时”
        const float CHECK_INTERVAL = 0.05f;

        while (!token.IsCancellationRequested)
        {
            if (CardControl == null || CardControl.CardData == null || CardControl.gameObject == null || !CardControl.gameObject.activeSelf)
            {
                // 被回收到对象池/销毁：结束监控
                break;
            }

            bool isTopNow = IsEnvMonsterTopNow();
            if (isTopNow && !m_WasTop)
            {
                // 从“非顶牌”变为“顶牌”：触发一次
                TryTriggerAttackIfNowTop();
            }

            m_WasTop = isTopNow;
            await UniTask.WaitForSeconds(CHECK_INTERVAL, cancellationToken: token);
        }

        // 退出前清理
        StopMonitor();
    }

    private bool IsEnvMonsterTopNow()
    {
        if (CardControl == null || CardControl.CardData == null) return false;
        if (!CardControl.IsEnv || CardControl.CardType != ECardType.monster) return false;

        int envIndex = CardControl.EnvIndex;
        if (envIndex < 0) return false;
        if (JoeyGameControl.Instance == null) return false;

        // 以“环境该列最外层卡牌 == 我”为准
        return JoeyGameControl.Instance.IsCardOnTop(CardControl, envIndex);
    }

    private void TryTriggerAttackIfNowTop()
    {
        // 防止同一段时间重复触发
        if (m_AttackScheduled) return;
        if (!IsEnvMonsterTopNow()) return;

        int attack = CardControl?.CardData?.currentAttack ?? 0;
        if (attack <= 0) return;

        int envIndex = CardControl.EnvIndex;
        m_AttackScheduled = true;
        TriggerAttackOnceAsync(attack, envIndex).Forget();
    }

    private async UniTaskVoid TriggerAttackOnceAsync(int attack, int envIndex)
    {
        // 用独立延迟，避免被 JoeyGameControl 的 SingleDelayAction（只能缓存一个 action）覆盖/取消
        await UniTask.WaitForSeconds(0.15f);

        try
        {
            if (CardControl == null || CardControl.CardData == null || !CardControl.gameObject.activeSelf)
            {
                return;
            }
            if (JoeyGameControl.Instance == null)
            {
                return;
            }
            if (!CardControl.IsEnv || CardControl.CardType != ECardType.monster || envIndex < 0)
            {
                return;
            }
            if (!JoeyGameControl.Instance.IsCardOnTop(CardControl, envIndex))
            {
                return;
            }

            // 走完整的“怪物主动攻击玩家”流程（包含防御/反击等逻辑）
            JoeyGameControl.Instance.QueueAction(EActionId.TakePlayerDamage, attack, CardControl, envIndex);
        }
        finally
        {
            m_AttackScheduled = false;
        }
    }
}


