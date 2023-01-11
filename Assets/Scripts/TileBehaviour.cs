using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBehaviour : MonoBehaviour
{
    public enum TileType
    {
        Empty,
        UpLeftRight,
        UpRightDown,
        RightDownLeft,
        DownLeftUp,
        LeftRight,
        UpDown,
    }
    
    public TileType tileType;
    

}
