using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public class SingleDelayAction
{
    private Action m_CachedAction;
    private CancellationTokenSource m_CancellationTokenSource;

    public void AddDelayCall(Action action, float delayTime)
    {
        if (m_CancellationTokenSource != null && !m_CancellationTokenSource.IsCancellationRequested)
        {
            m_CancellationTokenSource.Cancel();
            m_CancellationTokenSource.Dispose();
            if (m_CachedAction != null)
            {
                m_CachedAction.Invoke();
            }
        }

        m_CachedAction = action;
        m_CancellationTokenSource = new CancellationTokenSource();
        DelayCall(action, delayTime, m_CancellationTokenSource).Forget();
    }

    private async UniTaskVoid DelayCall(Action action, float delayTime, CancellationTokenSource cts)
    {
        await UniTask.WaitForSeconds(delayTime, cancellationToken: cts.Token);
        if (!cts.IsCancellationRequested && action != null)
        {
            action.Invoke();
        }
        m_CancellationTokenSource?.Dispose();
        m_CancellationTokenSource = null;
        m_CachedAction = null;
    }

    public void AddAnimDelayAction(Func<CancellationToken, UniTask> animActionFactory, Action delayAction)
    {
        if (m_CancellationTokenSource != null && !m_CancellationTokenSource.IsCancellationRequested)
        {
            m_CancellationTokenSource.Cancel();
            m_CancellationTokenSource.Dispose();
            if (m_CachedAction != null)
            {
                m_CachedAction.Invoke();
            }
        }

        m_CachedAction = delayAction;
        m_CancellationTokenSource = new CancellationTokenSource();
        UniTask animTask = animActionFactory(m_CancellationTokenSource.Token);
        ExecuteAnimAction(animTask, delayAction, m_CancellationTokenSource).Forget();
    }

    private async UniTaskVoid ExecuteAnimAction(UniTask animAction, Action delayAction, CancellationTokenSource cts)
    {
        try
        {
            await animAction.AttachExternalCancellation(cts.Token);
        }
        catch (System.OperationCanceledException)
        {
            return;
        }

        if (!cts.IsCancellationRequested && delayAction != null)
        {
            delayAction.Invoke();
        }

        m_CancellationTokenSource?.Dispose();
        m_CancellationTokenSource = null;
        m_CachedAction = null;
    }

    public void Cancel()
    {
        if (m_CancellationTokenSource != null && !m_CancellationTokenSource.IsCancellationRequested)
        {
            m_CancellationTokenSource.Cancel();
            m_CancellationTokenSource.Dispose();
        }
        m_CancellationTokenSource = null;
        m_CachedAction = null;
    }
}

