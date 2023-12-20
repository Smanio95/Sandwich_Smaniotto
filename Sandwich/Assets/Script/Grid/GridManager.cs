using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GridManager : MonoBehaviour
{
    [SerializeField] Roteable roteablePrefab;
    [SerializeField] Roteable breadPrefab;
    [SerializeField] float offset = 1f;
    [SerializeField] int difficultyMultiplicator = 2;
    [SerializeField] List<Transform> ingredientMeshes;
    [SerializeField] Slider difficultySlider;
    [SerializeField] GameObject endPanel;
    [SerializeField] TMP_Text endTxt;
    [SerializeField] string winText = "YOU WON";
    [SerializeField] string loseText = "YOU LOST";

    private readonly bool[,] cells = new bool[Constants.GRID_SIZE, Constants.GRID_SIZE];
    private readonly List<(int r, int c)> insertedLst = new();
    private readonly List<Roteable> roteableLst = new();

    public delegate void EndGame(Roteable caller, bool win);
    public static EndGame OnEndGame;

    private void Awake() => SetupValues();

    private void OnEnable()
    {
        OnEndGame += CheckWin;
    }

    public void SetupValues()
    {
        InstantiateGrid();

        insertedLst.Clear();

        foreach (Roteable r in roteableLst)
        {
            if (r != null) Destroy(r.gameObject);
        }

        roteableLst.Clear();
    }

    void InstantiateGrid()
    {
        for (int i = 0; i < Constants.GRID_SIZE; i++)
        {
            for (int j = 0; j < Constants.GRID_SIZE; j++)
            {
                cells[i, j] = false;
            }
        }
    }

    public void GenerateGrid()
    {
        InstantiateBreads();

        int nToSpawn = (int)difficultySlider.value * difficultyMultiplicator;

        InstantiateIngredients(nToSpawn);
    }

    void CheckWin(Roteable caller, bool win) => StartCoroutine(CheckWinDelayed(caller, win));

    IEnumerator CheckWinDelayed(Roteable caller, bool win)
    {
        yield return null;

        win = win && CheckOthers(caller);

        endTxt.text = win ? winText : loseText;

        endPanel.SetActive(true);
    }

    bool CheckOthers(Roteable caller)
    {
        foreach (Roteable r in roteableLst)
        {
            Debug.Log(r);
        }

        Debug.Log("---");

        List<Roteable> supp = roteableLst.FindAll(roteable => !ReferenceEquals(roteable, caller) && roteable != null);

        foreach (Roteable r in supp)
        {
            Debug.Log(r);
        }

        return roteableLst.FindAll(roteable => !roteable.Equals(caller) && roteable != null).Count == 0;
    }

    void InstantiateBreads()
    {
        (int r, int c) firstPos = (Random.Range(0, Constants.GRID_SIZE), Random.Range(0, Constants.GRID_SIZE));

        cells[firstPos.r, firstPos.c] = true;
        insertedLst.Add(firstPos);

        Roteable bread = Instantiate(breadPrefab, new(firstPos.r * offset, 0, firstPos.c * offset), Quaternion.identity, transform);
        bread.Setup(offset);
        roteableLst.Add(bread);

        (int r, int c) secondPos = GetRandom(FindAdiacent(firstPos.r, firstPos.c));

        cells[secondPos.r, secondPos.c] = true;
        insertedLst.Add(secondPos);

        bread = Instantiate(breadPrefab, new(secondPos.r * offset, 0, secondPos.c * offset), Quaternion.identity, transform);
        bread.Setup(offset);
        roteableLst.Add(bread);

    }

    void InstantiateIngredients(int n)
    {
        List<(int r, int c)> adiacents = null;

        for (int i = 0; i < n; i++)
        {
            while (adiacents == null || adiacents.Count == 0)
            {
                (int chosenR, int chosenC) = GetRandom(insertedLst);
                adiacents = FindAdiacent(chosenR, chosenC);
            }

            (int r, int c) = GetRandom(adiacents);

            cells[r, c] = true;
            insertedLst.Add((r, c));

            InstantiateIngredient(r, c);

            adiacents.Clear();
        }
    }

    void InstantiateIngredient(int r, int c)
    {
        if (ingredientMeshes.Count == 0) return;

        Roteable parent = Instantiate(roteablePrefab, new(r * offset, 0, c * offset), Quaternion.identity, transform);

        Transform randomIngredient = ingredientMeshes[Random.Range(0, ingredientMeshes.Count)];

        Transform mesh = Instantiate(randomIngredient);

        parent.Setup(offset, mesh);

        roteableLst.Add(parent);
    }

    List<(int r, int c)> FindAdiacent(int row, int col)
    {
        List<(int r, int c)> adiacentLst = new();

        for (int i = Mathf.Max(0, row - 1); i <= Mathf.Min(Constants.GRID_SIZE - 1, row + 1); i++)
        {
            for (int j = Mathf.Max(0, col - 1); j <= Mathf.Min(Constants.GRID_SIZE - 1, col + 1); j++)
            {
                if (!cells[i, j])
                {
                    adiacentLst.Add((i, j));
                }
            }
        }

        return adiacentLst;
    }

    (int r, int c) GetRandom(List<(int r, int c)> lst)
    {
        return lst[Random.Range(0, lst.Count)];
    }

    private void OnDisable()
    {
        OnEndGame -= CheckWin;
    }
}
