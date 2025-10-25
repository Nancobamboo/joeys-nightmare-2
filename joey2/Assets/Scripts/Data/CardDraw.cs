using System.Collections.Generic;
using System.IO;
using UnityEngine;

public sealed class CardDraw : PureSingleton<CardDraw>
{




    public List<List<string>> DrawCardEnv(int level)
    {
        return [["1001", "1002"],["1004", "1005"],  ["1007", "1008"],  ["1010", "1011"],["1013", "1014"]];
    }

}