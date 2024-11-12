using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class Arrow : MonoBehaviour
{
    NavMeshAgent navMeshAgent;

    Vector3 Fixed_coordinates;      // 화살표 캐릭터에 위치 좌표(고정시키기 위해서)
    public Vector3 Scenario_Place;//  = new Vector3(11.224f, 0.761f, 0.376f);  // 목표 위치

    void Start()
    {
        Fixed_coordinates = gameObject.transform.localPosition;
        navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        navMeshAgent.speed = 0;  // 이동 속도 0으로 설정 (화살표만 회전)
    }
    
    void Update()
    {
        Deactivate_Check();
        Update_Go_Target_Point();
    }
   void Deactivate_Check()
    {   
        if (!Managers.Scenario._doingScenario)
            gameObject.SetActive(false);

        if (Managers.Scenario.CurrentScenarioInfo.Place == null)
            gameObject.SetActive(false);

        if (Managers.Scenario.CurrentScenarioInfo.Position != Managers.Object.MyPlayer.Position)
            gameObject.SetActive(false);

        if (Managers.Scenario.CurrentScenarioInfo.Place == Managers.Object.MyPlayer.Place)
            gameObject.SetActive(false);

        if (Fixed_coordinates == null || Scenario_Place == null)
            gameObject.SetActive(false);
    }

    void Update_Go_Target_Point()
    {
        if (gameObject.transform.position != Fixed_coordinates)
        {
            gameObject.transform.localPosition = Fixed_coordinates;
        }

        if (Scenario_Place == null)
            return;

        // 목표 지점으로 내비게이션 설정
        if(gameObject.activeSelf)
            navMeshAgent.SetDestination(Scenario_Place);

        // NavMesh 경로를 얻고, 첫 번째 경로 포인트를 사용하여 회전
        if (navMeshAgent.pathPending || navMeshAgent.path.corners.Length < 2)
            return;

        // 첫 번째 코너(목표지점과 가까운 경로)를 바라보도록 회전
        Vector3 direction = navMeshAgent.path.corners[1] - transform.position;
        direction.y = 0;  // 수평면에서만 회전
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }
}
 