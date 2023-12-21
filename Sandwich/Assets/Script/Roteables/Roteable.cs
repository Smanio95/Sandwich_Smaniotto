using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum IngredientType
{
    Ingredient,
    Bread
}

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

[System.Serializable]
public struct MeshInfo
{
    public IngredientType type;
    public Transform mesh;

    public MeshInfo(IngredientType _type, Transform _mesh)
    {
        type = _type;
        mesh = _mesh;
    }
}

[RequireComponent(typeof(BoxCollider))]
public class Roteable : MonoBehaviour
{
    [Header("Ingredient Type")]
    [SerializeField] IngredientType type;

    [Header("Functionality")]
    //[SerializeField] LayerMask detectionMask;
    //[SerializeField] float minSqrDistance = 700;
    //[SerializeField] float rotationSpeed = 30;
    //[SerializeField] float repositionateAngle = 60;
    //[SerializeField] int breadWinCounter = 2;
    [SerializeField] RoteableInfo info;

    [Header("Refs")]
    [SerializeField] BoxCollider boxCollider;
    public List<MeshInfo> children = new();

    public bool Repositionating { get => repositionating; }
    bool repositionating = false;

    public float Height
    {
        get
        {
            float height = 0;

            foreach (MeshInfo meshInfo in children) height += info.SingleMeshHeight;

            return height;
        }
    }

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
    private bool askCompletion = false;
    private int winCounter = 0;

    private Roteable nextChild = null;

    private void Awake()
    {
        if (!boxCollider) boxCollider = GetComponent<BoxCollider>();

        if (type == IngredientType.Bread) winCounter++;

        GenerateAxisPoints();
    }

    void GenerateAxisPoints()
    {
        for (int i = 0; i <= 360; i += 45)
        {
            axisPoints.Add(Quaternion.AngleAxis(i, Vector3.up) * transform.forward);
        }
    }

    public void Setup(float offset, Transform firstChild = null)
    {
        if (firstChild)
        {
            firstChild.parent = transform;

            firstChild.localPosition = Vector3.zero;

            children.Add(new(type, firstChild));
        }

        sideDistance = offset / 2;
        diagDistance = Mathf.Sqrt((sideDistance * sideDistance) + (sideDistance * sideDistance));
    }

    public void EnableSelf(Vector2 _startPos, Inputs _input)
    {
        enabled = true;

        startPos = _startPos;
        lastPos = startPos;
        initialPos = transform.position;

        rotatedAngle = 0;

        if (inputs == null) inputs = _input;

        hasDirection = false;

        nextChild = null;
    }

    private void CheckWin()
    {
        if (winCounter < info.BreadWinCounter) return;

        MeshInfo highest = new();
        MeshInfo lowest = new();

        foreach (MeshInfo info in children)
        {
            if (highest.mesh == null || highest.mesh.position.y > info.mesh.position.y)
            {
                highest = new(info.type, info.mesh);
            }

            if (lowest.mesh == null || lowest.mesh.position.y < info.mesh.position.y)
            {
                lowest = new(info.type, info.mesh);
            }
        }

        GridManager.OnEndGame?.Invoke(this, lowest.type == highest.type && highest.type == IngredientType.Bread);
    }

    private void FixedUpdate() => ManageRotation(inputs.BaseInputs.Position.ReadValue<Vector2>());

    void ManageRotation(Vector2 posValue)
    {
        if (winCounter >= info.BreadWinCounter || repositionating) return;

        if (hasDirection)
        {
            if ((posValue - lastPos).sqrMagnitude > 0)
            {
                Rotate(posValue);
            }
            return;
        }

        if (Vector3.SqrMagnitude(startPos - posValue) < info.MinSqrDistance) return;

        hasDirection = true;

        direction = (posValue - startPos).normalized;

        rotationAxis = CalculateAxis(direction);

        nextChild = RetrieveNextChild(rotationAxis);

        // we adjust the height of the rotationAxis: the middle point is found through my height and the height of the object 
        // that we will inglobe. We must then watch the global position of the whole meshes, that we know they starts
        // at height 0 and subtract half of a single mesh for both myself and the object to inglobe in 
        // order to consider the position of the pivot of the single meshes (positioned in the middle of a single mesh that is built to have 0.2f of height).
        // We then substract by 2 to retrieve the center of rotation.
        if (nextChild) initialPos.y = (Height + nextChild.Height - info.SingleMeshHeight) / 2;

        lastPos = posValue;
    }

