using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAnimation : MonoBehaviour
{
    Animator _animator;
    string _prevAnimation;
    List<BaseController> _nearCharacters = new List<BaseController>();

    void Awake()
    {
        _animator = transform.parent.GetComponent<Animator>();
    }

    void PlayAnimation(string animationName)
    {
        if(_prevAnimation == animationName)
        {
            return;
        }

        _prevAnimation = animationName;
        _animator.Play(animationName);
    }

    private void OnTriggerEnter(Collider other)
    {
        BaseController controller = other.GetComponent<BaseController>();

        if (controller == null)
            return;

        _nearCharacters.Add(controller);

        if(_nearCharacters.Count > 0)
        {
            PlayAnimation("DoorOpening");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        BaseController controller = other.GetComponent<BaseController>();

        if (controller == null)
            return;

        if (_nearCharacters.Contains(controller))
        {
            _nearCharacters.Remove(controller);
        }

        if (_nearCharacters.Count <= 0)
        {
            PlayAnimation("DoorClosing");
        }
    }
}
