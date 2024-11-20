using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WordWrappingRatio : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<TMP_Text>().wordWrappingRatios = 1;
    }
}
