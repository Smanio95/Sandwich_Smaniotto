using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    [SerializeField] LayerMask hitMask;
    [SerializeField] Button undoBtn;

    Inputs inputs;

    private IControllable<Roteable> invoker;
    private Roteable currentRoteable = null;
    private int repositionatingRoteables = 0;

    private void Awake()
    {
        inputs = new();
        invoker = new CommandInvoker(inputs);
    }

    private void OnEnable()
    {
        GridManager.OnPlaced += EnableInputs;
        Roteable.OnRepositionate += BtnEnabling;

        inputs.BaseInputs.StartTouch.started += ctx => ManageInput();
        inputs.BaseInputs.StartTouch.canceled += ctx => ResetInput();
        inputs.BaseInputs.EndTouch.performed += ctx => ResetInput();
        inputs.BaseInputs.EndTouch.canceled += ctx => ResetInput();
    }

    void EnableInputs() => inputs.Enable();

    public void ResetInvoker() => invoker.Reset();

    void ManageInput()
    {
        Vector2 mousePos = inputs.BaseInputs.Position.ReadValue<Vector2>();

        if (!HasHit(mousePos, out Roteable hit)) return;

        StartRotation(hit);
    }

    void ResetInput()
    {
        if (currentRoteable) ResetRotation();
    }

    bool HasHit(Vector2 mousePos, out Roteable hit)
    {
        hit = null;

        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        Debug.DrawRay(ray.origin, ray.direction * 100, Color.blue, 10);

        return Physics.Raycast(ray, out RaycastHit raycastHit, Camera.main.farClipPlane, hitMask)
           && (hit = raycastHit.collider.GetComponent<Roteable>()) 
           && !hit.Repositionating;
    }

    void StartRotation(Roteable hit)
    {
        currentRoteable = hit;

        invoker.UpdateRef(currentRoteable);

    }

    void BtnEnabling(bool start)
    {
        if (start)
        {
            repositionatingRoteables++;
        }
        else
        {
            repositionatingRoteables--;
        }
    }

    private void Update() => undoBtn.interactable = repositionatingRoteables == 0;

    private void FixedUpdate() => invoker.Persist();

    void ResetRotation()
    {
        invoker.Perform();

        currentRoteable = null;
    }

    public void Undo() => invoker.Undo();

    private void OnDisable()
    {
        inputs.Disable();

        GridManager.OnPlaced -= EnableInputs;
        Roteable.OnRepositionate -= BtnEnabling;
    }
}
