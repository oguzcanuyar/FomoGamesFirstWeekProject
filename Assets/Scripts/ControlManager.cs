using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

//private _blockData
//public BlockData

//ilk hamle yapılana kadar süre başlamasın
//playerprefs ile level kullanımı next level uı
//levellar loopta
//3. level boş exitler json edit
//main menu level UI level seçmeyok playgame:sonlevel kilitler
//projects folder yapısı düzenle
public class ControlManager : MonoBehaviour
{
    private Vector2 startPos;
    [SerializeField] private Camera _camera;
    [SerializeField] private BlockManager _blockManager;
    [SerializeField] private GameObject WinPanel, FailPanel;
    private int ObjectMoving = 0;

    private void Update()
    {
        if (_blockManager.GameFinished) return;
        if (_blockManager.timer < 0)
        {
            if (ObjectMoving == 0) FinishGame(0);
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            startPos = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 endPos = Input.mousePosition;

            Vector3 startWorldPoint = _camera.ScreenToWorldPoint(startPos);
            Vector3 endWorldPoint = _camera.ScreenToWorldPoint(endPos);

            Vector3 diff = endWorldPoint - startWorldPoint;
            if (diff.magnitude < 0.4) return;
            Direction movDir;
            if (Math.Abs(diff.x) > Math.Abs(diff.y))
                movDir = diff.x > 0 ? Direction.Right : Direction.Left;
            else
                movDir = diff.y > 0 ? Direction.Up : Direction.Down;


            Vector2Int roundedStartPos = new Vector2Int();
            roundedStartPos.x = Mathf.RoundToInt(startWorldPoint.x);
            roundedStartPos.y = Mathf.RoundToInt(startWorldPoint.y);
            
            OnSwipe(roundedStartPos, movDir);
        }
    }

    private void OnSwipe(Vector2Int pos, Direction movDir)
    {
        Level.BlockData touchedBlockData = _blockManager.GetBlockDataFromPosition(pos);
        if (touchedBlockData == null) return;
        if (touchedBlockData.type == BlockType.Obstacle) return;

        Level.ExitData targetExit = _blockManager.GetExitPosition(touchedBlockData.type, movDir);
        if (targetExit == null) return;
        Debug.Log("Moving : " + touchedBlockData.type + " to : " + new Vector2Int(targetExit.x, targetExit.y));


        if (TryFindPathTo(pos, new Vector2Int(targetExit.x, targetExit.y), out var path))
        {
            _blockManager.GameStarted = true;
            MoveObjectInPath(touchedBlockData, path, movDir);
        }
    }

    private bool TryFindPathTo(Vector2Int startVector, Vector2Int targetVector, out List<int> path)
    {
        if (startVector == targetVector)
        {
            path = new List<int>();
            return true;
        }

        Node[] Graph = new Node[_blockManager.levelData.blocks.Count];
        for (int i = 0; i < Graph.Length; i++)
        {
            Graph[i] = new Node();
        }

        Level.BlockData targetBlock = _blockManager.GetBlockDataFromPosition(targetVector);
        Level.BlockData startBlock = _blockManager.GetBlockDataFromPosition(startVector);
        Graph[startBlock.index].score = 0;

        while (true)
        {
            Level.BlockData currentNode = BlockWithLowestScore(Graph);
            if (currentNode == null)
            {
                Debug.Log("No PATH FOUND");
                path = null;
                return false;
            }
            Graph[currentNode.index].visited = true;
            List<Level.BlockData> neighbors = GetNeighborsFor(currentNode);
            foreach (var NextNode in neighbors)
            {
                if (NextNode == null || NextNode.type != BlockType.None) continue;
                if (Graph[NextNode.index].visited == false)
                {
                    float newScore = Graph[currentNode.index].score + 1;
                    if (newScore < Graph[NextNode.index].score)
                    {
                        Graph[NextNode.index].score = newScore;
                        Graph[NextNode.index].routeToNode = currentNode.index;
                    }
                }
            }

            if (currentNode == targetBlock)
            {
                Debug.Log("path found");
                path = buildPath(targetBlock, Graph);
                return true;
            }

            Level.BlockData res = BlockWithLowestScore(Graph);
            if (res == null || Graph[res.index].score == Mathf.Infinity)
            {
                Debug.Log("No PATH FOUND");
                path = null;
                return false;
            }
        }
    }

