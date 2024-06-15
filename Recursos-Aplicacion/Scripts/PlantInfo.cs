using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlantInfo : MonoBehaviour
{
    [SerializeField] private InputField coordX;
    [SerializeField] private InputField coordY;
    [SerializeField] private InputField coordZ;
    [SerializeField] private InputField ID;

    [SerializeField] private Toggle addSeed;// añadir a rutina
    [SerializeField] private Toggle seeded; //sembrado

    public Position_CNC CNC;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnValuesEndChange()
    {
        Plant p = CNC.seedPointsList[int.Parse(ID.text)].GetComponent<Plant>();
        Vector3 newPos = new Vector3(float.Parse(coordX.text), float.Parse(coordY.text), p.GetPosition().z);
        p.ChangeParamPlant(newPos, addSeed.isOn);
    }
    public void SetUpParam(Vector3 v, int id, bool add, bool seed)
    {
        coordX.text = v.x.ToString();
        coordY.text = v.y.ToString();
        coordZ.text = v.z.ToString();
        ID.text = id.ToString();
        addSeed.isOn = add;
        seeded.isOn = seed;
    }
    /*public void IDParam()
    {
        // id
        CNC.seedPointsList[]
    }*/
}
