using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct AxisInfo
{
    public Vector3 axis;
    public bool isDiag;

    public AxisInfo(Vector3 _axis, bool _isDiag)
    {
        axis = _axis;
        isDiag = _isDiag;
    }
}

[RequireComponent(typeof(BoxCollider))]
public class Roteable : MonoBehaviour
{
    [Header("Functionality")]
    [SerializeField] float minSqrDistance = 700;
    [SerializeField] BoxCollider boxCollider;
    [SerializeField] float rotationSpeed = 30;
    [SerializeField] float repositionateAngle = 60;

    public bool Repositionating { get => repositionating; }
    bool repositionating = false;

    public virtual float Height { get => transform.localScale.y; }

    private float sideDistance;
    private float diagDistance;
    private readonly List<Vector3> axisPoints = new();

    private Inputs inputs;
    private Vector3 initialPos;

    private Vector2 startPos;
    private Vector2 lastPos;
    private Vector3 direction;
    private Vector3 rotationAxis;
    private float rotatedAngle = 0;

    private bool hasDirection = false;

    private Roteable child = null;

    private void Awake()
    {
        if (!boxCollider) boxCollider = GetComponent<BoxCollider>();

        sideDistance = boxCollider.size.x / 2;
        diagDistance = Mathf.Sqrt((sideDistance * sideDistance) + (sideDistance * sideDistance));

        GenerateAxisPoints();
    }

    void GenerateAxisPoints()
    {
        for (int i = 0; i <= 360; i += 45)
        {
            axisPoints.Add(Quaternion.AngleAxis(i, Vector3.up) * transform.forward);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(i, Vector3.up) * transform.forward * 100, Color.cyan, 5);
        }
    }

    public void EnableSelf(Vector2 _startPos, Inputs _input)
    {
        enabled = true;

        startPos = _startPos;
        lastPos = startPos;
        initialPos = transform.position;
        initialPos.y = Height;

        rotatedAngle = 0;

        if (inputs == null) inputs = _input;

        hasDirection = false;

        child = null;
    }

    private void FixedUpdate() => ManageRotation(inputs.BaseInputs.Position.ReadValue<Vector2>());

    void ManageRotation(Vector2 posValue)
    {
        if (repositionating) return;

        if (hasDirection)
        {
            if ((posValue - lastPos).sqrMagnitude > 0)
            {
                Rotate(posValue);
            }
            return;
        }

        if (Vector3.SqrMagnitude(startPos - posValue) < minSqrDistance) return;

        hasDirection = true;

        direction = (posValue - startPos).normalized;

        rotationAxis = CalculateAxis(direction);

        lastPos = posValue;
    }

    bool askCompletion = false;
    void Rotate(Vector2 posValue)
    {
        float delta = Vector3.Dot(direction, (posValue - lastPos).normalized) > 0 ? -1 : 1;

        float partialRot = rotatedAngle - delta * rotationSpeed * Time.fixedDeltaTime;

        if(partialRot < repositionateAngle && partialRot > 0)
        {
            askCompletion = false;

            transform.RotateAround(initialPos + rotationAxis, Vector3.Cross(rotationAxis, Vector3.up), delta * rotationSpeed * Time.fixedDeltaTime);

            lastPos = posValue;

            rotatedAngle = partialRot;
        }
        else if(partialRot >= repositionateAngle)
        {
            askCompletion = true;
        }
    }

    IEnumerator CompleteRotation(bool toMin, Action finalAction)
    {
        repositionating = true;

        float lastAngle = 0;

        float delta = toMin ? 1 : -1;

        while (toMin ? rotatedAngle > 0 : rotatedAngle < 180f)
        {
            lastAngle = rotatedAngle;

            yield return null;

            transform.RotateAround(initialPos + rotationAxis, Vector3.Cross(rotationAxis, Vector3.up), delta * rotationSpeed * Time.deltaTime);

            rotatedAngle -= delta * rotationSpeed * Time.deltaTime;
        }

        Vector3 eulers = transform.rotation.eulerAngles;
        transform.rotation = Quaternion.Euler(
            Utils.RoundToRectValue(eulers.x), 
            Utils.RoundToRectValue(eulers.y), 
            Utils.RoundToRectValue(eulers.z));

        repositionating = false;

        finalAction?.Invoke();
    }

    Vector3 CalculateAxis(Vector2 dir)
    {
        AxisInfo axisPoint = RetrievePoint(dir);

        float magnitude = axisPoint.isDiag ? diagDistance : sideDistance;

        Debug.DrawRay(transform.position + axisPoint.axis * magnitude, Vector3.Cross(axisPoint.axis * magnitude, Vector3.up) * 100, Color.red, 10);

        return axisPoint.axis * magnitude;
    }

    AxisInfo RetrievePoint(Vector2 dir)
    {
        AxisInfo axisPoint = new();
        float alignance = Mathf.Infinity;

        bool isDiag = false;

        foreach (Vector3 vector in axisPoints)
        {
            float partialAlign = 1 - Vector3.Dot(new(vector.x, vector.z), dir);
            if (partialAlign < alignance)
            {
                alignance = partialAlign;
                //axisPoint = new(new(vector.x, Height / 2, vector.z), isDiag);
                axisPoint = new(vector, isDiag);
            }

            isDiag = !isDiag;
        }

        return axisPoint;
    }

    public void DisableSelf() => StartCoroutine(CompleteRotation(!askCompletion, () => enabled = false));

}
