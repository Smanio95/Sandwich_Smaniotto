using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roteable : MonoBehaviour
{
    Inputs inputs;
    Vector2 startPos;

    public void EnableSelf(Vector2 _startPos, Inputs _input)
    {
        enabled = true;

        startPos = _startPos;

        if(inputs == null) inputs = _input;
    }

    private void FixedUpdate() => Rotate(inputs.BaseInputs.Position.ReadValue<Vector2>());

    void Rotate(Vector2 posValue)
    {
        Vector2 delta = posValue - startPos;

        if(delta.magnitude > 0f) Debug.Log(delta);

        startPos = posValue;
    }

    public void DisableSelf()
    {
        enabled = false;
    }
}
