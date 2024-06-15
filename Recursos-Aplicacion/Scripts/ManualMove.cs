using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManualMove : MonoBehaviour
{
    public BluetoothTest myBT;
    public InputField velText;
    public InputField moveText;

    public float move = 10.0f;// 1 cm 
    public int vel = 500;// cm/ min 
    // Start is called before the first frame update
    void Start()
    {
        velText.text = vel.ToString();
        moveText.text = move.ToString("F4");

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //Funcion on end edit
    public void OnValueEndEdit()
    {
        move = Mathf.Abs(float.Parse(moveText.text));
        vel = int.Parse(velText.text);
    }
    // Funcion de desplazamiento manual
    public void MoveManual(string textButton)
    {
        StartCoroutine(MovingManual(textButton));
    }
    IEnumerator MovingManual(string textB)
    {
        Debug.Log("funcion Move manual");
        move = Mathf.Abs(float.Parse(moveText.text));
        vel = int.Parse(velText.text);
        string s = "G01 " + textB + move + " F" + vel;
        Debug.Log(s);
        //myBT.SendText("G91");
        //myBT.ShowInConsole(s);
        //myBT.SendText("G90");
        myBT.ExternSendCommand("G91", false);
        yield return new WaitForSeconds(0.1f);
        myBT.ExternSendCommand(s);
        yield return new WaitForSeconds(0.1f);
        myBT.ExternSendCommand("G90", false);
        yield return new WaitForSeconds(0.1f); 
    }
}
