using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public partial class DataSystem
{
    public void LoadGameData()
    {
        Debug.Log("LoadGameData called");
        LoadDataJoeyPlayer();
    }
}

