using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Syringe : UsingItem
{
    public override bool Use(BaseController character)
    {

        //주사기 오브젝트 손에 생성
        //채혈 시작
        //캐릭터 상태 UsingItem? 같은 상태로 변경

        return base.Use(character);
    }
}
