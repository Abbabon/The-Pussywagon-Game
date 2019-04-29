using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class OutroSequenceManager : MonoBehaviour
{
    PlayableDirector sequence;
    GameObject fader;

    private void Awake()
    {
        sequence = GetComponent<PlayableDirector>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
