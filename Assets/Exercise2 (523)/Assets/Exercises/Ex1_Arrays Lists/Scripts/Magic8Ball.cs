using UnityEngine;
using TMPro;
using System.Collections.Generic;

namespace Ex1
{
    public class Magic8Ball : MonoBehaviour
    {
        public TextMeshProUGUI  ballAnswerText,pastAnswerText;
        public string [] possibleAnswers;
        public List<string> pastAnswerList = new List<string>();

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        void OnJump()
        {
           //grab a random answer from a random spot in our answer array
            int randNum = Random.Range(0,possibleAnswers.Length);

          //use the random number to grab a random answer and display it and add it to our past answer list;
          string lastAnswer = possibleAnswers[randNum];
          ballAnswerText.text = lastAnswer;
          pastAnswerList.Insert(0,lastAnswer);
         
         //clear out the pastAnswerText
         pastAnswerText.text = "";

         //print out our past answer list starting 
          for(int i = 1; i < pastAnswerList.Count; i++)
          {
            pastAnswerText.text += pastAnswerList[i] +"\n";
          }

        }
    }
}
