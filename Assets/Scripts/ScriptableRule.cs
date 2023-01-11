using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Rule", menuName = "WFC/Rule")]
public class ScriptableRule : ScriptableObject
{
    public TileBehaviour.TileType currentType;
    public Rule rule;


}

[Serializable]
public class Rule
{
    public enum Direction
    {
        Up,
        Right,
        Down,
        Left
    }
    [Serializable]
    public struct RuleData
    {
        public Direction direction;
        public List<TileBehaviour.TileType> validNeighbours;
    }
    // What are valid tiles at each direction from the current tile
    public List<RuleData> rules;
    
    public bool IsConnectionValid(Direction dir, TileBehaviour other)
    {
        return rules.Single(
            (d)=>d.direction==dir)
            .validNeighbours.Contains(other.tileType);
    }
    
    public List<TileBehaviour.TileType> GetValidNeighbours(Direction dir)
    {
        return rules.Single(
            (d)=>d.direction==dir)
            .validNeighbours;
    }

}