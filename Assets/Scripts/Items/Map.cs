using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    private void Awake()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (LayerMask.LayerToName(other.gameObject.layer) == "MyPlayer")
        {
            Managers.Scenario.UpdateMyPlace(gameObject.name);
        }

        if(other.gameObject.GetComponent<BaseController>() != null)
        {
            other.gameObject.GetComponent<BaseController>().Place = gameObject.name;
        }
    }
}
