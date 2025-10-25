using System.Collections.Generic;
using System.IO;
using UnityEngine;

public sealed class CardDraw : PureSingleton<CardDraw>
{




    public List<List<string>> DrawCardEnv(int level)
    {
        return new List<List<string>>
        {
            new List<string> { "1001", "1002" },
            new List<string> { "1004", "1005" },
            new List<string> { "1007", "1008" },
            new List<string> { "1010", "1011" },
            new List<string> { "1013", "1014" }
        };
    }

}