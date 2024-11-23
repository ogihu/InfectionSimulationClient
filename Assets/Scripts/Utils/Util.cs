using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Util
{
    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }

    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }

    public static GameObject FindChildByName(GameObject go, string name)
    {
        if (go == null)
            return null;

        for(int i = 0; i < go.transform.childCount; i++)
        {
            if(go.transform.GetChild(i).name == name)
                return go.transform.GetChild(i).gameObject;

            GameObject goChild = FindChildByName(go.transform.GetChild(i).gameObject, name);
            if (goChild != null)
                return goChild;
        }

        return null;
    }

    public static GameObject FindParentByName(GameObject go, string name)
    {
        if (go == null)
            return null;

        if (go.transform.parent == null)
            return null;

        if (go.name == name)
            return go;

        return FindParentByName(go.transform.parent.gameObject, name);
    }

    public static List<T> FindComponentInRange<T>(GameObject center, float radius, LayerMask exceptionLayer) where T : Component
    {
        if (center == null)
        {
            Debug.LogError("Various center didn't assinged.");
            return null;
        }

        LayerMask layerMask = ~exceptionLayer;
        
        // 현재 오브젝트의 위치를 기준으로 반경 내에 있는 Collider들 가져옴
        Collider[] hitColliders = Physics.OverlapSphere(center.transform.position, radius, layerMask);
        List<T> result = new List<T>();

        // Collider들 중에서 원하는 컴포넌트를 가진 오브젝트를 찾음
        foreach (Collider collider in hitColliders)
        {
            T component = collider.GetComponent<T>();
            if (component != null)
            {
                result.Add(component);
            }
        }

        if (result.Count == 0)
            return null;

        return result;
    }

    public static Material[] AddMaterial(Material[] materials, Material materialToAdd)
    {
        Material[] newMaterials = new Material[materials.Length + 1];
        materials.CopyTo(newMaterials, 0);
        newMaterials[newMaterials.Length - 1] = materialToAdd;
        return newMaterials;
    }

    public static Material[] RemoveMaterial(Material[] materials, Material materialToRemove)
    {
        List<Material> materialList = new List<Material>(materials);
        materialList.Remove(materialToRemove);
        return materialList.ToArray();
    }

}
