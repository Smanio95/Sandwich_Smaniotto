using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] LayerMask hitMask;

    Inputs inputs;

    private Roteable currentRoteable = null;

    private void Awake()
    {
        inputs = new();
    }

    private void OnEnable()
    {
        inputs.Enable();

        inputs.BaseInputs.StartTouch.started += ctx => ManageInput();
        inputs.BaseInputs.StartTouch.canceled += ctx => ResetInput();
        inputs.BaseInputs.EndTouch.performed += ctx => ResetInput();
        inputs.BaseInputs.EndTouch.canceled += ctx => ResetInput();
    }

    void ManageInput()
    {
        Vector2 mousePos = inputs.BaseInputs.Position.ReadValue<Vector2>();

        if (!HasHit(mousePos, out Roteable hit)) return;

        StartRotation(mousePos, hit);
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
           && (hit = raycastHit.collider.GetComponent<Roteable>()) && !hit.Repositionating;
    }

    void StartRotation(Vector2 startPos, Roteable hit)
    {
        currentRoteable = hit;

        currentRoteable.EnableSelf(startPos, inputs);
    }

    void ResetRotation()
    {
        currentRoteable.DisableSelf();

        currentRoteable = null;
    }

    private void OnDisable()
    {
        inputs.Disable();
    }
}
