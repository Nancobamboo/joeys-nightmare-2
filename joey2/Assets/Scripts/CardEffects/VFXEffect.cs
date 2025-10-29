// Scripts/CardEffects/VFXEffect.cs
using System.Collections;
using UnityEngine;

/// <summary>
/// VFX 效果类型
/// </summary>
public enum VFXType
{
    ParticleSystem,    // 粒子系统
    Animation,         // Animator 动画
    SpriteAnimation,   // 逐帧 Sprite 动画
    AudioClip,         // 音效
    Shake              // 屏幕震动
}

/// <summary>
/// VFX 类别（何时触发）
/// </summary>
public enum VFXCategory
{
    Attack,   // 攻击特效
    Hit       // 受击特效
}

/// <summary>
/// 单个特效配置
/// </summary>
[System.Serializable]
public class VFXConfig
{
    public string id;                    // 特效唯一 ID
    public VFXCategory category;         // 攻击/受击
    public VFXType type;                 // 特效类型
    public float duration;               // 持续时间（秒）
    public float delay;                  // 延迟时间（秒）
    
    // 根据类型设置
    public ParticleSystem particleSystemPrefab;
    public AnimationClip animationClip;
    public AudioClip audioClip;
    public int spriteAnimationFrameCount;
    public float spriteFrameRate;
    
    // 位置
    public bool useTargetPosition = true;  // 是否在目标位置播放
    public Vector3 positionOffset;
}

/// <summary>
/// VFX 序列（包含多个 VFX）
/// </summary>
[System.Serializable]
public class VFXSequence
{
    public string id;                           // 序列 ID
    public System.Collections.Generic.List<VFXConfig> attackVFXs = new();    // 攻击特效列表
    public System.Collections.Generic.List<VFXConfig> hitVFXs = new();       // 受击特效列表
}