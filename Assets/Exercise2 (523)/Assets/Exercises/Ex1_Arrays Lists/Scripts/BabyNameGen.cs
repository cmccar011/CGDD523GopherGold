using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BabyNameGen : MonoBehaviour
{
    //UI
    public TextMeshProUGUI babyNameText, pastBabyText;
    public string[] phonemes;
    public List<string> pastBabyNameList = new List<string>(); 
    void Start()
    {

    }
    void Update()
    {

    }
    void OnJump()
    {
        int randNum1 = Random.Range(0, phonemes.Length);
        int randNum2 = Random.Range(0, phonemes.Length);

        string lastAnswer = phonemes[randNum1] + phonemes[randNum2];
        babyNameText.text = lastAnswer;
        pastBabyNameList.Insert(0, lastAnswer);

        pastBabyText.text = "";

        for(int i = 1; i < pastBabyNameList.Count; i++)
        {
            pastBabyText.text += pastBabyNameList[i] + "\n";
        }
    }
}