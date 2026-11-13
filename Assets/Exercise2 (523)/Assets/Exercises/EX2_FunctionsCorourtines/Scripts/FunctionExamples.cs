using UnityEngine;
using UnityEngine.InputSystem;
namespace Ex2
{
    public class FunctionExamples : MonoBehaviour
    {
        public int sayCount = 0;
        public string [] weapons;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            SaySomething();
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        //call a basic function print a message increase counter;
        public void SaySomething()
        {
            print("Something");
            sayCount++;
        }
        // call a function and pass parameter
        public void SayParam(string phrase)
        {
            print(phrase);
            sayCount++;
        }
         // call a function and pass parameter
        public void SayMultiParam(string playerName,string phrase)
        {
            print("Hello "+playerName+" "+ phrase);
            sayCount++;
        }
        // call a function and have a default parameter
        public void SayDefault(string phrase="Hi")
        {
            print(phrase);
            sayCount++;
        }

        // return a value from a function
        public string PickRandomAttack()
        {
            int randW = Random.Range(0,weapons.Length);
            return weapons[randW];
        } 

       void OnPressSpace()
        {
            print("You attack with a "+ PickRandomAttack());
        }
    }
}