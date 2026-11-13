using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using NUnit.Framework.Constraints;

public class GGameManager : MonoBehaviour
{
    public List<GameObject> playerList;
    public static GGameManager instance;
    [SerializeField] public List<Gopher> activeRacers = new List<Gopher>(); 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        /*
        instance = this;
        DontDestroyOnLoad(this.gameObject);
        */
        
        }

    // Update is called once per frame
    void Update()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);
    }
}
