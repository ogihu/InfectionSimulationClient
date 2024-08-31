using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class InputManager
{
    public readonly Dictionary<KeyCode, int> KeyMap = new Dictionary<KeyCode, int>()
    {
        { KeyCode.W, 1 << 0 },
        { KeyCode.A, 1 << 1 },
        { KeyCode.S, 1 << 2 },
        { KeyCode.D, 1 << 3 }
    };

    public int SetKeyInput(KeyCode key, int inputBit)
    {
        if (Input.GetKey(key))
            inputBit = inputBit | KeyMap[key];
        else
        {
            inputBit = inputBit & ~KeyMap[key];
        }

        return inputBit;
    }

    public bool GetKeyInput(KeyCode key, int inputBit)
    {
        return (inputBit & KeyMap[key]) != 0;
    }
}