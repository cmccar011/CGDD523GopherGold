using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
namespace Ex2
{
    public class Goal : MonoBehaviour
    {
        //      UI          //
        [SerializeField] TextMeshProUGUI timerTxt;
        public TextMeshProUGUI racerNameTxt;
        public TextMeshProUGUI racerTimeTxt;

        //          LISTS AND ARRAYS FOR RACERS         //
        public List<string> racerPlacementList = new List<string>();
        public string[] placementText;
        public List<float> racerTime = new List<float>();

        //          VARIABLES       //
        int finishCount = 0;
        [SerializeField] public int secondsPass = 0;
        public bool keepTime = true;



        void Start()
        {
            StartCoroutine(SimpleTimer());
        }
        void FixedUpdate()
        {
            //updates timer text
            timerTxt.text = "Time: " + secondsPass.ToString();
            SceneManagement();

            if(finishCount == 4)
            {
                StartCoroutine(Delay());
            }
        }
        //Coroutine with a loop
        IEnumerator SimpleTimer()
        {
            while (keepTime)
            {
                yield return new WaitForSeconds(1.0f);
                secondsPass++;
            }
        }
        //Delay for score to be visible for 2 seconds before game finishes
        IEnumerator Delay()
        {
                yield return new WaitForSeconds(2.0f);
                ResetGame();
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.gameObject.tag == "Racer")
            {
                string racerName = collision.gameObject.GetComponent<Racer>().RacerColor;

                print(racerName);

                racerTime.Add(secondsPass);

                racerPlacementList.Add(racerName);
                //formats the string to be added to the list
                string racerNameFormat = "";
                string placementTextFormat = "";
                string racerTimeFormat = "";

                //counter to place the racers in a single text box in the proper place
                for (int i = 0; i < racerTime.Count; i++)
                {
                    racerTimeFormat += racerTime[i] + "\n";
                }
                for (int i = 0; i < racerPlacementList.Count; i++)
                {
                    racerNameFormat += racerPlacementList[i] + "\n";
                }
                for (int i = 0; i < placementText.Length; i++)
                {
                    placementTextFormat += placementText[i] + "\n";
                }
                //displays the placements
                racerNameTxt.text = racerNameFormat;
                racerTimeTxt.text = racerTimeFormat;

                //Destroys Racer
                Destroy(collision.gameObject);

                //Counter to end game
                finishCount++;
                print(finishCount);
            }
        }
        void OnPressSpace()
        {
            ResetGame();
        }
        void SceneManagement()
        {
            if (secondsPass == 60)
            {
                SceneManager.LoadScene("RaceExample");
            }
        }
        void ResetGame()
        {
            print("I AM RESETTING");     
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        }
    }
}
