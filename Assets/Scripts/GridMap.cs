using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class GridMap : MonoBehaviour
{
    public Sprite cellSprite;
    public Sprite wallSprite;
    public Sprite exitSprite;

    public Cell[,] Cells;
    
    private int _width = 0;
    private int _height = 0;
    
    private Tilemap _tilemap;

    private PathFinder _pathFinder;

    private GridMap _instance;

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
    
    void Start()
    {
        _tilemap = FindObjectOfType<Tilemap>();
        _pathFinder = FindObjectOfType<PathFinder>();
    }

    // creates grid of only walkable cells
    public void CreateDefaultGrid(int w, int h)
    {
        _tilemap.ClearAllTiles();
        
        _width = w;
        _height = h;
        
        Cells = new Cell[_width, _height];
        
        for (var i = 0; i < _width; ++i)
        {
            for (var j = 0; j < _height; ++j)
            {
                Cells[i, j] = new Cell();
                SetCellBlocked(i, j, false);
            }
        }
        
        SetEdgeTiles();
    }

    public void CreateRandomGrid(int w, int h)
    {
        _tilemap.ClearAllTiles();

        _width = w;
        _height = h;
        
        Cells = new Cell[_width, _height];
        
        for (var i = 0; i < _width; ++i)
        {
            for (var j = 0; j < _height; ++j)
            {
                Cells[i, j] = new Cell();
                
                var chance = Random.Range(0f, 1f);
                var blocked = false;

                var wallChance = ((i > 0 && Cells[i - 1, j].isBlocked) || (j > 0 && Cells[i, j - 1].isBlocked))
                    ? 0.5
                    : 0.3;
                if (chance <= wallChance)
                {
                    blocked = true;
                }
                
                SetCellBlocked(i, j, blocked);
            }
        }
        
        SetEdgeTiles();
    }

    private void SetEdgeTiles()
    {
        for (var i = -1; i <= _width; ++i)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = wallSprite;
            _tilemap.SetTile(new Vector3Int(i, -1, 0), tile);
            _tilemap.SetTile(new Vector3Int(i, _height, 0), tile);
        }
        for (var j = -1; j <= _height; ++j)
        {
            var tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = wallSprite;
            _tilemap.SetTile(new Vector3Int(-1, j, 0), tile);
            _tilemap.SetTile(new Vector3Int(_width, j, 0), tile);
        }
    }
    

    public void SetCellBlocked(int i, int j, bool isBlocked)
    {
        if (CellOutOfBounds(i, j)) return;

        if (CellIsStart(i, j)) isBlocked = false;

        // set cell is only blocked
        Cells[i, j].isBlocked = isBlocked;
        Cells[i, j].isExit = false;
        
        // set tile sprite
        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = isBlocked ? wallSprite : cellSprite;
        _tilemap.SetTile(new Vector3Int(i, j, 0), tile);
    }

    public void SetCellExit(int i, int j)
    {
        if (CellOutOfBounds(i, j) || CellIsStart(i, j)) return;

        // set cell is only exit
        Cells[i, j].isBlocked = false;
        Cells[i, j].isExit = true;
        
        // set tile sprite
        var tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = exitSprite;
        _tilemap.SetTile(new Vector3Int(i, j, 0), tile);
    }
    
    // checks if cell position is valid
    public bool CellOutOfBounds(int i, int j)
    {
        return (i < 0) || (j < 0) || (i >= _width) || (j >= _height);
    }

    public bool CellIsStart(int i, int j)
    {
        var cell = new Vector2Int(i, j);
        return cell == _pathFinder.start;
    }
    
    public int Width => _width;

    public int Height => _height;
}
