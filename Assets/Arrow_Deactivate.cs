using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow_Deactivate : MonoBehaviour
{
    GameObject Arrow;
    GameObject NoDoorMap;
    GameObject Collider_Place;

    private void Start()
    {
        Arrow = Managers.Resource.Instantiate($"Items/Arrow", this.gameObject.transform);
        NoDoorMap = GameObject.Find("NoDoorMap");
        Collider_Place = Util.FindChild(NoDoorMap, "Colliders");
        Arrow.SetActive(false);
    }

    private void Update()
    {
        if (!Managers.Scenario._doingScenario)
            return;

        if (Managers.Scenario.CurrentScenarioInfo == null)
            return;

        if (Managers.Scenario.CurrentScenarioInfo.Place == null)
            return;

        if (!(Managers.Scenario.CurrentScenarioInfo.Position == Managers.Object.MyPlayer.Position))
            return;

        if (!(Managers.Scenario.CurrentScenarioInfo.Place != Managers.Object.MyPlayer.Place))
            return;

        if (Arrow.activeSelf)
            return;

        Arrow.SetActive(true);
    }
    
    public void init()
    {
        Arrow.GetComponent<Arrow>().Scenario_Place = Util.FindChild(Collider_Place, Managers.Scenario.CurrentScenarioInfo.Place).transform.position;
    }
}
