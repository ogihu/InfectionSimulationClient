using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class Navigation : MonoBehaviour
{
    NavMeshAgent navMeshAgent;
    Vector3 FixedCoordinates;      // 화살표 캐릭터에 위치 좌표(고정시키기 위해서)
    public Vector3 TargetPosition;
    GameObject PlaceGroup;

    private void Awake()
    {
        FixedCoordinates = gameObject.transform.localPosition;
        navMeshAgent = gameObject.GetComponent<NavMeshAgent>();
        navMeshAgent.speed = 0;
        PlaceGroup = GameObject.Find("Colliders");
        Managers.Scenario.Navigation = this;
        gameObject.SetActive(false);
    }
    
    void Update()
    {
        UpdateGoTargetPoint();
    }

    void UpdateGoTargetPoint()
    {
        if (Managers.Scenario.CurrentScenarioInfo == null)
            return;

        if (TargetPosition == null)
            return;

        if (gameObject.transform.position != FixedCoordinates)
        {
            gameObject.transform.localPosition = FixedCoordinates;
        }

        // 목표 지점으로 내비게이션 설정
        if(gameObject.activeSelf)
            navMeshAgent.SetDestination(TargetPosition);

        // NavMesh 경로를 얻고, 첫 번째 경로 포인트를 사용하여 회전
        if (navMeshAgent.pathPending || navMeshAgent.path.corners.Length < 2)
            return;

        // 첫 번째 코너(목표지점과 가까운 경로)를 바라보도록 회전
        Vector3 direction = navMeshAgent.path.corners[1] - transform.position;
        direction.y = 0;  // 수평면에서만 회전
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    public void SetDestination(string place)
    {
        TargetPosition = Util.FindChildByName(PlaceGroup, place).transform.position;
    }
}
 