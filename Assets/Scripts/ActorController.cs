using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class ActorController : MonoBehaviour
{
    private PathFinder _pathFinder;
    private LabyrinthManager _labyrinthManager;
    
    private IEnumerator<(int x, int y)> _pEnum;
    private Rigidbody2D _rigidbody;

    public float speed = 5f;

    private Vector3 _target;
    
    // Start is called before the first frame update
    void Start()
    {
        _pathFinder = FindObjectOfType<PathFinder>();
        _labyrinthManager = FindObjectOfType<LabyrinthManager>();
        _rigidbody = GetComponent<Rigidbody2D>();

        _target = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // moving actor on path
        if(_labyrinthManager.state == LabyrinthManager.State.FollowingPath) MoveActor();
    }

    // public IEnumerator GetPath()
    public void GetPath()
    {
        // set target to first cell after start
        _pEnum = _pathFinder.path.GetEnumerator();
        _pEnum.MoveNext();

        _target = new Vector3(_pEnum.Current.x, _pEnum.Current.y);
    }

    private void MoveActor()
    {
        // get direction to target
        var direction = _target - transform.position;
        direction.Normalize();
        // set movement
        _rigidbody.velocity = new Vector2(direction.x * speed, direction.y * speed);
        
        // check if target is reached
        if (Vector3.Distance(_target, transform.position) > 0.1) return;
        
        // if reached target, get next cell in path as target position
        var hasNext = _pEnum.MoveNext();
        if (hasNext)
        {
            _target = new Vector3(_pEnum.Current.x, _pEnum.Current.y);
        }
        else
        {
            // stop moving
            _labyrinthManager.state = LabyrinthManager.State.Finished;
            _rigidbody.velocity = Vector2.zero;
        }
    }
}
