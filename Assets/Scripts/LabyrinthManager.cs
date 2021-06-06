using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Button = UnityEngine.UI.Button;
using Toggle = UnityEngine.UI.Toggle;

public class LabyrinthManager : MonoBehaviour
{
    public float scrollSpeed = 5;
    public Button startButton;
    public Text startButtonText;

    public GameObject sizeFields;
    public InputField widthInput;
    public InputField heightInput;
    public Toggle randomToggle;
    public Button setSizeButton;

    public Button resetLabyrinthButton;

    public Button helpButton;
    public Button exitButton;
    public GameObject helpView;

    public Text infoText;
    
    private GridMap _gridMap;
    private PathFinder _pathFinder;
    private ActorController _actor;
    private Rigidbody2D _actorRb;

    private Camera _camera;
    private const int _camMin = 1;
    private int _camMax;

    private bool _foundPath = false;

    public enum State
    {
        Initializing,
        Editing,
        FollowingPath,
        Finished
    }

    public State state;
    
    // Start is called before the first frame update
    void Start()
    {
        _gridMap = FindObjectOfType<GridMap>();
        _pathFinder = FindObjectOfType<PathFinder>();
        _actor = FindObjectOfType<ActorController>();
        _actorRb = _actor.GetComponent<Rigidbody2D>();
        
        _camera = Camera.main;

        startButton.onClick.AddListener(StartSearch);
        startButton.gameObject.SetActive(false);
        startButtonText = startButton.GetComponentInChildren<Text>();

        widthInput.characterValidation = InputField.CharacterValidation.Integer;
        heightInput.characterValidation = InputField.CharacterValidation.Integer;
        
        setSizeButton.onClick.AddListener(CreateGrid);

        resetLabyrinthButton.onClick.AddListener(ResetLabyrinth);
        
        helpButton.onClick.AddListener(ToggleHelpView);
        helpView.SetActive(false);
        
        exitButton.onClick.AddListener(Exit);

        infoText.text = "";

        // set game state to enter size
        state = State.Initializing;
    }

    // Update is called once per frame
    void Update()
    {
        if(state != State.Initializing) UpdateCamera();
        
        if(state == State.Editing) EditLabyrinth();
    }

    private void StartSearch()
    {
        if (state == State.Editing)
        {
            _foundPath = _pathFinder.FindPath();
        }
        else if (state == State.Finished)
        {
            var start = _pathFinder.start;
            _actor.transform.position = new Vector3(start.x, start.y);
        }

        // if path not found dont get path
        if (!_foundPath)
        {
            state = State.Finished;
            return;
        }
        
        // set path for actor to follow
        _actor.GetPath();

        // set game state to finding path
        state = State.FollowingPath;

        // set button text to indicate restarting simulation
        startButtonText.text = "Restart simulation";
    }

    private void CreateGrid()
    {
        var width = int.Parse(widthInput.text);
        var height = int.Parse(heightInput.text);

        if (width < 1 || height < 1)
        {
            // Debug.Log("Cannot set field with dimensions less than 1");
            infoText.text = "Cannot set field with dimensions less than 1";
            return;
        }
        
        // create grid size width * height
        if(randomToggle.isOn) _gridMap.CreateRandomGrid(width, height);
        else _gridMap.CreateDefaultGrid(width, height);
        
        // disable size menu;
        sizeFields.SetActive(false);

        _gridMap.gameObject.SetActive(true);
        
        SetCameraToCenter();

        infoText.text = "Edit your labyrinth! Check help for controls.";
    }
    

    private void EditLabyrinth()
    {
        // setting the tiles
        if (Input.GetMouseButton(0))
        {
            // set wall tile
            var (mX, mY) = GetMousePosition();
            
            _gridMap.SetCellBlocked(mX, mY, true);
        } 
        else if (Input.GetMouseButton(1))
        {
            // set walkable tile
            var (mX, mY) = GetMousePosition();
            
            _gridMap.SetCellBlocked(mX, mY, false);
        } 
        else if (Input.GetKeyDown(KeyCode.G))
        {
            // set exit
            var (mX, mY) = GetMousePosition();
            
            _gridMap.SetCellExit(mX,mY);
        } 
        else if (Input.GetKeyDown(KeyCode.B))
        {
            // set start position
            var (mX, mY) = GetMousePosition();
            
            _gridMap.SetCellBlocked(mX, mY, false);
            _pathFinder.start = new Vector2Int(mX, mY);
            _actorRb.transform.position = new Vector3(_pathFinder.start.x, _pathFinder.start.y);
        }
    }

    private (int x, int y) GetMousePosition()
    {
        var mousePos = _camera.ScreenToWorldPoint(Input.mousePosition);
        var mX = (int) Math.Floor(mousePos.x);
        var mY = (int) Math.Floor(mousePos.y);
        return (mX, mY);
    }

    private void SetCameraToCenter()
    {
        // set camera position to center of map, viewing entire map
        var gridMapWidth = _gridMap.Width;
        var gridMapHeight = _gridMap.Height;
        
        var cameraX = gridMapWidth / 2;
        var cameraY = gridMapHeight / 2;
        
        if (_camera is null) return;
        // set max camera zoom
        _camMax = Math.Max(gridMapWidth, gridMapHeight);
        
        // set camera position
        var cameraTransform = _camera.transform;
        cameraTransform.position = new Vector3(cameraX, cameraY, cameraTransform.position.z);
        
        // set camera zoom
        var cameraOrthographicSize = _camMax / 2;
        _camera.orthographicSize = cameraOrthographicSize;

        // enable start button
        startButton.gameObject.SetActive(true);
        
        // set game state to editing
        state = State.Editing;
    }
    
    private void UpdateCamera()
    {
        // set camera movement
        var newPos = _camera.transform.position;
        
        newPos += new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        
        // limit camera movement space around the labyrinth
        var clampX = Mathf.Clamp(newPos.x, 0, _gridMap.Width);
        var clampY = Mathf.Clamp(newPos.y, 0, _gridMap.Height);
        _camera.transform.position = new Vector3(clampX, clampY, newPos.z);
        

        // set camera zoom
        var camSize = _camera.orthographicSize;
        camSize += Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;
        _camera.orthographicSize = Mathf.Clamp(camSize, _camMin, _camMax);
    }


    private void ResetLabyrinth()
    {
        // set game state to initializing
        state = State.Initializing;
        startButton.gameObject.SetActive(false);
        sizeFields.SetActive(true);
        _gridMap.gameObject.SetActive(false);

        _foundPath = false;

        startButtonText.text = "Start";
        
        _pathFinder.start = Vector2Int.zero;
        
        // set actor to initial position
        _actor.transform.position = Vector3.zero;
        _actorRb.velocity = Vector2.zero;
    }

    private void ToggleHelpView()
    {
        var active = helpView.activeSelf;
        helpView.SetActive(!active);
    }

    private void Exit()
    {
        Application.Quit();
    }
}
