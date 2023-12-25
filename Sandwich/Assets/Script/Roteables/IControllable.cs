using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IControllable<T> where T : MonoBehaviour
{
    public void Reset();
    public void UpdateRef(T reference);
    public void Persist();
    public void Perform();
    public void Undo();
}
