using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bread : Roteable
{
    protected override void Awake()
    {
        base.Awake();

        type = IngredientType.Bread;

        InitializeChildren();

        winCounter++;
    }

    void InitializeChildren()
    {
        if (children.Count == 0)
        {
            if (transform.childCount > 0)
            {
                children.Add(new(type, transform.GetChild(0)));
            }
            else
            {
                Debug.LogError("this element has no children: " + name);
            }
        }
    }

    public void Setup(float offset) => InitializeDistances(offset);
}