    private List<int> buildPath(Level.BlockData targetNode, Node[] Graph)
    {
        List<int> route = new List<int>();
        Node currentNode = Graph[targetNode.index];
        int currentNodeInd = targetNode.index;
        while (true)
        {
            route.Add(currentNodeInd);
            currentNodeInd = currentNode.routeToNode;
            if (currentNodeInd == -1) break;
            currentNode = Graph[currentNodeInd];
        }

        return route;
    }

    private void MoveObjectInPath(Level.BlockData startNode, List<int> ints, Direction exitDirection)
    {
        ObjectMoving++;
        List<Vector3> path = GetVectorPath(ints);
        path.Reverse();
        Vector3 LastPosition = Vector3.zero;
        if (path.Count != 0) LastPosition = path[path.Count - 1];
        else
        {
            Vector3 startPos =  new Vector3(startNode.x,startNode.y,0);
            path.Add(startPos);
            LastPosition = startPos;
        }
        if (exitDirection == Direction.Down) LastPosition += Vector3.down;
        else if (exitDirection == Direction.Left) LastPosition += Vector3.left;
        else if (exitDirection == Direction.Right) LastPosition += Vector3.right;
        else if (exitDirection == Direction.Up) LastPosition += Vector3.up;

        path.Add(LastPosition);
        _blockManager.levelData.blocks[startNode.index].type = BlockType.None;
        startNode.gameObject.transform.DOPath(path.ToArray(), .3f * path.Count)
            .OnComplete(() => OnPathComplete(startNode.gameObject));
    }

    private void OnPathComplete(GameObject node)
    {
        ObjectMoving--;
        Destroy(node.gameObject);
        if (_blockManager.isGameFinished() && ObjectMoving == 0)
        {
            FinishGame(1);
        }
        else
        {
            if (_blockManager.timer < 0)
                FinishGame(0);
        }
    }

    public void EndButton()
    {
        SceneManager.LoadScene(1);
    }

    private void FinishGame(int i)
    {
        _blockManager.GameFinished = true;
        if (i == 1) WinPanel.SetActive(true);
        else FailPanel.SetActive(true);
        if (i == 1)
        {
            PlayerPrefs.SetInt("CurrentLevel", PlayerPrefs.GetInt("CurrentLevel") + 1);
        }
    }

    List<Vector3> GetVectorPath(List<int> path)
    {
        List<Vector3> res = new List<Vector3>();
        foreach (var node in path)
        {
            res.Add(new Vector3(_blockManager.levelData.blocks[node].x, _blockManager.levelData.blocks[node].y,
                0));
        }

        return res;
    }


    private List<Level.BlockData> GetNeighborsFor(Level.BlockData node)
    {
        Vector2Int Place = new Vector2Int(node.x, node.y);
        Vector2Int up = Place + Vector2Int.up;
        Vector2Int down = Place + Vector2Int.down;
        Vector2Int left = Place + Vector2Int.left;
        Vector2Int right = Place + Vector2Int.right;

        List<Level.BlockData> results = new List<Level.BlockData>();
        Level.BlockData a = _blockManager.GetBlockDataFromPosition(up);
        results.Add(a);
        results.Add(_blockManager.GetBlockDataFromPosition(down));
        results.Add(_blockManager.GetBlockDataFromPosition(left));
        results.Add(_blockManager.GetBlockDataFromPosition(right));

        return results;
    }

    private Level.BlockData BlockWithLowestScore(Node[] graph)
    {
        int i = 0;
        float min = Mathf.Infinity;
        int minIndex = -1;
        foreach (var node in graph)
        {
            if (node.score < min && !node.visited)
            {
                minIndex = i;
                min = node.score;
            }

            i++;
        }

        if (minIndex == -1) return null;
        return _blockManager.levelData.blocks[minIndex];
    }

    private class Node
    {
        public float score;
        public bool visited;
        public int routeToNode;

        public Node()
        {
            score = Mathf.Infinity;
            visited = false;
            routeToNode = -1;
        }
    }
}