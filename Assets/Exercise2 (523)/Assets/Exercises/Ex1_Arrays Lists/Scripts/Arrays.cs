using UnityEngine;
using UnityEngine.InputSystem;

namespace Ex1
{
    public class Arrays : MonoBehaviour
    {
        public string[] inventory;
        public int [] stats = {100,50,20,2};
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            //loop through the strings in inventory
            for(int i=0; i < inventory.Length - 1; i++)
            {
                    print(inventory[i]);
            }
            
            //access and change the first item in our array;
            inventory[0] = "super number 1 toothpick";

            //a forach loop work like a for loop but counts the number of elements for us
            foreach(string s in inventory)
            {
                    print(s);
            }
        }

        // Update is called once per frame
        void Update()
        {
     
        }
    }
}
