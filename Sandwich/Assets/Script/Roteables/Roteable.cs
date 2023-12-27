using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] protected IngredientType type;

    [Header("Functionality")]
    [SerializeField] RoteableInfo info;

    [Header("Refs")]
    [SerializeField] BoxCollider boxCollider;
    public List<MeshInfo> children = new();

    public bool Repositionating { get => repositionating; }
    bool repositionating = false;
    public float Height { get => info.SingleMeshHeight * children.Count; }

    private float sideDistance;
    private float diagDistance;
    private readonly List<Vector3> axisPoints = new();

    private Vector3 initialPos;
    private Vector3 rotationPos;
    private Quaternion initialRot;

    private Vector2 startPos;
    private Vector2 lastPos;
    private Vector3 direction;
    private Vector3 rotationAxis;
    private float rotatedAngle = 0;

    protected int winCounter = 0;
    private bool hasDirection = false;
    private bool askCompletion = false;

    private Roteable nextChild = null;

    public delegate void Repositionate(bool start);
    public static Repositionate OnRepositionate;

    protected virtual void Awake()
    {
        enabled = false;

        if (!boxCollider) boxCollider = GetComponent<BoxCollider>();

        GenerateAxisPoints();
    }

    void GenerateAxisPoints()
    {
        for (int i = 0; i <= 360; i += 45)
        {
            axisPoints.Add(Quaternion.AngleAxis(i, Vector3.up) * transform.forward);
        }
    }

    public void Setup(float offset, Transform firstChild)
    {
        firstChild.parent = transform;

        firstChild.localPosition = Vector3.zero;

        children.Add(new(type, firstChild));

        InitializeDistances(offset);
    }

    protected void InitializeDistances(float offset)
    {
        sideDistance = offset / 2;
        diagDistance = Mathf.Sqrt((sideDistance * sideDistance) + (sideDistance * sideDistance));
    }

    public void EnableSelf(Vector2 _startPos)
    {
        enabled = true;

        startPos = _startPos;
        lastPos = startPos;

        initialPos = transform.position;
        initialRot = transform.rotation;
        rotationPos = initialPos;

        rotatedAngle = 0;

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

    public void ManageRotation(Vector2 posValue)
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
        if (nextChild) rotationPos.y = (Height + nextChild.Height - info.SingleMeshHeight) / 2;

        lastPos = posValue;
    }

    void Rotate(Vector2 posValue)
    {
        float delta = Vector3.Dot(direction, (posValue - lastPos).normalized) > 0 ? -1 : 1;

        float partialRot = rotatedAngle - delta * info.RotationSpeed * Time.fixedDeltaTime;

        if (partialRot < info.RepositionateAngle && partialRot > 0)
        {
            askCompletion = false;

            transform.RotateAround(rotationPos + rotationAxis, Vector3.Cross(rotationAxis, Vector3.up), delta * info.RotationSpeed * Time.fixedDeltaTime);

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
        OnRepositionate?.Invoke(repositionating);

        float lastAngle = 0;

        float delta = toMin ? 1 : -1;

        while (toMin ? rotatedAngle > 0 : rotatedAngle < 180f)
        {
            lastAngle = rotatedAngle;

            yield return null;

            transform.RotateAround(rotationPos + rotationAxis, Vector3.Cross(rotationAxis, Vector3.up), delta * info.RotationSpeed * Time.deltaTime);

            rotatedAngle -= delta * info.RotationSpeed * Time.deltaTime;

            //Debug.DrawRay(rotationPos + rotationAxis, Vector3.Cross(rotationAxis, Vector3.up) * 100, Color.red, 10);
        }

        if (toMin)
        {
            transform.SetPositionAndRotation(
                initialPos,
                initialRot);
        }
        else
        {
            transform.rotation = Utils.RoundToRectValue(transform.rotation);
        }

        repositionating = false;
        OnRepositionate?.Invoke(repositionating);

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
        float modifiedHeight = ((Height - (info.SingleMeshHeight / 2)) / children.Count) / 2; // the lowest mesh center to raycast 

        Vector3 origin = new(initialPos.x, modifiedHeight, initialPos.z);

        //Debug.DrawLine(origin, origin + direction * 5, Color.magenta, 5);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, sideDistance * 2, info.DetectionMask))
        {
            return hit.collider.GetComponent<Roteable>();
        }

        return null;
    }

    public void DisableSelf(out RoteableCommandInfo commandInfo)
    {
        commandInfo = null;

        if (nextChild == null || !askCompletion)
        {
            StartCoroutine(CompleteRotation(true, () => enabled = false));
        }
        else
        {
            commandInfo = ValorizeCommandInfo();

            (float collCenter, float collSize) = Inglobe(commandInfo);

            StartCoroutine(CompleteRotation(false, () => EndInglobation(collCenter, collSize)));
        }
    }

    RoteableCommandInfo ValorizeCommandInfo()
    {
        RoteableCommandInfo commandInfo = new();

        commandInfo.inglobedRoteable = nextChild;
        commandInfo.oldBCCenter = boxCollider.center;
        commandInfo.oldBCSize = boxCollider.size;
        commandInfo.oldRotationAxis = rotationAxis;

        return commandInfo;
    }

    public (float collCenter, float collSize) Inglobe(RoteableCommandInfo commandInfo)
    {
        float collSize = boxCollider.size.y,
              collCenter = Mathf.Abs(boxCollider.center.y);

        foreach (MeshInfo child in nextChild.children)
        {
            commandInfo.inglobedChildren.Add(children.Count);

            collSize += (float)Math.Round(child.mesh.localScale.y, 1);
            collCenter += (float)Math.Round(child.mesh.localScale.y / 2, 1);

            children.Add(child);

            if (child.type == IngredientType.Bread) winCounter++;
        }

        return (collCenter, collSize);
    }

    public void EndInglobation(float collCenter, float collSize)
    {
        foreach (MeshInfo child in nextChild.children)
        {
            child.mesh.parent = transform;
        }

        boxCollider.center = Vector3.Dot(Vector3.up, transform.up) < 0
        ? collCenter * Vector3.up
        : -collCenter * Vector3.up;

        boxCollider.size = new(boxCollider.size.x, collSize, boxCollider.size.z);

        ResetPositions();

        nextChild.gameObject.SetActive(false);

        CheckWin();

        enabled = false;
    }

    public void Undo(RoteableCommandInfo commandInfo)
    {
        rotatedAngle = 0;

        rotationPos = new(transform.position.x,
                        (Height - info.SingleMeshHeight / 2) / 2,
                        transform.position.z);

        rotationAxis = -commandInfo.oldRotationAxis;

        Excorporate(commandInfo);

        StartCoroutine(CompleteRotation(false, ResetPositions));
    }

    void Excorporate(RoteableCommandInfo info)
    {
        List<MeshInfo> newChildren = new();

        for (int i = 0; i < children.Count; i++)
        {
            if (!info.inglobedChildren.Contains(i))
            {
                newChildren.Add(children[i]);
            }
            else if (children[i].type == IngredientType.Bread)
            {
                winCounter--;
            }
        }

        children = newChildren;

        boxCollider.center = info.oldBCCenter;
        boxCollider.size = info.oldBCSize;

        info.inglobedRoteable.Resurrect();
    }

    void Resurrect()
    {
        foreach (MeshInfo info in children)
        {
            info.mesh.parent = transform;
        }

        gameObject.SetActive(true);
    }

    void ResetPositions()
    {
        transform.position = Utils.Round(
            new(
            transform.position.x,
            Height - info.SingleMeshHeight,
            transform.position.z),
            1,
            Axis.Y);

        UpdatePositions();
    }

    void UpdatePositions()
    {
        List<MeshInfo> sortedLst = children.OrderBy(a => a.mesh.position.y).ToList();

        for (int i = 0; i < sortedLst.Count; i++)
        {
            sortedLst[i].mesh.position = Utils.Round(new(
                sortedLst[i].mesh.position.x,
                info.SingleMeshHeight * i,
                sortedLst[i].mesh.position.z),
                1,
                Axis.Y);
        }
    }
}
