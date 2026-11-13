using Unity.VisualScripting;
using UnityEngine;
using System.Collections.Generic;


public class GopherSetup : MonoBehaviour

  
 {
    [SerializeField]  public Gopher[] gData = new Gopher[4];

    public Gopher ourGopher;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelectGopherData(int i)
    {
        ourGopher = gData[i];
        /*
        if (GGameManager.instance != null && ourGopher != null)
        {
            GGameManager.instance.activeRacers.Add(ourGopher);
        }
        */

    }


}
