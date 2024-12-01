using UnityEngine;

public class QuitButton : ButtonUI
{
    protected override void OnClicked()
    {
        base.OnClicked();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    
}
