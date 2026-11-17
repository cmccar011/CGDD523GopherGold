using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelection : MonoBehaviour
{
    [Header("GopherData")]
    public TextMeshProUGUI choosePlayer;
    public int maxPlayers = 4;
    public int currentPlayerNum = 0;

    //Stores selected gophs
    public List<int> selectedGopher = new List<int>();
    public Boolean isStarted = false;
    public Button[] gopherButtons = new Button[4];
    public Button startButton; 
    public List<String> gopherNames = new List<String>();
    public String currentGophName = "";
    public static CharacterSelection instance;

    [SerializeField]
    public List<Gopher> activeRacers = new List<Gopher>();

    [SerializeField]
    private Sprite janeImg;

    [SerializeField]
    private Sprite popeImg;

    [SerializeField]
    private Sprite tobyImg;

    [SerializeField]
    private Sprite timmyImg;

    [SerializeField]
    public Gopher jane;

    [SerializeField]
    public Gopher pope;

    [SerializeField]
    public Gopher toby;

    [SerializeField]
    public Gopher timmy;

    [SerializeField]
    private float fadeTime = 0.5f;
    private bool[] isPicked = new bool[4];

    public List<Gopher> gopherObjectList;
    public int lastPlayerJoined;

    //this is so that we can get them in the target group cam! -Cory
    public List<Transform> selectedRacers = new List<Transform>();
    public List<int> selectedRacerGophers = new List<int>();
    public PlayerInput pi;


    void Start()
    {
        DontDestroyOnLoad(this);
        if (instance == null)
        {
            instance = this;
        }

        isPicked[0] = false;
        isPicked[1] = false;
        isPicked[2] = false;
        isPicked[3] = false;

        if(startButton == null)
        {
            startButton = GetComponent<Button>(); 
        }
    }

    private void Update()
    {
        if (currentPlayerNum > maxPlayers)
        {
            choosePlayer.text = "ALL PLAYERS HAVE SELECTED!\nPress 'Ready' to play!";
        }

        if (
            Input.GetKeyDown(KeyCode.Alpha1) && isPicked[0] == false
            )
        {
            isPicked[lastPlayerJoined] = true;
            lastPlayerJoined = 0;
        }
        if (
            Input.GetKeyDown(KeyCode.B)
            && (isPicked[1] == false)
            )
        {
            lastPlayerJoined = 1;
            isPicked[lastPlayerJoined] = true;
        }
        if (
            Input.GetKeyDown(KeyCode.Alpha4) && (isPicked[2] == false)
        )
        {

            lastPlayerJoined = 2;
            isPicked[lastPlayerJoined] = true;
        }
        if (
            Input.GetKeyDown(KeyCode.Alpha7) && (isPicked[3] == false)
        )
        {
            lastPlayerJoined = 3;
            isPicked[lastPlayerJoined] = true;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            startButton.onClick.Invoke(); 
        }
    }


    public void ShowJoin(int whichG)
    {
        isPicked[whichG] = true;
        if (gopherButtons != null && lastPlayerJoined < gopherButtons.Length)
        {
            FadeOutButtonImage(gopherButtons[whichG]);
        }
    }

    public void StartGame()
    {
        Gopher currentPlayer = new Gopher();

        isStarted = true;

        print(CharacterSelection.instance.activeRacers);

        gameObject.GetComponent<PlayerInputManager>().enabled = false;
        this.enabled = false;
        SceneManager.LoadScene(1);

    }

    public void OnStartGame()
    {
        print("blek");
        SceneManager.LoadScene(1);
    }
   // public void OnSpaceDown

    public void OnPlayerJoined(PlayerInput p)
    {
        print("here");
        GopherSetup setup = p.gameObject.GetComponent<GopherSetup>();
        p.gameObject.GetComponent<GopherSetup>().SelectGopherData(lastPlayerJoined);

        if (GGameManager.instance != null)
        {
            GGameManager.instance.playerList.Add(p.gameObject);
        }
        if (!selectedRacers.Contains(p.transform)) //added logic for the cam to get the player positions!
        {
            selectedRacers.Add(p.transform);
            selectedRacerGophers.Add(lastPlayerJoined);
        }
        ShowJoin(lastPlayerJoined);
    }

    public void FadeOutButtonImage(UnityEngine.UI.Button targetButton)
    {
        if (targetButton != null)
        {
            UnityEngine.UI.Image img = targetButton.GetComponent<UnityEngine.UI.Image>();

            if (img != null)
            {
                img.DOFade(0.2f, 0.5f);
            }
        }
    }
}