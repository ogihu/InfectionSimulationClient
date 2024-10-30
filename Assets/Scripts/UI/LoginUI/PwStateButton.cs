using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PwStateButton : ButtonUI
{
    TMP_InputField _pwInput;
    Sprite _publicImage;
    Sprite _privateImage;
    Image _image;
    bool _isPrivate;

    protected override void Awake()
    {
        base.Awake();
        _pwInput = transform.parent.GetComponent<TMP_InputField>();
        _publicImage = Managers.Resource.Load<Sprite>("Sprites/PwStateIcons/public");
        _privateImage = Managers.Resource.Load<Sprite>("Sprites/PwStateIcons/private");
        _image = GetComponent<Image>();
        _isPrivate = true;
        ChangeState(_isPrivate);
    }

    protected override void OnClicked()
    {
        base.OnClicked();
        _isPrivate = !_isPrivate;
        ChangeState(_isPrivate);
    }

    /// <summary>
    /// isPrivate가 true면 ContentType = Password, 아니면 ContentType = Standard
    /// </summary>
    /// <param name="isPrivate"></param>
    void ChangeState(bool isPrivate)
    {
        if (isPrivate)
        {
            _image.sprite = _privateImage;
            _pwInput.contentType = TMP_InputField.ContentType.Password;
            _pwInput.ForceLabelUpdate();
        }
        else
        {
            _image.sprite = _publicImage;
            _pwInput.contentType = TMP_InputField.ContentType.Standard;
            _pwInput.ForceLabelUpdate();
        }
    }
}
