using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    int layerMask;

    private void Start()
    {
        layerMask = 1 << LayerMask.NameToLayer("MyPlayer");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != layerMask)
            return;

        
    }
}
