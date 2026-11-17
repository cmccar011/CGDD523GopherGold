using UnityEngine;

public class GopherStateManager : MonoBehaviour
{
    public enum GopherState { Shooting, Hit, Blinking, Driving };
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ourState = GopherState.Driving;
    }

    public GopherState ourState;
    // Update is called once per frame
    void Update()
    {
        switch (ourState)
        {
            case GopherState.Shooting:
                break;
            case GopherState.Hit:
                break;
            case GopherState.Blinking:
                break;
            case GopherState.Driving:
                break; 
        }
    }
}

