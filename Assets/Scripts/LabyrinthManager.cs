using System;
using UnityEngine;
using UnityEngine.UI;

public class LabyrinthManager : MonoBehaviour
{
    public float scrollSpeed = 5;
    public Button startButton;
    
    private GridMap _gridMap;
    private PathFinder _pathFinder;
    private Rigidbody2D _actorRb;

    private Camera _camera;
    private int _camMin;
    private int _camMax;

    private enum State
    {
        EnterSize,
        Editing,
        Started,
        Finished
    }

    private State _state;
    
    // Start is called before the first frame update
    void Start()
    {
        _gridMap = FindObjectOfType<GridMap>();
        _pathFinder = FindObjectOfType<PathFinder>();
        _actorRb = FindObjectOfType<ActorController>().GetComponent<Rigidbody2D>();
        
        _camera = Camera.main;
        _camMin = 5;
        _camMax = Math.Max(_gridMap.width, _gridMap.height);

        var cameraX = _gridMap.width / 2;
        var cameraY = _gridMap.height / 2;
        if (!(_camera is null))
        {
            var cameraTransform = _camera.transform;
            cameraTransform.position = new Vector3(cameraX, cameraY, cameraTransform.position.z);
        }

        startButton.onClick.AddListener(StartSearch);
    }

    // Update is called once per frame
    void Update()
    {
        // for editing labyrinth
        EditLabyrinth();
    }

    private void StartSearch()
    {
        _pathFinder.GetPath();
    }

    private void EditLabyrinth()
    {
        // setting the tiles
        if (Input.GetMouseButton(0))
        {
            // set wall tile
            var mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
            _gridMap.SetCellBlocked((int)mousePos.x, (int)mousePos.y, true);
        } 
        else if (Input.GetMouseButton(1))
        {
            // set walkable tile
            var mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
            _gridMap.SetCellBlocked((int)mousePos.x, (int)mousePos.y, false);
        } 
        else if (Input.GetKeyDown(KeyCode.G))
        {
            // set exit
            var mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
            _gridMap.SetCellExit((int)mousePos.x, (int)mousePos.y);
        } 
        else if (Input.GetKeyDown(KeyCode.B))
        {
            // set start position
            var mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
            _gridMap.SetCellBlocked((int)mousePos.x, (int)mousePos.y, false);
            _pathFinder.start = new Vector2Int((int)mousePos.x, (int)mousePos.y);
            _actorRb.transform.position = new Vector3(_pathFinder.start.x, _pathFinder.start.y);
        }

        // camera movement
        var newPos = _camera.transform.position;
        
        newPos += new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        
        // limit camera movement space around the labyrinth
        var clampX = Mathf.Clamp(newPos.x, 0, _gridMap.width);
        var clampY = Mathf.Clamp(newPos.y, 0, _gridMap.height);
        _camera.transform.position = new Vector3(clampX, clampY, newPos.z);
        

        // camera zoom
        var camSize = _camera.orthographicSize;
        camSize += Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
        _camera.orthographicSize = Mathf.Clamp(camSize, _camMin, _camMax);
    }
}
