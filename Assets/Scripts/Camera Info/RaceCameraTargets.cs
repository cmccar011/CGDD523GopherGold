using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class RaceCameraTargets : MonoBehaviour
{
    public struct Racer
    {
        public MinecartController cart;
        public int gopherIndex;
    }

    [SerializeField] private GameObject targetGroup;
    [SerializeField] private float weight = 1f;
    [SerializeField] private float radius = 2f;

    private Component group;
    private MethodInfo add, find, remove;
    private readonly List<Racer> racers = new List<Racer>();

    public IReadOnlyList<Racer> Racers => racers;

    IEnumerator Start()
    {
        Rebuild();
        yield return null;
        Rebuild();
    }

    void CollectRacers()
    {
        racers.Clear();
        CharacterSelection sel = CharacterSelection.instance;

        if (sel != null)
        {
            for (int i = 0; i < sel.selectedRacers.Count; i++)
            {
                Transform t = sel.selectedRacers[i];
                if (t == null || !t.gameObject.activeInHierarchy) continue;

                MinecartController cart = t.GetComponentInChildren<MinecartController>();
                if (cart == null) cart = t.GetComponentInParent<MinecartController>();
                if (cart == null || ContainsCart(cart)) continue;

                int gopher = i < sel.selectedRacerGophers.Count ? sel.selectedRacerGophers[i] : i;
                racers.Add(new Racer { cart = cart, gopherIndex = gopher });
            }
        }

        if (racers.Count == 0)
        {
            int i = 0;
            foreach (MinecartController cart in FindObjectsByType<MinecartController>(FindObjectsInactive.Exclude))
            {
                racers.Add(new Racer { cart = cart, gopherIndex = i++ });
            }
        }
    }

    bool ContainsCart(MinecartController cart)
    {
        foreach (Racer r in racers) if (r.cart == cart) return true;
        return false;
    }

    bool Bind()
    {
        if (group != null && add != null) return true;
        if (targetGroup == null) return false;

        foreach (Component c in targetGroup.GetComponents<Component>())
        {
            if (c != null && c.GetType().Name == "CinemachineTargetGroup") { group = c; break; }
        }
        Type type = group.GetType();
        add = type.GetMethod("AddMember", new[] { typeof(Transform), typeof(float), typeof(float) });
        find = type.GetMethod("FindMember", new[] { typeof(Transform) });
        remove = type.GetMethod("RemoveMember", new[] { typeof(Transform) });

        if (add == null)
        {
            Debug.LogWarning("ADDMEMBER FOR TGROUP NOT FOUND");
            return false;
        }
        return true;
    }

    public void SetTracked(MinecartController cart, bool tracked)
    {
        if (cart == null || !Bind()) return;

        object[] args = { cart.transform };
        bool present = find != null && (int)find.Invoke(group, args) >= 0;

        if (tracked && !present) add.Invoke(group, new object[] { cart.transform, weight, radius });
        else if (!tracked && present && remove != null) remove.Invoke(group, args);
    }

    void Rebuild()
    {
        CollectRacers();
        if (!Bind()) return;

        ClearMembers(group, group.GetType());

        if (find != null && remove != null)
        {
            for (int i = 0; i < 64 && (int)find.Invoke(group, new object[] { null }) >= 0; i++)
            {
                remove.Invoke(group, new object[] { null });
            }
        }

        List<string> names = new List<string>();

        foreach (Racer racer in racers)
        {
            if (find != null && (int)find.Invoke(group, new object[] { racer.cart.transform }) >= 0) continue;

            SetTracked(racer.cart, true);
            names.Add(racer.cart.name);
        }

        Debug.Log(racers.Count + " racer(s), added " + names.Count + ": " + string.Join(", ", names));
    }

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