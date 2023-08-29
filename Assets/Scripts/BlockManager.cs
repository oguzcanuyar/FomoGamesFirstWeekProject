using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class BlockManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [HideInInspector] public Level.LevelData levelData;
    [SerializeField] private float blockDist;
    [SerializeField] private List<Color32> blockColors;
    [SerializeField] private GameObject blockPrefab, exitPrefab;
    [SerializeField] private GameObject _MaskPrefab;

    [HideInInspector] public float timer;
    [HideInInspector] public bool GameFinished = false;
    
    [HideInInspector]
    public bool GameStarted = false;

    private void Update()
    {
        if (GameFinished || !GameStarted) return;
        timer -= Time.deltaTime;
        if (timer < 0)
            timerText.text = "0";
        else
        {
            timerText.text = ((int)timer).ToString();
        }
    }

    private void Start()
    {
        string levelText = Resources.Load<TextAsset>("Level" + (((PlayerPrefs.GetInt("CurrentLevel")-1)%4)+1)).text;
        levelData = JsonConvert.DeserializeObject<Level.LevelData>(levelText);

        for (int i = 0; i < levelData.blocks.Count; i++)
        {
            Level.BlockData a = levelData.blocks[i];
            GameObject obj = Instantiate(blockPrefab, transform);
            obj.transform.position = new Vector2(a.x, a.y);
            obj.GetComponent<Renderer>().material.color = blockColors[(int)a.type];
            a.gameObject = obj;
            a.index = i;
        }

        List<Vector2Int> emptyPositions = GetEmptyPositions();
        int ind = levelData.blocks.Count;
        foreach (var emptyBlock in emptyPositions)
        {
            Level.BlockData blockData = new Level.BlockData();
            blockData.x = emptyBlock.x;
            blockData.y = emptyBlock.y;
            blockData.type = BlockType.None;
            blockData.index = ind++;
            blockData.gameObject = null;
            levelData.blocks.Add(blockData);
        }

        for (int i = 0; i < levelData.exits.Length; i++)
        {
            Level.ExitData exitData = levelData.exits[i];
            GameObject obj = Instantiate(exitPrefab, transform);
            obj.transform.position = new Vector2(exitData.x, exitData.y);
            if ((int)exitData.direction % 2 == 0)

                obj.transform.position += Vector3.down * 0.5f * ((int)exitData.direction - 1);
            else
                obj.transform.position += Vector3.left * 0.5f * ((int)exitData.direction - 2);

            obj.GetComponent<Renderer>().material.color = blockColors[(int)exitData.type];
            obj.transform.eulerAngles = new Vector3(0, 0, 90 + 90 * (int)exitData.direction);
        }

        Camera mainCam = Camera.main;
        if (levelData.width % 2 != 0)
            mainCam.gameObject.transform.position = new Vector3(levelData.width / 2, levelData.height / 2, -10f);
        else
            mainCam.gameObject.transform.position =
                new Vector3((levelData.width - 1) / 2f, levelData.height / 2, -10f);

        GameObject maskPrefab = Instantiate(_MaskPrefab);
        maskPrefab.transform.position = new Vector3(levelData.width/2- ((levelData.width % 2 == 0 )? 0.5f:0f),mainCam.transform.position.y - ((levelData.height % 2 == 0 )? 0.5f:0f),0);
        maskPrefab.transform.localScale = new Vector3(levelData.width,levelData.height,1);
        ManageCameraZoom();
        timer = levelData.time;
    }

    private List<Vector2Int> GetEmptyPositions()
    {
        List<Vector2Int> result = new List<Vector2Int>();
        for (int x = 0; x < levelData.width; x++)
        {
            for (int y = 0; y < levelData.height; y++)
            {
                bool isEmpty = IsEmptyPosition(new Vector2Int(x, y));
                if (!isEmpty) continue;

                result.Add(new Vector2Int(x, y));
            }
        }

        return result;
    }


    private bool IsEmptyPosition(Vector2Int vector2Int)
    {
        foreach (var block in levelData.blocks)
        {
            if (block.x == vector2Int.x && block.y == vector2Int.y) return false;
        }

        return true;
    }

    private void ManageCameraZoom()
    {
        int zoomSize = levelData.width > levelData.height ? levelData.width : levelData.height;
        Camera.main.orthographicSize = zoomSize;
    }

    public Level.BlockData GetBlockDataFromPosition(Vector2Int pos)
    {
        for (int i = 0; i < levelData.blocks.Count; i++)
        {
            if (levelData.blocks[i].x == pos.x && levelData.blocks[i].y == pos.y)
            {
                return levelData.blocks[i];
            }
        }

        return null;
    }

    public bool isGameFinished()
    {
        foreach (var block in levelData.blocks)
        {
            if (block.type != BlockType.Obstacle && block.type != BlockType.None)
                return false;
        }

        return true;
    }

    public Level.ExitData GetExitPosition(BlockType type, Direction movDir)
    {
        foreach (var exit in levelData.exits)
        {
            if (exit.direction == movDir && type == exit.type) return exit;
        }

        return null;
    }
}