using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public bool isUsable = true;
    public GameObject potion;

    public Node(bool _isUsable, GameObject _potionq)
    {
        isUsable = _isUsable;
        potion = _potionq;
    }
}
