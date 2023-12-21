using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoteableInfo", menuName = "Info/Roteable")]
public class RoteableInfo : ScriptableObject
{
    [SerializeField] LayerMask detectionMask;
    [SerializeField] float minSqrDistance = 700;
    [SerializeField] float rotationSpeed = 30;
    [SerializeField] float repositionateAngle = 60;
    [SerializeField] int breadWinCounter = 2;
    [SerializeField] float singleMeshHeight = 0.2f;

    public LayerMask DetectionMask { get => detectionMask; }
    public float MinSqrDistance { get => minSqrDistance; }
    public float RotationSpeed { get => rotationSpeed; }
    public float RepositionateAngle { get => repositionateAngle; }
    public int BreadWinCounter { get => breadWinCounter; }
    public float SingleMeshHeight { get => singleMeshHeight; }
}
