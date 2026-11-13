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
    public List<String> gopherNames = new List<String>();
    public String currentGophName = "";

    //public List<String> gopherRacers = new List<String>();
    public static CharacterSelection instance;

    //[SerializeField] public List<Gopher> gopherData;
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

    public PlayerInput pi;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(this);
        if (instance == null)
        {
            instance = this;
        }
        //changeChoosePlayer();
        isPicked[0] = false;
        isPicked[1] = false;
        isPicked[2] = false;
        isPicked[3] = false;
    }

    private void Update()
    {
        if (currentPlayerNum > maxPlayers)
        {
            choosePlayer.text = "ALL PLAYERS HAVE SELECTED!\nPress 'Ready' to play!";
        }

        if (
            Input.GetKeyDown(KeyCode.Alpha1)&&isPicked[0] == false
            )
        {
                isPicked[lastPlayerJoined] = true;
                lastPlayerJoined=0;
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
            Input.GetKeyDown(KeyCode.Alpha4)&& (isPicked[2] == false)
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

        /* Delete Probably
        PlayerPrefs.SetInt("TotalPlayers", selectedGopher.Count);
        for (int i = 0; i < selectedGopher.Count; i++)
        {
            PlayerPrefs.SetInt("Player " + (i + 1) + "Gopher ID", selectedGopher[i]);
            //PlayerPrefs.SetString("Gopher Name", selectedGopher);
        }

        PlayerPrefs.Save();
        */
        print(CharacterSelection.instance.activeRacers);

        gameObject.GetComponent<PlayerInputManager>().enabled = false;
        this.enabled = false;
        SceneManager.LoadScene(1);
    }

    public void OnPlayerJoined(PlayerInput p)
    {
        print("here");
        GopherSetup setup = p.gameObject.GetComponent<GopherSetup>();
        p.gameObject.GetComponent<GopherSetup>().SelectGopherData(lastPlayerJoined);
   
        if (GGameManager.instance != null)
        {
            GGameManager.instance.playerList.Add(p.gameObject);
            //GGameManager.instance.playerList.Add(p.gameObject);
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
