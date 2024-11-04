using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SFXSlider : SliderUI
{
    private void Start()
    {
        GetComponent<Slider>().value = Managers.Setting.SFXVol;
    }

    public override void OnValueChanged()
    {
        base.OnValueChanged();
        Managers.Setting.SFXVol = GetComponent<Slider>().value;
    }
}