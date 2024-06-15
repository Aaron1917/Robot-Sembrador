using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using System.Text;

public class Position_CNC : MonoBehaviour
{
    // Campos de entrada de coordenadas
    [Header("Input Coordinates")]
    public InputField TextPosX;
    public InputField TextPosY;
    public InputField TextPosZ;
    // Campos de entrada de parámetros de siembra
    [Header("Seeder Parameters")]
    public InputField inTextInitX;
    public InputField inTextInitY;
    public InputField inTextInitP;
    public InputField inDistRow;
    public InputField inDistCol;
    //Campos de salida de seguiiento de lineas 
    [Header("Line Obs")]
    public InputField RL;
    public InputField SL;
    [Header("Velocities Machine")]
    public InputField velXYText;
    public InputField velZText;
    [Header("Plants Admin")]
    [SerializeField] private GameObject plantPrefab;
    [SerializeField] private Transform plantContainer;
    public List<GameObject> seedPointsList = new List<GameObject>();
    //private List<Plant> plantList = new List<Plant>();
    // Array de puntos de siembra 
    //public Vector2[] SeedPoints;


    // Variables de sistema GRBL
    public Vector3 MPos; // X, Y, Z machine pos
    public Vector3 WC0; // X, Y, Z coordenas de trabajo actual
    public int[] FS = new int[2]; // Velocidad de avance   |   Velocida husillo/ posicion del servo.s
    // Sequimiento de lineas 
    public long sendLine = 0;
    public long reciveLine = 0;
    // Las unidades deben de estar en mm
    public float initX = 80.0f;
    public float initY = 25.0f;
    public float initP = 0.0f;//20
    //                  Row (Y)
    //              * --------------*              
    //              |               |
    //              |               |
    //  Coulumn  (X)|               |
    //              |               |
    //              |               |
    //              *---------------*

    public float distRow = 100.0f;
    public float distCol = 100.0f;
    // Parametro Velocidad
    public int velXY = 1500;
    public int velZ = 1000;
    // Parametro de longitud de la punta
    public float puntaM = 88.0f;
    //Variable de salida Grbl
    public float disSharp = 0.0f; // medida del efector con respecto a la tierra
    public int seedConfirmed = 0; //confirmacion de semilla

    private float limitX = 610.0f; //
    private float limitY = 480.0f; //450
    private float limitZ = 280.0f;

    public bool homing = false;
    public bool home = true;

