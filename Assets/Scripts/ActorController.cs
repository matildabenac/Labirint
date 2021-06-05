using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ActorController : MonoBehaviour
{
    private PathFinder _pathFinder;
    // private IEnumerable<(int x, int y)> _path;
    private IEnumerator<(int x, int y)> _pEnum;
    private Rigidbody2D _rigidbody;

    private float _speed = 5f;

    private bool _go = false;

    private Vector3 _target;
    
    // Start is called before the first frame update
    void Start()
    {
        _pathFinder = FindObjectOfType<PathFinder>();
        _rigidbody = GetComponent<Rigidbody2D>();
        
        StartCoroutine(GetPath());

        _target = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(_go) MoveActor();
    }

    private IEnumerator GetPath()
    {
        // wait until there is a path
        yield return new WaitUntil(() => _pathFinder.path != null);
        
        // set target to first cell after start
        _pEnum = _pathFinder.path.GetEnumerator();
        _pEnum.MoveNext();
        _target = new Vector3(_pEnum.Current.x, _pEnum.Current.y);

        _go = true;
    }

    private void MoveActor()
    {
        // get direction to target
        var direction = _target - transform.position;
        direction.Normalize();
        // set movement
        _rigidbody.velocity = new Vector2(direction.x * _speed, direction.y * _speed);
        
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
            _go = false;
            _rigidbody.velocity = Vector2.zero;
        }
    }
}
