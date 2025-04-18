using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaceableBase : MonoBehaviour
{
    protected WorldGenerator _world;

    public virtual void Place(WorldGenerator world)
    {
        _world = world;
    }
}
