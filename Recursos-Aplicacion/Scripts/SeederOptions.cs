using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeederOptions : MonoBehaviour
{
    public Animator transitionSO;
    //public bool stateSO = true;//true = show, flase = hide
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ShowHideButtonSO()
    {
        if(transitionSO.GetCurrentAnimatorStateInfo(0).IsName("ShowSO"))
        {
            StartCoroutine(HideSeedOpt());
        }
        else
        {
            StartCoroutine(ShowSeedOpt());
        }
    }

    IEnumerator ShowSeedOpt()
    {
        transitionSO.SetTrigger("ShowSO");

        yield return new WaitForSeconds(1f);
    }
    IEnumerator HideSeedOpt()
    {
        transitionSO.SetTrigger("HideSO");

        yield return new WaitForSeconds(1f);
    }
}
