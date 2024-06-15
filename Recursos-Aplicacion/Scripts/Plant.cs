using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plant : MonoBehaviour
{
    public int plantID;
    private Vector3 position;
    public new Transform transform;
    private System.DateTime seedDateTime;
    private bool confirmSeed = false;
    public bool addList = true;
    public bool executed = false;
    public SpriteRenderer sprite;

    private Color newGray = Color.gray;
    private void Start()
    {
        confirmSeed = false;//false
        addList = true;
        executed = false;//false
        newGray.a = 0.5f;
    }
    private void Update()
    {
        //position = transform.position * 10;
        transform.position = position / 10;
        if(executed && confirmSeed)
        {
            sprite.color = Color.green;
        }
        else if (executed && !confirmSeed)
        {
            sprite.color = Color.red;
        }
        else if (!addList){
            sprite.color = Color.black;
        }
        else
        {
            sprite.color = newGray;
        }
    }
    //public void SetupPlant(Vector3 v, int ID, bool c)
    public void SetupPlant(Vector3 v, int ID)
    {
        //seedDateTime = System.DateTime.Now;
        position = v;
        plantID = ID;
        //confirmSeed = c;
    }
    public void ChangeParamPlant(Vector3 v, bool rutine)
    {
        position = v;
        addList = rutine;
    }
    public void SetConfirm(bool c)
    {
        confirmSeed = c;
        executed = true;
    }
    public bool GetConfirm()
    {
        return confirmSeed;
    }
    public void SetPosition(Vector3 v)
    {
        position = v;
    }
    public Vector3 GetPosition()
    {
        return position;
    }
}
