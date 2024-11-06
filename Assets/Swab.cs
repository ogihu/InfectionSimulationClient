using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Swab : UsingItem
{
    bool check = false;
    public Texture2D cursorTexture;  // 사용할 커서 이미지
    public Vector2 hotSpot = Vector2.zero; // 커서의 포인터 위치 설정 (0,0은 좌상단)
    public override bool Use(BaseController character)
    {
        cursorTexture = 
        StartCoroutine(Use_Swab());
        StartCoroutine(Use_Checking());
        return base.Use(character);
    }

    IEnumerator Use_Swab()
    {
        Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
        yield return null;
    }
    IEnumerator Use_Checking()
    {
        while(!check)
            yield return null;
    }
}
