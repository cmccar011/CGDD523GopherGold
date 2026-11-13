using UnityEngine;
namespace Ex2
{
    public class Racer : MonoBehaviour
    {
        public string RacerColor;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            RandomYLoc();    
        }

        // Update is called once per frame
        void Update()
        {
            
        }
        public void RandomYLoc()
        {
            float yLoc = Random.Range(-3.0f,3.0f);
            transform.position = new Vector3(transform.position.x,yLoc,0);
        }
    }
}
