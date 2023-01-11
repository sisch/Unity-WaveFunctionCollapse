using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;
using Random = UnityEngine.Random;

public class Board : MonoBehaviour
{
    /// <summary>
    /// Board that contains tiles and empty spaces
    /// </summary>

    [SerializeField] private List<GameObject> availableTilePrefabs;
    [SerializeField] private Transform boardParent;

    [SerializeField] private int width = 3;
    [SerializeField] private int height = 3;
    
    private TileBehaviour[] _tileBehaviours;
    private List<TileBehaviour.TileType>[] _allowedTypes;
    [SerializeField] private  List<int> availableTilesIndices;
    
    [SerializeField] private  List<ScriptableRule> rules;
    
    public TileBehaviour GetTileAt(int x, int y)
    {
        return _tileBehaviours[x + y * width];
    }
    
    [SerializeField] private float interval = 3f;

    private float _timer = 3f;
    public void SetTileAt(int x, int y, TileBehaviour tile)
    {
        _tileBehaviours[x + y * width] = tile;
        availableTilesIndices.Remove(x + y * width);
    }
    
    public void SpawnTile(int x, int y)
    {
        LogTile(x+y*width);
        var allowedPrefabs = availableTilePrefabs
            .Where((prefab, index) => _allowedTypes[x+y*width].Contains(prefab.GetComponent<TileBehaviour>().tileType));
        var go = Instantiate(allowedPrefabs.PickRandom(), boardParent);
        go.transform.localPosition = new Vector3(x, 0, y);
        var tile = go.GetComponent<TileBehaviour>();
        SetTileAt(x, y, tile);
    }

    public void SpawnTile()
    {
        var randomIndex = Random.Range(0, availableTilesIndices.Count);
        var x = availableTilesIndices[randomIndex] % width;
        var y = availableTilesIndices[randomIndex] / width;
        SpawnTile(x, y);
    }

    public void ConstrainTile(int index)
    {
        int x = index % width;
        int y = index / width;
        var neighbours = new Dictionary<Rule.Direction, TileBehaviour>();
        if (x > 0)
        {
            var tile = GetTileAt(x - 1, y);
            if (tile != null)
            {
                neighbours.Add(Rule.Direction.Right, tile);
            }
        }
        if (x < width - 1)
        {
            var tile = GetTileAt(x + 1, y);
            if (tile != null)
            {
                neighbours.Add(Rule.Direction.Left, tile);
            }
        }
        if (y > 0)
        {
            var tile = GetTileAt(x, y - 1);
            if (tile != null)
            {
                neighbours.Add(Rule.Direction.Up, tile);
            }
        }
        if (y < height - 1)
        {
            var tile = GetTileAt(x, y + 1);
            if (tile != null)
            {
                neighbours.Add(Rule.Direction.Down, tile);
            }
        }

        var allowedTypes = _allowedTypes[index];
        foreach (var neighbour in neighbours)
        {
            var rule = rules.Find(r=>r.currentType == neighbour.Value.tileType);
            if (rule != null)
            {
                var validNeighbours = rule.rule.GetValidNeighbours(neighbour.Key);
                allowedTypes = allowedTypes.Intersect(validNeighbours).ToList();
            }
        }
        _allowedTypes[index] = allowedTypes;
    }

    int Cardinality(int index)
    {
        return _allowedTypes[index].Count;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        if(boardParent == null)
            boardParent = GameObject.Find("Board")?.transform;
        Init();
    }

    private void Init()
    {
        var usedTileTypes = 
            from prefab
                in availableTilePrefabs 
            select prefab.GetComponent<TileBehaviour>().tileType; 
        _tileBehaviours = new TileBehaviour[width * height];
        _allowedTypes = new List<TileBehaviour.TileType>[width * height];
        for (int i = 0; i < _allowedTypes.Length; i++)
        {
            _allowedTypes[i] = new List<TileBehaviour.TileType>(usedTileTypes);
        }

        availableTilesIndices = new List<int>();
        for (int i = 0; i < width * height; i++)
        {
            availableTilesIndices.Add(i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer > interval)
        {
            if(availableTilesIndices.Count <= 0)
            {
                return;
            }
            Tuple<int,int>[] cardinalities = new Tuple<int,int>[availableTilesIndices.Count];
            for (var index = 0; index < availableTilesIndices.Count; index++)
            {
                var tilesIndex = availableTilesIndices[index];
                ConstrainTile(tilesIndex);
                var cardinality = Cardinality(tilesIndex);
                cardinalities[index] = new Tuple<int, int>(cardinality, tilesIndex);
            }
            // determine one or more smallest cardinalities
            var smallestCardinality = cardinalities.Min(t => t.Item1);
            var validCardinalities = cardinalities.Where(t => t.Item1 == smallestCardinality).ToList();
            var nextTile = validCardinalities.PickRandom();
            
            var x = nextTile.Item2 % width;
            var y = nextTile.Item2 / width;
            Debug.Log($"Next tile is spawning at: {x}/{y}, because Cardinality is {nextTile.Item1}");
            _timer = 0f;
            SpawnTile(x, y);
        }
    }

    void LogTile(int index)
    {
        Debug.Log($"Tile at {index} has following allowed types: \n" +
                  $"{_allowedTypes[index].Select(t => t.ToString()).Aggregate((a, b) => a + ", " + b)}");
    }
    
}
