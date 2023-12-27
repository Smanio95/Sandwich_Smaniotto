using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public struct InsertedInfo
{
    public Roteable inserted;
    public (int r, int c) pos;

    public InsertedInfo(Roteable _inserted, (int r, int c) _pos)
    {
        inserted = _inserted;
        pos = _pos;
    }
}

public class GridManager : MonoBehaviour
{
    [SerializeField] GridInfo info;
    [Header("Refs")]
    [SerializeField] Slider difficultySlider;
    [SerializeField] TMP_Text[] difficultyTxts;
    [SerializeField] GameObject endPanel;
    [SerializeField] TMP_Text endTxt;

    private readonly bool[,] cells = new bool[Constants.GRID_SIZE, Constants.GRID_SIZE];
    private readonly List<InsertedInfo> insertedList = new();

    public delegate void Placed();
    public static Placed OnPlaced;

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

        foreach (InsertedInfo r in insertedList)
        {
            if (r.inserted != null) Destroy(r.inserted.gameObject);
        }

        insertedList.Clear();

        ValorizeDifficultyTxts();
    }

    void ValorizeDifficultyTxts()
    {
        for (int i = 0; i < difficultyTxts.Length; i++)
        {
            difficultyTxts[i].text = ((i + 1) * info.DifficultyMultiplicator).ToString();
        }
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

        int nToSpawn = (int)difficultySlider.value * info.DifficultyMultiplicator;

        InstantiateIngredients(nToSpawn);

        StartCoroutine(PlaceElements());
    }

    void CheckWin(Roteable caller, bool win)
    {
        win = win && CheckOthers(caller);

        endTxt.text = win ? info.WinText : info.LoseText;

        endPanel.SetActive(true);
    }

    bool CheckOthers(Roteable caller)
    {
        return insertedList.FindAll(
            insertedInfo => !insertedInfo.inserted.Equals(caller)
            && insertedInfo.inserted.gameObject.activeSelf
            ).Count == 0;
    }

    void InstantiateBreads()
    {
        (int r, int c) firstPos = (Random.Range(0, Constants.GRID_SIZE), Random.Range(0, Constants.GRID_SIZE));

        InstantiateBread(firstPos);

        (int r, int c) secondPos = GetRandom(FindAdiacent(firstPos.r, firstPos.c));

        InstantiateBread(secondPos);
    }

    void InstantiateBread((int r, int c) pos)
    {
        cells[pos.r, pos.c] = true;

        Bread bread = Instantiate(info.BreadPrefab, new(pos.r * info.Offset, info.StartHeight, pos.c * info.Offset), Quaternion.identity, transform);
        bread.Setup(info.Offset);

        insertedList.Add(new(bread, pos));
    }

    void InstantiateIngredients(int n)
    {
        List<(int r, int c)> adiacents = null;

        for (int i = 0; i < n; i++)
        {
            List<(int r, int c)> insertedPos = insertedList.Select(elem => elem.pos).ToList();

            while (adiacents == null || adiacents.Count == 0)
            {
                (int chosenR, int chosenC) = GetRandom(insertedPos);
                adiacents = FindAdiacent(chosenR, chosenC);
            }

            (int r, int c) = GetRandom(adiacents);

            InstantiateIngredient(r, c);

            adiacents.Clear();
        }
    }

    void InstantiateIngredient(int r, int c)
    {
        if (info.IngredientMeshes.Count == 0) return;

        cells[r, c] = true;

        Roteable parent = Instantiate(info.RoteablePrefab, new(r * info.Offset, info.StartHeight, c * info.Offset), Quaternion.identity, transform);

        Transform randomIngredient = info.IngredientMeshes[Random.Range(0, info.IngredientMeshes.Count)];

        Transform mesh = Instantiate(randomIngredient);

        parent.Setup(info.Offset, mesh);

        //roteableLst.Add(parent);

        insertedList.Add(new(parent, (r, c)));
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

    IEnumerator PlaceElements()
    {
        for (int i = 0; i < insertedList.Count; i++)
        {
            Sequence seq = DOTween.Sequence();
            seq.Append(insertedList[i].inserted.transform.DOMoveY(0, info.PlacingDuration).SetEase(info.PlacingEase));

            if (i == insertedList.Count - 1)
            {
                seq.AppendCallback(() => OnPlaced?.Invoke());
            }

            yield return new WaitForSeconds(info.PlacingDelay);
        }
    }

    private void OnDisable()
    {
        OnEndGame -= CheckWin;
    }
}
