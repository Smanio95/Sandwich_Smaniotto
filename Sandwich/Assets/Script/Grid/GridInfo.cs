using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GridInfo", menuName = "Info/Grid")]
public class GridInfo : ScriptableObject
{
    [SerializeField] Roteable roteablePrefab;
    [SerializeField] Roteable breadPrefab;
    [SerializeField] float offset = 1f;
    [SerializeField] int difficultyMultiplicator = 2;
    [SerializeField] List<Transform> ingredientMeshes;
    [SerializeField] string winText = "YOU WON";
    [SerializeField] string loseText = "YOU LOST";

    public Roteable RoteablePrefab { get => roteablePrefab; }
    public Roteable BreadPrefab { get => breadPrefab; }
    public float Offset { get => offset; }
    public int DifficultyMultiplicator { get => difficultyMultiplicator; }
    public List<Transform> IngredientMeshes { get => ingredientMeshes; }
    public string WinText { get => winText; }
    public string LoseText { get => loseText; }
}
