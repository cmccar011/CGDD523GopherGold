using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection; //for rebuilding the scene and adding the gophers
using UnityEngine;

public class RaceCameraTargets : MonoBehaviour
{
    [SerializeField] private GameObject targetGroup;
    [SerializeField] private float weight = 1f;
    [SerializeField] private float radius = 2f;

    IEnumerator Start()
    {
        Rebuild();
        yield return null;
        Rebuild();
    }

    List<Transform> GetRacers()
    {
        List<Transform> racers = new List<Transform>();

        if (CharacterSelection.instance != null)
        {
            foreach (Transform t in CharacterSelection.instance.selectedRacers)
            {
                if (t == null || !t.gameObject.activeInHierarchy) 
                    continue;

                MinecartController cart = t.GetComponentInChildren<MinecartController>(); //gets the child bc otherwise would grab the empty gameobject's pos
                if (cart == null) 
                    cart = t.GetComponentInParent<MinecartController>(); //not in the child? grab the parents!

                Transform target = cart != null ? cart.transform : t; // ? and : is just a shortened if/else statement (putting in notes for y'all so it's easier to read!)
                if (!racers.Contains(target)) 
                    racers.Add(target);
            }
        }

        if (racers.Count == 0)
        {
            foreach (MinecartController cart in FindObjectsByType<MinecartController>(FindObjectsInactive.Exclude))
            {
                racers.Add(cart.transform);
            }
        }

        return racers;
    }

    void Rebuild()
    {
        if (targetGroup == null) return;

        Component group = null;
        foreach (Component c in targetGroup.GetComponents<Component>())
        {
            if (c != null && c.GetType().Name == "CinemachineTargetGroup") { group = c; break; }
        }

        Type type = group.GetType();
        ClearMembers(group, type);


        //this was annoying. this is why I needed the reflection package
        //it looks at the names in TargetGroup, then GetMethod finds what to handle
        //Invoke calls those methods and actually adds them one by one with all the bells and whistles
        MethodInfo add = type.GetMethod("AddMember", new[] { typeof(Transform), typeof(float), typeof(float) });
        MethodInfo find = type.GetMethod("FindMember", new[] { typeof(Transform) });
        MethodInfo remove = type.GetMethod("RemoveMember", new[] { typeof(Transform) });

        if (add == null)
        {
            Debug.LogWarning("NO ADDMEMBER");
            return;
        }

        if (find != null && remove != null)
        {
            for (int i = 0; i < 64 && (int)find.Invoke(group, new object[] { null }) >= 0; i++)
            {
                remove.Invoke(group, new object[] { null });
            }
        }

        List<Transform> racers = GetRacers();
        List<string> names = new List<string>();

        foreach (Transform racer in racers)
        {
            if (find != null && (int)find.Invoke(group, new object[] { racer }) >= 0) continue;

            add.Invoke(group, new object[] { racer, weight, radius });
            names.Add(racer.name);
        }

        Debug.Log(racers.Count + " racer(s), added " + names.Count + ": " + string.Join(", ", names));
    }


    // Another part of Reflection that as they are added clears to ensure there are no duplicates (I was getting like, 4-10 gophers carrying over between scenes)
    // field is just the raw variable, then propertyinfo assigns like a get/set
    // you have to call them both bc reflection is silly and needs them there as two diff things
    // think of PropertyInfro as a failsafe for FieldInfo (also if there are ever issues it works with different versions of Cinemachine)
    void ClearMembers(Component group, Type type)
    {
        foreach (string memberName in new[] { "m_Targets", "Targets" })
        {
            FieldInfo field = type.GetField(memberName);
            PropertyInfo prop = field == null ? type.GetProperty(memberName) : null;
            if (field == null && prop == null) continue;

            object value = field != null ? field.GetValue(group) : prop.GetValue(group);

            if (value is Array && field != null)
            {
                field.SetValue(group, Array.CreateInstance(field.FieldType.GetElementType(), 0));
            }
            else if (value is IList list)
            {
                list.Clear();
            }
            return;
        }
    }
}