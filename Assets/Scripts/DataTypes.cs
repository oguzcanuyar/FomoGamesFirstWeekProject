
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Level
{
    public class BlockData
    {
        public int x;
        public int y;
        public BlockType type;
        public GameObject gameObject;
        public int index;
    }

    public class ExitData
    {
        public int x;
        public int y;
        public Direction direction;
        public BlockType type;
        public GameObject gameObject;
    }

    public class LevelData
    {
        public int width;
        public int height;
        public int time;
        public List<BlockData> blocks;
        public ExitData[] exits;
    }
        
}
public enum BlockType
{
    None = 0,
    Yellow = 1,
    Blue = 2,
    Green = 3,
    Red = 4,
    Orange = 5,
    Obstacle = 6
}

public enum Direction
{
    
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3
    
}