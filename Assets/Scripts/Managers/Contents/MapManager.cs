using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager
{
    public GameObject PlaceGroup;

    public void Init()
    {
        PlaceGroup = GameObject.Find("Colliders");
    }

    public Vector3 FindPlacePosition(string place)
    {
        if(PlaceGroup == null)
        {
            Init();
        }

        GameObject go = Util.FindChildByName(PlaceGroup, place);

        if(go == null)
        {
            Debug.LogError($"There is no place : {place}");
            return Vector3.zero;
        }

        return go.transform.position;
    }

    public void Clear()
    {
        PlaceGroup = null;
    }
}
