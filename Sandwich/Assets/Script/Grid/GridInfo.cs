using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridInfo", menuName = "Info/Grid")]
public class GridInfo : ScriptableObject
{
    [SerializeField] Roteable roteablePrefab;
    [SerializeField] Bread breadPrefab;
    [SerializeField] float offset = 1f;
    [SerializeField] int difficultyMultiplicator = 2;
    [SerializeField] List<Transform> ingredientMeshes;
    [SerializeField] string winText = "YOU WON";
    [SerializeField] string loseText = "YOU LOST";
    [Header("Initial Animation")]
    [SerializeField] float startHeight = 8;
    [SerializeField] float placingDuration = 0.35f;
    [SerializeField] float placingDelay = 0.35f;
    [SerializeField] Ease placingEase;

    public Roteable RoteablePrefab { get => roteablePrefab; }
    public Bread BreadPrefab { get => breadPrefab; }
    public float Offset { get => offset; }
    public int DifficultyMultiplicator { get => difficultyMultiplicator; }
    public List<Transform> IngredientMeshes { get => ingredientMeshes; }
    public string WinText { get => winText; }
    public string LoseText { get => loseText; }
    public float StartHeight { get => startHeight; }
    public float PlacingDuration { get => placingDuration; }
    public float PlacingDelay { get => placingDelay; }
    public Ease PlacingEase { get => placingEase; }
}
