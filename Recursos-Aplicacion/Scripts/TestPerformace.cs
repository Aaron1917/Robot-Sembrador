using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestPerformace : MonoBehaviour
{
    public GameObject testG;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void EnebleGrup()
    {
        if (testG.activeInHierarchy)
        {
            testG.SetActive(false);
        }
        else
        {
            testG.SetActive(true);
        }
    } 
}
