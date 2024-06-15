using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickeableSprite : MonoBehaviour
{
    private Collider myCollider;
    public Plant p;
    public PlantInfo pInfo;
    // Start is called before the first frame update
    void Start()
    {
        // Obtener el Collider del GameObject
        myCollider = GetComponent<Collider>();
        p = GetComponent<Plant>();
        pInfo = FindObjectOfType<PlantInfo>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnMouseDown()
    {
        // Coloca aquí el comportamiento que deseas ejecutar al hacer clic
        //.Log("¡El jugador hizo clic en el sprite!");
        // Por ejemplo, podrías cambiar el color del sprite
        //GetComponent<SpriteRenderer>().color = Color.green;
        pInfo.SetUpParam(p.GetPosition(), p.plantID, p.addList, p.GetConfirm());
    }
}