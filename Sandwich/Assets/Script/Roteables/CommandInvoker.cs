using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoteableCommandInfo
{
    public Vector3 oldRotationAxis;
    public Vector3 oldBCCenter;
    public Vector3 oldBCSize;
    public Roteable inglobedRoteable;
    public List<int> inglobedChildren;

    public RoteableCommandInfo()
    {
        inglobedChildren = new();
    }
}

public struct RoteableCommand
{
    public Roteable roteable;
    public RoteableCommandInfo roteableCommandInfo;

    public RoteableCommand(Roteable _roteable, RoteableCommandInfo _roteableCI)
    {
        roteable = _roteable;
        roteableCommandInfo = _roteableCI;
    }
}

public class CommandInvoker : IControllable<Roteable>
{
    private readonly Stack<RoteableCommand> commandsList = new();
    private Roteable currentRoteable;

    private readonly Inputs inputs;

    public CommandInvoker(Inputs _inputs) => inputs = _inputs;

    public void UpdateRef(Roteable roteable)
    {
        currentRoteable = roteable;

        currentRoteable.EnableSelf(inputs.BaseInputs.Position.ReadValue<Vector2>());
    }
    public void Persist()
    {
        if (currentRoteable)
        {
            currentRoteable.ManageRotation(inputs.BaseInputs.Position.ReadValue<Vector2>());
        }
    }

    public void Perform()
    {
        currentRoteable.DisableSelf(out RoteableCommandInfo info);

        if (info != null)
        {
            commandsList.Push(new(currentRoteable, info));
        }

        currentRoteable = null;
    }


    public void Undo()
    {
        if (commandsList.Count == 0) return;

        RoteableCommand lastInfo = commandsList.Pop();

        lastInfo.roteable.Undo(lastInfo.roteableCommandInfo);
    }

    public void Reset() => commandsList.Clear();
}