    // Start is called before the first frame update
    void Start()
    {
        MPos = new Vector3(0.0f, 0.0f, 0.0f);

        inDistRow.text = distRow.ToString("F1");
        inDistCol.text = distCol.ToString("F1");
        inTextInitX.text = initX.ToString("F1");
        inTextInitY.text = initY.ToString("F1");
        inTextInitP.text = initP.ToString("F1");
        velXYText.text = velXY.ToString();
        velZText.text = velZ.ToString();
        StartCoroutine(UpdateLine());
    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OnValueEndEdit()
    {
        distRow = float.Parse(inDistRow.text);
        distCol = float.Parse(inDistCol.text);
        initX = float.Parse(inTextInitX.text);
        initY = float.Parse(inTextInitY.text);
        initP = float.Parse(inTextInitP.text);
        velXY = int.Parse(velXYText.text);
        velZ = int.Parse(velZText.text);
    }
    public string SpeedAxis(string axis)
    {
        if (axis == "x" || axis == "y" || axis == "xy" || axis == "yx")
        {
            return "F" + velXY.ToString();
        }
        else if (axis == "z" || axis == "Z")
        {
            return "F" + velZ.ToString();
        }
        else
        {
            return "";
        }
    }
    public void UpdateCoorBox()
    {
        TextPosX.text = MPos.x.ToString("F4");
        TextPosY.text = MPos.y.ToString("F4");
        TextPosZ.text = MPos.z.ToString("F4");
    }
    public void UpdateLineN()
    {
        SL.text = sendLine.ToString();
        RL.text = reciveLine.ToString();
    }
    //FUnciones con plantas
    public void SeedPlant(int ID) // always send after M15 code
    {
        //Plant plant = new Plant(MPos, ID, seedConfirmed == 1);
        Plant plant = seedPointsList[ID].GetComponent<Plant>();
        plant.SetConfirm(seedConfirmed == 1);
        //plantList.Add(plant);
        seedConfirmed = 0;
    }
    public void DestroyPlantList(Transform container)
    {
        seedPointsList.Clear();
        for (var i = container.childCount - 1; i >= 0; i--)
        {
            Destroy(container.GetChild(i).gameObject);
        }
    }
    public void GenerateSeedPoints()
    {
        DestroyPlantList(plantContainer);
        float currentX = initX;
        float currentY = initY;
        int indexPoints = 0;

        //List<GameObject> seedPointsList = new List<GameObject>();

        while (currentX < limitX)
        {
            while (currentY < limitY)
            {
                Vector3 newPos = new Vector3(currentX, currentY, 0);
                GameObject newPlant = Instantiate(plantPrefab, newPos / -10f, Quaternion.identity);
                newPlant.transform.SetParent(plantContainer);
                seedPointsList.Add(newPlant);
                Plant p = seedPointsList[indexPoints].GetComponent<Plant>();
                if (p != null)
                {
                    p.SetupPlant(newPos * -1, indexPoints);
                }
                indexPoints++;
                //seedPointsList.Add(new Vector2(currentX, currentY));
                Debug.Log("X: " + currentX + " Y: " + currentY);
                currentY += distRow;
            }
            currentY -= distRow;
            currentX += distCol;
            while (currentY > 0)
            {
                Vector3 newPos = new Vector3(currentX, currentY, 0);
                GameObject newPlant = Instantiate(plantPrefab, newPos / -10f, Quaternion.identity);
                newPlant.transform.SetParent(plantContainer);
                seedPointsList.Add(newPlant);
                Plant p = seedPointsList[indexPoints].GetComponent<Plant>();
                if (p != null)
                {
                    p.SetupPlant(newPos * -1, indexPoints);
                }
                indexPoints++;
                //seedPointsList.Add(new Vector2(currentX, currentY));
                Debug.Log("X: " + currentX + " Y: " + currentY);
                currentY -= distRow;
            }
            currentY = initY;
            currentX += distCol;
        }
        // SeedPoints = seedPointsList.ToArray();
        Debug.Log("Total of seed points: " + indexPoints);
    }
    public string CalculateDeep()
    {//calcula la profundiad en base a valor de avance = distancia que mide el sensor- distancia de la punta + profundidad de siembra
        float valAvance = MPos.z - (disSharp - puntaM + initP);
                            //-200 = -1 - (267  - 88      + 20  )
        disSharp = 0.0f;
        return valAvance.ToString("F2"); 
    }
    public string RetracionZ() // el valor es negativo
    {
        if (MPos.z > -15.0f)
        {
            Debug.Log("Moviendo Z a -100");
            return "-100";
        }
        //float retraccion = MPos.z + 100.0f; // Ejemplo -283 MPos + 100 = -183.0
        //retraccion = PosiciónActual + 2 toleracia + medida punta + profundidad
        //             280   +20+880+20  
        float retraccion = MPos.z + 20.0f + puntaM/2 + initP;
        Debug.Log("Moviendo Z a "+retraccion.ToString("F2"));
        return retraccion.ToString("F2");
    }
    // manda g90 y g21 // coordenadas absolutas y mm como unidad
    public string InitialConfig()
    {
        return "G90 G21";// Sistema Absoluto | Sistema metrico 
    }
    public string Unlock()
    {
        return "$X";
    }
    public string Home()
    {
        homing = true;
        return "$H";
    }
    //
    IEnumerator UpdateLine()
    {
        while (true)
        {
            //SendGetStatus();
            UpdateLineN();
            //Debug.Log("R: " + CNC.reciveLine + " S: " + CNC.sendLine + CNC.MPos.ToString());
            //Debug.Log("Get Status.....");
            yield return new WaitForSeconds(0.1f);
        }
    }
}
