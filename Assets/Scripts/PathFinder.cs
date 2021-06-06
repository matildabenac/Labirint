using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    public Vector2Int start;

    [SerializeField]
    private GridMap gridMap;

    private LabyrinthManager _labyrinthManager;
    
    // for DFS
    // private HashSet<(int i, int j)> exitsFound;

    public IEnumerable<(int, int)> path;

    public readonly (int i, int j)[] Dir = new (int, int)[]
    {
        (0, 1), // up
        (1, 0), // right
        (0, -1), // down
        (-1, 0) // left
    };
    
    private PathFinder _instance;

    void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
        
        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        gridMap = FindObjectOfType<GridMap>();
        _labyrinthManager = FindObjectOfType<LabyrinthManager>();

        // for DFS
        // exitsFound = new HashSet<(int i, int j)>();
    }

    public bool FindPath()
    {
        // // Debug.Log(Dfs(start.x, start.y, 0, -1, -1));
        // var found = Dfs(start.x, start.y, 0, -1, -1)
        // var exitCell = exitsFound.First();
        // path = GetPath(exitCell.exitI, exitCell.exitJ)

        var (found, dist, (exitI, exitJ)) = Bfs(start.x, start.y);
        if (found)
        {
            // Debug.Log("Distance to goal: " + dist);
            _labyrinthManager.infoText.text = "Distance to goal: " + dist;
            
            path = CreatePath(exitI, exitJ);
            return true;
        }
        // Debug.Log("Path not found!");
        _labyrinthManager.infoText.text = "Path not found!";
        return false;
        }

    // private bool Dfs(int i, int j, int dist, int prevI, int prevJ)
    // {
    //     if (CellOutOfBounds(i, j)) return false;
    //
    //     var currCell = gridMap.Cells[i, j];
    //     if (currCell.isBlocked) return false;
    //     
    //     if (dist >= currCell.dist) return false;
    //     currCell.dist = dist;
    //     currCell.previousCell = (prevI, prevJ);
    //
    //     if (currCell.isExit)
    //     {
    //         exitsFound.Add((i, j));
    //         return true;
    //     }
    //
    //     var ret = false;
    //     for (var k = 0; k < Dir.Length; ++k)
    //     {
    //         var nI = i + Dir[k].i;
    //         var nJ = j + Dir[k].j;
    //
    //         if (CellOutOfBounds(nI, nJ)) continue;
    //         
    //         ret |= Dfs(nI, nJ, dist + 1, i, j);
    //     }
    //
    //     return ret;
    // }
    
    // returns if exit ws found, distance to exit and exit cell position
    private (bool found, int dist, (int exitI, int exitJ)) Bfs(int startI, int startJ)
    {
        // check if start cell is valid
        if (gridMap.CellOutOfBounds(startI, startJ)) return (false, -1, (-1, -1));
        // check if start is blocked
        if(gridMap.Cells[startI, startJ].isBlocked) return (false, -1, (-1, -1));
    
        var cells = new Queue<(int i, int j, int dist)>();
        // add start to queue
        cells.Enqueue((startI, startJ, 0));
    
        while (cells.Count > 0)
        {
            // get next cell position from queue
            var (currI, currJ, currDist) = cells.Dequeue();

            var currentCell = gridMap.Cells[currI, currJ];

            // check if arrived at exit
            if (currentCell.isExit) return (true, currDist, (currI, currJ));
            
            // check neighbour cells
            for (var k = 0; k < Dir.Length; ++k)
            {
                var nI = currI + Dir[k].i;
                var nJ = currJ + Dir[k].j;
    
                // check if neighbour cell is valid
                if (gridMap.CellOutOfBounds(nI, nJ)) continue;
    
                var nCell = gridMap.Cells[nI, nJ];
                
                // check if cell is blocked
                if (nCell.isBlocked) continue;
                // check if cell was already checked
                if (nCell.isClosed) continue;
    
                // set previous cell as current cell
                nCell.previousCell = (currI, currJ);
                
                // add cell to queue
                cells.Enqueue((nI, nJ, currDist + 1));
                // set cell was checked
                nCell.isClosed = true;
            }
        }
    
        return (false, -1, (-1, -1));
    }

    private IEnumerable<(int, int)> CreatePath(int endI, int endJ)
    {
        // path list
        var p = new List<(int i, int j)>();
        
        (int i, int j) curr = (endI, endJ);
        
        // iterate through cells from exit to start and add them to path list
        while (curr != (start.x, start.y) && !gridMap.CellOutOfBounds(curr.i, curr.j))
        {
            p.Add(curr);
            var prev = gridMap.Cells[curr.i, curr.j].previousCell;
            curr = prev;
        }
        
        // get list from start to exit
        p.Reverse();

        return p;
    }
}
