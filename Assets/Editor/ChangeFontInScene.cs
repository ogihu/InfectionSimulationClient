#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEditor.SceneManagement;

public class ChangeFont : Editor
{
    [MenuItem("Tools/Change All Text Fonts")]
    public static void ChangeAllTextFonts()
    {
        TMP_FontAsset newFont = Resources.Load<TMP_FontAsset>("Fonts/LINESeedKR-Rg SDF");
        TMP_Text[] texts = GameObject.FindObjectsOfType<TMP_Text>();
        foreach (TMP_Text text in texts)
        {
            text.font = newFont;
        }
    }

    [MenuItem("Tools/Change All Fonts Bold")]
    public static void BoldAllTextFonts()
    {
        TMP_Text[] texts = GameObject.FindObjectsOfType<TMP_Text>();
        foreach (TMP_Text text in texts)
        {
            text.fontStyle = FontStyles.Bold;
        }
    }

    [MenuItem("Tools/Change All Fonts Normal")]
    public static void NormalAllTextFonts()
    {
        TMP_Text[] texts = GameObject.FindObjectsOfType<TMP_Text>();
        foreach (TMP_Text text in texts)
        {
            text.fontStyle = FontStyles.Normal;
        }
    }
}
#endif
