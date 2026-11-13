using System.Collections;
using UnityEngine;
using TMPro;
namespace Ex2
{
public class CoroutinesExamples : MonoBehaviour
{



        [Header("Racer Times")]

        [SerializeField] public float racerGreenTime;

        [SerializeField] public float racerPinkTime;

        [SerializeField] public float racerOrangeTime;

        [SerializeField] public float racerBlueTime;

    [Header("Placements")]
        [SerializeField] public int firstPlaceTime;

        [SerializeField] public float secondPlaceTime;

        [SerializeField] public float thirdPlaceTime;

        [SerializeField] public float fourthPlaceTime;


    void Start()
    {
        //StartCoroutine(SimpleConversation());
        //StartCoroutine(KnockKnock());
    }

    void Update()
    {
        


        }

    //simple Coroutine
    IEnumerator SimpleConversation()
    {
        print("Hey is anyone there?");
        yield return new WaitForSeconds(2.0f);
        print("Just me the ghost");
    }

    
    //simple Coroutine with multiple yields
    IEnumerator KnockKnock()
    {
        print("Knock Knock");
        yield return new WaitForSeconds(1.0f);
        print("Who's there?");
        yield return new WaitForSeconds(1.0f);
        print("Boo");
        yield return new WaitForSeconds(1.0f);
        print("Boo Who?");
        yield return new WaitForSeconds(1.0f);
        print("Boo Who stop crying");
    }


    // changes scene at 60 seconds
}
}
