using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Axis
{
    All,
    X,
    Y,
    Z
}

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

    public static Vector3 Round(Vector3 vector, int decimals, Axis axis = Axis.All)
    {
        return new(
                (float)Math.Round(vector.x, (axis == Axis.X || axis == Axis.All) ? decimals : 0),
                (float)Math.Round(vector.y, (axis == Axis.Y || axis == Axis.All) ? decimals : 0),
                (float)Math.Round(vector.z, (axis == Axis.Z || axis == Axis.All) ? decimals : 0)
                );
    }

    public static Quaternion RoundToRectValue(Quaternion quaternion)
    {
        Vector3 eulers = quaternion.eulerAngles;
        return Quaternion.Euler(
            RoundToRectValue(eulers.x),
            RoundToRectValue(eulers.y),
            RoundToRectValue(eulers.z));
    }

    public static List<T> CopyLst<T>(List<T> lst)
    {
        List<T> newLst = new();

        foreach(T obj in lst)
        {
            newLst.Add(obj);
        }

        return newLst;
    }
}

public static class Constants
{
    public const int GRID_SIZE = 4;
}
