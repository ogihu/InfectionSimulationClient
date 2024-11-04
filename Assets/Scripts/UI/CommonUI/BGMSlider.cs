using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BGMSlider : SliderUI
{
    private void Start()
    {
        GetComponent<Slider>().value = Managers.Setting.BGMVol;
    }

    public override void OnValueChanged()
    {
        base.OnValueChanged();
        Managers.Setting.BGMVol = GetComponent<Slider>().value;
    }
}