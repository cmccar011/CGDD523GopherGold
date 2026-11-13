using TMPro;
using UnityEngine;

namespace Ex3
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager instance;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }

        // Update is called once per frame
        void Update() { }
    }
}
