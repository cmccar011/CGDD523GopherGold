using UnityEngine;

namespace Ex4
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager instance;
        public AudioSource efxSource,
            musicSource;

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
