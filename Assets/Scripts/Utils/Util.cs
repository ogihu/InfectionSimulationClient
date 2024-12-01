using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine. UI;

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

        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(go.transform);

        while (queue.Count > 0)
        {
            Transform current = queue.Dequeue();

            if (current.name == name)
                return current.gameObject;

            for (int i = 0; i < current.childCount; i++)
            {
                Transform child = current.GetChild(i);
                if (child != null)
                    queue.Enqueue(child);
            }
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

    public static List<T> FindComponentInRange<T>(GameObject center, float radius, LayerMask layer) where T : Component
    {
        if (center == null)
        {
            Debug.LogError("Various center didn't assinged.");
            return null;
        }

        LayerMask layerMask = layer;
        
        
        Collider[] hitColliders = Physics.OverlapSphere(center.transform.position, radius, layerMask);
        List<T> result = new List<T>();

        
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
    public static IEnumerator FadeOutImage(Image image, float duration)
    {
        if (image == null)
        {
            Debug.LogError("Image is null!");
            yield break;
        }

        Color color = image.color;
        float startAlpha = color.a;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, 0f, elapsedTime / duration);
            image.color = color;
            yield return null;
        }

        color.a = 0f;
        image.color = color;
    }

    public static IEnumerator FadeOutCoverImages(Transform coverImagesParent, float fadeDuration)
    {
        if (coverImagesParent == null)
        {
            Debug.LogError("CoverImagesParent is null!");
            yield break;
        }

        int childCount = coverImagesParent.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Image imageComponent = coverImagesParent.GetChild(i).GetComponent<Image>();
            if (imageComponent != null)
            {
                
                yield return FadeOutImage(imageComponent, fadeDuration);
            }

            
            coverImagesParent.GetChild(i).gameObject.SetActive(false);
        }
    }
}

