using System;
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

    public static Vector3 Round(this Vector3 vector, int decimals)
    {
        return new(
            (float)Math.Round(vector.x, decimals),
            (float)Math.Round(vector.y, decimals),
            (float)Math.Round(vector.z, decimals)
            );
    }

    public static Quaternion RoundToRectValue(this Quaternion quaternion)
    {
        Vector3 eulers = quaternion.eulerAngles;
        return Quaternion.Euler(
            Utils.RoundToRectValue(eulers.x),
            Utils.RoundToRectValue(eulers.y),
            Utils.RoundToRectValue(eulers.z));
    }
}

public static class Constants
{
    public const int GRID_SIZE = 4;
}
