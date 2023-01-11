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
    ///
    /// TODO: Add allowedTypes for every tile as matrix
    /// TODO: Remove allowedTypes from cell instead of determining cardinality as number
    /// </summary>
    
    public List<GameObject> availableTilePrefabs;
    public Transform boardParent;

    public int width = 3;
    public int height = 3;
    
    public TileBehaviour[] tileBehaviours;

    public List<int> availableTilesIndices;
    
    public List<ScriptableRule> rules;
    
    public TileBehaviour GetTileAt(int x, int y)
    {
        return tileBehaviours[x + y * width];
    }
    
    public void SetTileAt(int x, int y, TileBehaviour tile)
    {
        tileBehaviours[x + y * width] = tile;
        availableTilesIndices.Remove(x + y * width);
    }
    
    public void SpawnTile(int x, int y)
    {
        var go = Instantiate(availableTilePrefabs.PickRandom(), boardParent);
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

    public int DetermineCardinality(int index)
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

        var allowedTypes = new List<TileBehaviour.TileType>();
        allowedTypes.Add(TileBehaviour.TileType.Empty);
        allowedTypes.Add(TileBehaviour.TileType.North);
        allowedTypes.Add(TileBehaviour.TileType.East);
        allowedTypes.Add(TileBehaviour.TileType.South);
        allowedTypes.Add(TileBehaviour.TileType.West);

        foreach (var neighbour in neighbours)
        {
            var rule = rules.Find(r=>r.currentType == neighbour.Value.tileType);
            if (rule != null)
            {
                var validNeighbours = rule.rule.GetValidNeighbours(neighbour.Key);
                allowedTypes = allowedTypes.Intersect(validNeighbours).ToList();
            }
        }

        return allowedTypes.Count;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        tileBehaviours = new TileBehaviour[width * height];
        availableTilesIndices = new List<int>();
        for (int i = 0; i < width * height; i++)
        {
            availableTilesIndices.Add(i);
        }
        
        /*for (int i = 0; i < 5; i++)
        {
            SpawnTile();
        }*/

    }

    private float interval = 3f;

    private float timer = 3f;
    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > interval)
        {
            if(availableTilesIndices.Count <= 0)
            {
                return;
            }
            Tuple<int,int>[] cardinalities = new Tuple<int,int>[availableTilesIndices.Count];
            for (var index = 0; index < availableTilesIndices.Count; index++)
            {
                var tilesIndex = availableTilesIndices[index];
                var cardinality = DetermineCardinality(tilesIndex);
                cardinalities[index] = new Tuple<int, int>(cardinality, tilesIndex);
            }
            // determine one or more smallest cardinalities
            var smallestCardinality = cardinalities.Min(t => t.Item1);
            var validCardinalities = cardinalities.Where(t => t.Item1 == smallestCardinality).ToList();
            var nextTile = validCardinalities.PickRandom();
            
            var x = nextTile.Item2 % width;
            var y = nextTile.Item2 / width;
            Debug.Log($"Next tile is spawning at: {x}/{y}, because Cardinality is {nextTile.Item1}");
            timer = 0f;
            SpawnTile(x, y);
        }
        
    }
}
