using UnityEngine;
//you need to include this to use lists!
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Ex1
{

    public class Lists : MonoBehaviour
    {
        public List<string> inventory = new List<string>();
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            //we can add to a list by calling the add function
            inventory.Add("SwordOfPower");
            inventory.Add("ShieldOfDrain");
            
            //we can acess items in our lists using array notation!
            print(inventory[0]);

            //we can remove thing from our array using Remove

        }

        // Update is called once per frame
        void Update()
        {
            
        }

        void OnJump()
        {
            //we can remove thing from our array using Remove
            inventory.Remove("ShieldOfDrain");

            // we can check a list to see if it contains an item! 
            // and we can add items to a List!
            
            if (inventory.Contains("SwordOfPower"))
            {
                print("You have the sword of power, please take some gems!");
                inventory.Add("Gem");
            }

        }
    }
}