    void Rotate(Vector2 posValue)
    {
        float delta = Vector3.Dot(direction, (posValue - lastPos).normalized) > 0 ? -1 : 1;

        float partialRot = rotatedAngle - delta * info.RotationSpeed * Time.fixedDeltaTime;

        if (partialRot < info.RepositionateAngle && partialRot > 0)
        {
            askCompletion = false;

            transform.RotateAround(initialPos + rotationAxis, Vector3.Cross(rotationAxis, Vector3.up), delta * info.RotationSpeed * Time.fixedDeltaTime);

            lastPos = posValue;

            rotatedAngle = partialRot;
        }
        else if (partialRot >= info.RepositionateAngle)
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

            transform.RotateAround(initialPos + rotationAxis, Vector3.Cross(rotationAxis, Vector3.up), delta * info.RotationSpeed * Time.deltaTime);

            Debug.DrawRay(initialPos + rotationAxis, Vector3.Cross(rotationAxis, Vector3.up) * 100, Color.red, 10);

            rotatedAngle -= delta * info.RotationSpeed * Time.deltaTime;
        }

        transform.rotation = transform.rotation.RoundToRectValue();

        transform.position = transform.position.Round(1);

        repositionating = false;

        finalAction?.Invoke();
    }

    Vector3 CalculateAxis(Vector2 dir)
    {
        AxisInfo axisPoint = RetrievePoint(dir);

        float magnitude = axisPoint.isDiag ? diagDistance : sideDistance;

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
                axisPoint = new(vector, isDiag);
            }

            isDiag = !isDiag;
        }

        return axisPoint;
    }

    Roteable RetrieveNextChild(Vector3 direction)
    {
        float modifiedHeight = (Height / children.Count) / 2; // the lowest mesh center to raycast 

        Vector3 origin = new(initialPos.x, modifiedHeight, initialPos.z);

        Debug.DrawLine(origin, origin + direction * 5, Color.magenta, 5);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, sideDistance * 2, info.DetectionMask))
        {
            return hit.collider.GetComponent<Roteable>();
        }

        return null;
    }

    public void DisableSelf()
    {
        if (nextChild == null || !askCompletion)
        {
            StartCoroutine(CompleteRotation(true, () => enabled = false));
        }
        else
        {
            StartCoroutine(CompleteRotation(false, Inglobe));
        }

    }

    public void Inglobe()
    {
        float collSize = boxCollider.size.y,
              collCenter = Mathf.Abs(boxCollider.center.y);

        foreach (MeshInfo child in nextChild.children)
        {
            child.mesh.parent = transform;

            collSize += (float)Math.Round(child.mesh.localScale.y, 1);
            collCenter += (float)Math.Round(child.mesh.localScale.y / 2, 1);

            children.Add(child);

            if (child.type == IngredientType.Bread) winCounter++;
        }

        boxCollider.center = Vector3.Dot(Vector3.up, transform.up) < 0
            ? collCenter * Vector3.up
            : -collCenter * Vector3.up;

        boxCollider.size = new(boxCollider.size.x, collSize, boxCollider.size.z);

        ResetPositions();

        Destroy(nextChild.gameObject);

        CheckWin();

        enabled = false;
    }

    void ResetPositions()
    {
        List<Vector3> positionsCopy = CopyPositions(children);

        transform.position = new(
            transform.position.x,
            Height - info.SingleMeshHeight,
            transform.position.z);

        UpdatePositions(positionsCopy);
    }

    List<Vector3> CopyPositions(List<MeshInfo> infos)
    {
        List<Vector3> copyLst = new();
        foreach (MeshInfo i in infos) copyLst.Add(i.mesh.position);
        return copyLst;
    }

    void UpdatePositions(List<Vector3> positionsCopy)
    {
        for (int i = 0; i < positionsCopy.Count; i++)
        {
            children[i].mesh.position = positionsCopy[i];
        }
    }

}
