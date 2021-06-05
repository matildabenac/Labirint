using UnityEngine;
using UnityEngine.Tilemaps;

public class GridMap : MonoBehaviour
{
    public int width;
    public int height;
    public Sprite cellSprite;
    public Sprite wallSprite;
    public Sprite exitSprite;

    public Cell[,] Cells;
    
    private Tilemap _tilemap;

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
        Cells = new Cell[width, height];
        
        _tilemap = FindObjectOfType<Tilemap>();
        
        CreateDefaultGrid();
    }

    // creates grid of only walkable cells
    private void CreateDefaultGrid()
    {
        for (var i = 0; i < width; ++i)
        {
            for (var j = 0; j < height; ++j)
            {
                Cells[i, j] = new Cell();
                SetCellBlocked(i, j, false);
            }
        }
    }
    

    public void SetCellBlocked(int i, int j, bool isBlocked)
    {
        if (CellOutOfBounds(i, j)) return;

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
        if (CellOutOfBounds(i, j)) return;

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
        return (i < 0) || (j < 0) || (i >= width) || (j >= height);
    }
}
