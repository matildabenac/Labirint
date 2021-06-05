using UnityEngine;

public class Cell : Object
{
    public bool isBlocked = false;
    public bool isExit = false;
    public bool isClosed = false;

    // for dfs
    // public int dist = int.MaxValue;

    public (int i, int j) previousCell = (-1, -1);
}
