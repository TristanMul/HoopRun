using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
    Stores level's data that is needed to recreate level in the scene.
*/
[System.Serializable]
public class Level
{
    public LevelType type;

    public int baitID;

    [System.NonSerialized]
    public Bait bait;

    public Block[] blocks;

    public Block GetLevelBlock(int id){
        if(id >= 0 && id < blocks.Length){
            return blocks[id];
        }
        return null;
    }

    [System.Serializable]
    public class Block{
        public BlockType type;
        
        // Note that only strait line can contain obsticles and gems
        public int obstacleID;
        public int ammountOfObstacles;

        public int ammountOfPillars;

        public int gemPatternID;
        [System.NonSerialized]
        public GemPattern gemPattern;

        [System.NonSerialized]
        public Obstacle obstacle;
    }

    [System.Serializable]
    public enum  BlockType
    {
        START, FINISH, STRAIGHT_LINE, TURN_LEFT, TURN_RIGHT, RAILS, TRAMPLIN, DESCENDING_RAILS, ASCENDING_RAILS
    }

    [System.Serializable]
    public enum LevelType{
        JELLY_RUSH, GEM_RUSH
    }
}