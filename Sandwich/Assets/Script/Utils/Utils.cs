using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static float RoundToRectValue(float t)
    {
        int value = Mathf.RoundToInt(t);
        int module = value % 90;

        if (module == 0) return value;

        if (module > 45) return value + (90 - module);

        return value - module;
    }
}

public static class Constants
{
    public const int GRID_SIZE = 4;
}
