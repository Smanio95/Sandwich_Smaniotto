using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Difficulty
{
    Easy = 1,
    Medium = 2,
    Hard = 3,
}

public class GridManager : MonoBehaviour
{
    [SerializeField] Roteable roteablePrefab;
    [SerializeField] Roteable breadPrefab;
    [SerializeField] float offset = 1f;
    [SerializeField] int difficultyMultiplicator = 2;

    [Header("Debug")]
    [SerializeField] int breadPos1;
    [SerializeField] int breadPos2;
    [SerializeField] int ingredientPos1;
    [SerializeField] int ingredientPos2;

    private readonly bool[,] cells = new bool[Constants.GRID_SIZE, Constants.GRID_SIZE];

    private void Start()
    {
        GenerateGrid(0);
    }

    public void GenerateGrid(int difficulty)
    {
        for (int i = 0; i < Constants.GRID_SIZE; i++)
        {
            for (int j = 0; j < Constants.GRID_SIZE; j++)
            {
                cells[i, j] = false;
            }
        }

        cells[breadPos1, breadPos1] = true;
        Instantiate(breadPrefab, new(breadPos1 * offset, 0, breadPos1 * offset), Quaternion.identity, transform);
        cells[breadPos2, breadPos2] = true;
        Instantiate(breadPrefab, new(breadPos2 * offset, 0, breadPos2 * offset), Quaternion.identity, transform);
        cells[ingredientPos1, ingredientPos1] = true;
        Instantiate(breadPrefab, new(ingredientPos1 * offset, 0, ingredientPos1 * offset), Quaternion.identity, transform);
        cells[ingredientPos1, breadPos2] = true;
        Instantiate(breadPrefab, new(ingredientPos1 * offset, 0, ingredientPos2 * offset), Quaternion.identity, transform);
    }
}
