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
