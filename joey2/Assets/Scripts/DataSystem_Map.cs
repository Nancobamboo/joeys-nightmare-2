using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public partial class DataSystem
{
    public Dictionary<int, float> VFXDelayTimeDict = new Dictionary<int, float>();

    public void LoadGameData()
    {
        Debug.Log("LoadGameData called");
        LoadDataJoeyPlayer();
        LoadVFX();
    }

    public void LoadVFX()
    {
        VFXDelayTimeDict[(int)EVFXName.VFX_Dun] = 1.0f;
        VFXDelayTimeDict[(int)EVFXName.VFX_boom] = 0.65f;
        VFXDelayTimeDict[(int)EVFXName.VFX_Shouji] = 1.0f;
        VFXDelayTimeDict[(int)EVFXName.VFX_LeiDan] = 0.65f;
        VFXDelayTimeDict[(int)EVFXName.VFX_appear] = 0f;
        VFXDelayTimeDict[(int)EVFXName.VFX_disappear] = 0f;
        VFXDelayTimeDict[(int)EVFXName.VFX_Dunsui] = 0f;
        VFXDelayTimeDict[(int)EVFXName.VFX_glow] = 0f;
    }

    public float GetVFXDelayTime(EVFXName vfxName)
    {
        if (VFXDelayTimeDict.TryGetValue((int)vfxName, out float delayTime))
        {
            return delayTime;
        }
        return 0f;
    }
}

