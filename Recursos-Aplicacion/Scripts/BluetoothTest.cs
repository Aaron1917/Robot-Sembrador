using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using System.Text;
using System.IO;



public class BluetoothTest : MonoBehaviour
{
    public Text deviceName;// can delete
    [Header("UI Components")]
    public InputField dataToSend;
    public Text consoleText;
    public Scrollbar consoleScrollbar;
    public Dropdown dropdownDevices;
    public Position_CNC CNC;
    public Text textStatus;

    [Header("CSV Test")]
    public string filePath;
    public List<string[]> rowData = new List<string[]>();
    public Vector3 oldv3 = new Vector3();
    public float oldTime = 0; 

    [Header("Plant Info Resources")]
    public Animator transitionPI;
    public bool statePI = true; //false = hide; true = show  
    /*
     * Robot Status has 4 values:
     * "Inactivo"
     * "Corriendo"
     * "Alarma"
     * "Home"
     * Estos estados definidos por grbl: Idle, Run, Hold, Jog, Alarm, Door, Check, Home, Sleep
     */

    private int errorN = 0;
    private Dictionary<int, string> errorTable = new Dictionary<int, string>();
    private string robotStatus = "Sleep";
    private bool IsConnected;
    public static string dataRecived = "";
    char[] newlineChars = { '\n', '\r' };
    //
    private bool autoRute = false;
    private bool sendComand = false;
    // Corrutinas
    private Coroutine sendTextData = null;
    private Coroutine statusCoroutine = null;
    private Coroutine callHoming = null;
    private Coroutine sendingMsg = null;
    private Coroutine receivingMsg = null;
    private Coroutine testRutine = null;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_2020_2_OR_NEWER
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN")
            || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_ADVERTISE")
            || !Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_CONNECT"))
        {
            Permission.RequestUserPermissions(new string[] {
                "android.permission.BLUETOOTH_SCAN",
                "android.permission.BLUETOOTH_ADVERTISE",
                "android.permission.BLUETOOTH_CONNECT"
            });
        }
#endif
#endif

        IsConnected = false;
        BluetoothService.CreateBluetoothObject();
        dropdownDevices.options.Clear();
        GetDevicesButton();
        dropdownDevices.value = 3;
        DropdownValueChanged(dropdownDevices.value);
        consoleText.text = "";

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetDevicesButton()
    {
        // Obtener los dispositivos Bluetooth como arreglo de cadenas
        string[] devicesArray = BluetoothService.GetBluetoothDevices();

        // Convertir el arreglo a una lista de cadenas
        List<string> devicesList = new List<string>(devicesArray);

        // Llenar el dropdown con los dispositivos vinculados
        foreach (var d in devicesList)
        {
            Debug.Log(d);
            dropdownDevices.options.Add(new Dropdown.OptionData() { text = d });
            //ShowInConsole(d);
        }
        dropdownDevices.RefreshShownValue();
        //dropdownDevices.onValueChanged.AddListener(delegate { DropdownValueChanged(dropdownDevices.value)});
    }

    public void DropdownValueChanged(int index)
    {
        // Aquí puedes manejar el evento de cambio de selección del Dropdown si lo necesitas
        Debug.Log("Seleccionaste: " + dropdownDevices.options[index].text);
    }

    /*
     * Estas funciones son para interactuar con los apartados de conexión con el socket Bluetooth.
     */
    public void StartButton()
    {
        int index = dropdownDevices.value;

        if (!IsConnected)
        {
            ShowInConsole("Conectando...");
            // IsConnected =  BluetoothService.StartBluetoothConnection(deviceName.text.ToString());
            IsConnected = BluetoothService.StartBluetoothConnection(dropdownDevices.options[index].text.ToString());
            ShowInConsole(dropdownDevices.options[index].text.ToString() + " Estado: " + (IsConnected ? "Conectado" : "Desconectado"));
            //consoleText.text += dropdownDevices.options[index].text.ToString() + " Estado: " + (IsConnected ? "Conectado":"Desconectado")+"\n";
            BluetoothService.Toast(dropdownDevices.options[index].text.ToString() + " Estado: " + (IsConnected ? "Conectado" : "Desconectado"));// status
            if (IsConnected)
            {
                dropdownDevices.enabled = false;
                if (receivingMsg == null)
                    receivingMsg = StartCoroutine(RecieveMessage());
                statePI = true;
                ShowHideButtonPI();
            }
        }
    }
    public void StopButton()
    {
        int index = dropdownDevices.value;
        if (IsConnected)
        {
            BluetoothService.StopBluetoothConnection();
            if (!BluetoothService.ConnectionStatus)
            {
                // consoleText.text += dropdownDevices.options[index].text.ToString() + " estado: Desconectado";
                ShowInConsole(dropdownDevices.options[index].text.ToString() + " Estado: Desconectado");
                IsConnected = BluetoothService.ConnectionStatus;
                dropdownDevices.enabled = true;
                if (statusCoroutine != null)
                {
                    StopCoroutine(statusCoroutine);
                    statusCoroutine = null;
                }
                if (receivingMsg != null)
                {
                    StopCoroutine(receivingMsg);
                    receivingMsg = null;
                }
                CNC.sendLine = 0;
                CNC.reciveLine = 0;
                statePI = false;
                ShowHideButtonPI();
            }
        }
        //Application.Quit();
    }

    /*
     * Esta funcion es para interactuar al enviar por bluetooth 
     * Dato a enviar     |   Aumenta el contador o no (por defecto, sí).
     * De momento solo acepta una instruccion terminado con "\n" 
     * Al enviar mas de de 1 puede generar problemas
     */
    public void SendText(string dataSend, bool Cnt = true)
    {
        if (string.IsNullOrEmpty(dataSend))
            return;
        if (IsConnected)// y no error o alarm. 
        {
            dataSend = dataSend.Replace("\n", "");
            dataSend = dataSend.Replace("\r", "");
            dataSend += "\n";
            if (!Cnt)
            {
                BluetoothService.WritetoBluetooth(dataSend);
            }
            else
            {
                // wait until recieve mesage and then send
                //editar
                while(true)// bucle hasta que sean iguales 
                {
                    if (CNC.sendLine == CNC.reciveLine)
                        break;
                }
                sendComand = true;
                Debug.Log("Envia:" + dataSend.Replace("\n", ""));
                BluetoothService.WritetoBluetooth(dataSend);
                CNC.sendLine++;
            }
        }
        else
        {
            BluetoothService.Toast("Dispositivo no conectado...");
            ShowInConsole("Dispositivo no conectado ....");
        }
    }

    public void RecieveText()
    {
        if (IsConnected)
        {
            try
            {
                // Reed data response
                string dataResponse = BluetoothService.ReadFromBluetooth();
                if (!string.IsNullOrEmpty(dataResponse))
                {
                    Debug.Log("Se recibio: " + dataResponse);
                    bool statusReportMsg = false;
                    string[] lines = dataResponse.Split(newlineChars, System.StringSplitOptions.RemoveEmptyEntries);//, System.StringSplitOptions.RemoveEmptyEntries
                    foreach (string line in lines) 
                    {
                        Debug.Log(line);
                        if (line == "Grbl 1.2h ['$' for help]") // despues del mensaje de bienvenida
                        {
                            ShowInConsole(line);
                            robotStatus = "Inactivo";
                        }
                        else if (line == "ok")
                        {
                            if (!statusReportMsg && sendComand)
                            {
                                ShowInConsole(line);
                                sendComand = false;
                                CNC.reciveLine++;
                            }
                            else
                            {
                                statusReportMsg = false;
                            }
                        }
                        else if (line.StartsWith("error:"))
                        {
                            errorN = int.Parse(line.Substring(6));
                            ShowInConsole(line);
                            CNC.reciveLine++;// Se recibio pero falta funcion para continuar operacion ahora simplemente salta al siguiente codigo.
                        }
                        else if (line.StartsWith("d:")) //lee la distancia (cm)
                        {
                            if (sendComand)
                            {
                                //string dis = line;
                                float dis = float.Parse(line.Replace("cm", "").Replace("d:", ""));
                                Debug.Log("Los cm son: " + dis.ToString());
                                //CNC.distanceRef = (1.1114f * (dis) + 0.4189f) * 10.0f;
                                //CNC.distanceRef = (float)((0.0046 * (Math.Pow((double)dis, 2) + (dis * 0.9752) + 2.2299)) * 10.0);
                                CNC.disSharp = (float)(((0.0046 * (Math.Pow(dis, 2))) + (dis * 0.9752) + 2.2299)) * 10;//-0.0248
                                Debug.Log("Los mm son: " + CNC.disSharp);
                                ShowInConsole(CNC.disSharp.ToString());
                                CNC.reciveLine++;
                                sendComand = false;
                                break;
                            }
                        }
                        else if (line.StartsWith("s:")) // lee la confirmaacion
                        {
                            //string conf = line.Replace("s:", ""); 
                            CNC.seedConfirmed = int.Parse(line.Substring(2));
                            ShowInConsole(line);
                        }
                        else if (line.StartsWith("["))// sirve para mostrar mensajes "push grbl"
                        {
                            if (line == "[MSG:Reset to continue]")
                            {
                                CNC.sendLine = 0;
                                CNC.reciveLine = 0;
                                // sacar la interrupcion 
                            }
                            ShowInConsole(line);
                        }
                        else if (line.StartsWith("<")) // de momento solo funcional con $10=1 y retorna un ok igual
                        {
                            statusReportMsg = true;
                            // Eliminar los caracteres "<" y ">" alrededor de la cadena
                            string data = line.TrimStart('<').TrimEnd('>');

                            // Dividir la cadena en partes usando "|" como separador
                            string[] partes = data.Split('|');

                            // Procesar cada parte
                            // Check Status
                            robotStatus = partes[0];
                            textStatus.text = partes[0];

                            // Procesar MPos
                            //if (partes[1].StartsWith(Mpos"))
                            string[] mposPartes = partes[1].Split(':')[1].Split(',');
                            CNC.MPos.x = float.Parse(mposPartes[0]);
                            CNC.MPos.y = float.Parse(mposPartes[1]);
                            CNC.MPos.z = float.Parse(mposPartes[2]);

                            // Procesar FS
                            string[] fsPartes = partes[2].Split(':')[1].Split(',');
                            for (int j = 0; j < fsPartes.Length; j++)
                            {
                                CNC.FS[j] = int.Parse(fsPartes[j]);
                            }
                            AddDataToList(CNC.MPos, CNC.FS[0]);
                            /*// Procesar WCO
                            string[] wcoPartes = partes[3].Split(':')[1].Split(',');
                            CNC.MPos.x = float.Parse(wcoPartes[0]);
                            CNC.MPos.y = float.Parse(wcoPartes[1]);
                            CNC.MPos.z = float.Parse(wcoPartes[2]);*/
                            //Debug.Log(line);
                            //break;
                            //i++;
                        }
                        else // sirve para mostrar otros mensajes
                        {
                            ShowInConsole(line);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Log(e);
                // BluetoothService.Toast("Device no Conected");
                // consoleText.text += "\n Dispositivo no Conectado";
                StopButton();
            }
        }
    }
    public void SendButton()
    {
        string s = dataToSend.text.ToString();
        /*ShowInConsole(s);
        SendText(s);*/
        SendCommand(s);
        dataToSend.text = "";
    }
    public void SendCommand(string s)
    {
        ShowInConsole(s);
        sendTextData = StartCoroutine(SendCommands(s));
    }
    public void ExternSendCommand(string s, bool sConsole = true)
    {
        if (sConsole)
        {
            ShowInConsole(s);
        }
        sendTextData = StartCoroutine(SendCommands(s));
    }
    IEnumerator SendCommands(string s)
    {
        SendText(s);
        yield return WaitUntilLinesMatch();
        //StopCoroutine(sendTextData);
        sendTextData = null;
    }

    /*
     * Estas funciones es para interactuar con la consola al mostrar Texto o Limpiar la consola
     */
    public void ShowInConsole(string text_show)
    {
        consoleText.text += text_show + "\n";
    }
    public void CleanConsole()
    {
        consoleText.text = "";
    }
    /// <summary>
    /// Funciones de ejcuecion de Grbl
    /// SoftReset
    /// Unlock
    /// Home
    /// </summary>

    public void SoftRest()
    {
        byte b = 0x18;
        Debug.Log("softreset");
        CNC.reciveLine = 0;
        CNC.sendLine = 0;
        SendText(Encoding.ASCII.GetString(new byte[] { b }));
    }
    public void Unlock()
    {
        Debug.Log("unlock");
        SendText("$X");
    }
    // Funciones correspondientes al Home
    public void CallHoming()
    {
        if (callHoming == null)
        {
            Debug.Log("Empezo corutina homing");
            callHoming = StartCoroutine(Homing());
        }
    }
    public IEnumerator Homing()
    {
        while (true)
        {
            if (!CNC.homing)// si no esta realizando home
            {
                if (statusCoroutine != null)
                {
                    StopCoroutine(statusCoroutine);
                    statusCoroutine = null;
                }
                Debug.Log("Realizando home... ");
                SendText(CNC.Home());
                CNC.homing = true;
            }
            yield return new WaitForSecondsRealtime(1.0f);
            if (CNC.reciveLine == CNC.sendLine)
            {
                if (statusCoroutine == null)
                {
                    Debug.Log("Empezo corutina status");
                    statusCoroutine = StartCoroutine(GetStatus());
                }
                CNC.homing = false;
                StopCoroutine(callHoming);
                callHoming = null;
                break;
            }
        }
    }
    // Funciones correspondientes a la rutina de siembra 
    IEnumerator SendingMessage()
    {
        
        //CNC.sendLine++;
        Debug.Log("Empieza la corrutina del ciclo de trabajo");

        yield return WaitUntilLinesMatch();

        callHoming = StartCoroutine(Homing());

        yield return WaitUntilLinesMatch();
        autoRute = true;

        SendText(CNC.InitialConfig());//absolutas | mm
        Debug.Log("Realizo Initial Config");
        yield return WaitUntilLinesMatch();
        Debug.Log("mov abajo -150");
        SendText("G01 Z-150 " + CNC.SpeedAxis("z") + "M03 S128");
        yield return WaitUntilLinesMatch();
        yield return WaitUntilIdle();

        for (int i = 0; i <= CNC.seedPointsList.Count; i++)
        {
            //Vector2 point = CNC.SeedPoints[i];
            Plant p = CNC.seedPointsList[i].GetComponent<Plant>();
            if (p.addList)
            {
                Vector2 point = new Vector2(p.GetPosition().x, p.GetPosition().y);

                Debug.Log("X-" + point.x + ", Y-" + point.y);
                SendText("G01 " + "X" + point.x + " Y" + point.y + CNC.SpeedAxis("xy"));
                yield return WaitUntilLinesMatch();
                yield return WaitUntilIdle();
                //Debug.Log("midiendo");
                SendText("M16");
                // esperar a por la distancia nueva
                yield return WaitUntilLinesMatch();
                yield return WaitUntilIdle();
                string dep = CNC.CalculateDeep();
                Debug.Log("moviendo a Z: " + dep);
                SendText("G01 Z" + dep + CNC.SpeedAxis("z")); // el valor es negativo no necesita (-)
                yield return WaitUntilLinesMatch();
                yield return WaitUntilIdle();
                Debug.Log("Sembrando");
                SendText("M15");
                // hasta tener respuesta
                yield return WaitUntilLinesMatch();
                yield return WaitUntilIdle();
                CNC.SeedPlant(i);
                SendText("G01 Z" + CNC.RetracionZ() + CNC.SpeedAxis("z") + "S128");
                yield return WaitUntilLinesMatch();
                yield return WaitUntilIdle();
            }
            else
            {
                yield return new WaitForSeconds(0.1f);
            }
        }

        //Agregar comando de fin de programa
        SendText("M5 S0");
        // Finalizo exitosamente la corrutina
        Debug.Log("Fin de la rutina programada");
        yield return new WaitForSeconds(1.0f);
        autoRute = false;
        
        StopCoroutine(statusCoroutine);
        statusCoroutine = null;
        StopCoroutine(sendingMsg);// ojo aqui
        sendingMsg = null;
    }
    IEnumerator WaitUntilLinesMatch()
    {
        while (true)
        {
            if (CNC.reciveLine == CNC.sendLine)
            {
                yield break;
            }
            yield return new WaitForSeconds(.1f);
        }
    }
    IEnumerator WaitUntilIdle()
    {
        while (true)
        {
            yield return new WaitForSeconds(.1f);
            if (autoRute && robotStatus == "Idle")
            {
                yield break;
            }
        }
    }
    IEnumerator RecieveMessage()
    {
        while (true)
        {
            RecieveText();
            yield return new WaitForSeconds(.1f);
        }
    }
    IEnumerator GetStatus()
    {
        while (true)
        {
            SendGetStatus();
            CNC.UpdateCoorBox();
            Debug.Log("R: " + CNC.reciveLine + " S: " + CNC.sendLine + CNC.MPos.ToString());
            //Debug.Log("Get Status.....");
            yield return new WaitForSeconds(0.1f);
        }
    }
    public void StartSeederRutine()
    {
        if (statusCoroutine == null)
        {
            Debug.Log("Empezo corutina status");
            statusCoroutine = StartCoroutine(GetStatus());
        }
        if (sendingMsg == null)
        {
            Debug.Log("Empezo la ruta");
            sendingMsg = StartCoroutine(SendingMessage());
        }
    }
    //Funciones del test de velocidad 
    public void TestRutine()
    {
        if (statusCoroutine == null)
        {
            Debug.Log("Empezo corutina status");
            statusCoroutine = StartCoroutine(GetStatus());
        }
        if (testRutine == null)
        {
            Debug.Log("Empezo la ruta");
            testRutine = StartCoroutine(TestRutineC());
        }
    }
    IEnumerator TestRutineC()
    {
        Debug.Log("Empieza Test de Velocidad");
        yield return WaitUntilLinesMatch();
        callHoming = StartCoroutine(Homing());
        yield return WaitUntilLinesMatch();
        autoRute = true;

        SendText(CNC.InitialConfig());//absolutas | mm
        Debug.Log("Realizo Initial Config");
        yield return WaitUntilLinesMatch();
        Debug.Log("mov X -590");
        SendText("G01 X-600 " + CNC.SpeedAxis("x"));
        yield return WaitUntilLinesMatch();
        yield return WaitUntilIdle();
        Debug.Log("mov Y-440");
        SendText("G01 Y-440 " + CNC.SpeedAxis("y"));
        yield return WaitUntilLinesMatch();
        yield return WaitUntilIdle();
        Debug.Log("mov Z-240");
        SendText("G01 Z-240 " + CNC.SpeedAxis("z"));
        yield return WaitUntilLinesMatch();
        yield return WaitUntilIdle();

        Debug.Log("Fin de la rutina programada");
        yield return new WaitForSeconds(1.0f);
        autoRute = false;

        StopCoroutine(statusCoroutine);
        statusCoroutine = null;
        StopCoroutine(sendingMsg);// ojo aqui
        sendingMsg = null;
        yield break;
    }
    // Funciones correspondientes al status
    public void SendGetStatus()
    {
        SendText("?", false);
    }
    public void SumarRecieve()
    {
        Debug.Log("R: " + CNC.reciveLine + " S: " + CNC.sendLine);
        CNC.reciveLine++;
        Debug.Log("R: " + CNC.reciveLine + " S: " + CNC.sendLine);
    }
    // CSV funtions
    public void InitCSV()
    {
        filePath = Path.Combine(Application.persistentDataPath, "data.csv");
        // Agregar encabezados
        rowData.Add(new string[] { "Time", "PosX", "PosY", "PosZ","VelX", "VelY", "VelZ", "VelAvance" });
    }

    private void AddDataToList(Vector3 newv3, int vM)
    {
        if(oldTime == 0)
        {
            oldTime = Time.time;
            oldv3 = newv3;
        }
        if (oldTime == Time.time)
        {
            return;
        }
        float deltaT = Time.time - oldTime;
        oldTime = Time.time;
        Vector3 vel = (newv3 - oldv3) / deltaT;
        /*float x = (newv3.x - oldv3.x) / deltaT;
        float y = (newv3.y - oldv3.y) / deltaT;
        float z = (newv3.z - oldv3.z) / deltaT;*/
        oldv3 = newv3;
        //float velM = 0f;
        rowData.Add(new string[] { Time.time.ToString(), newv3.x.ToString("F4"), newv3.y.ToString("F4"), newv3.z.ToString("F4"), vel.x.ToString("F4"), vel.y.ToString("F4"), vel.z.ToString("F4"), vM.ToString() });
    }

    public void WriteCSV()
    {
        // Crear una cadena para almacenar los datos en formato CSV
        string delimiter = ",";
        StringBuilder sb = new StringBuilder();

        // Convertir cada fila de datos a una línea CSV y agregarla al StringBuilder
        foreach (var row in rowData)
        {
            sb.AppendLine(string.Join(delimiter, row));
        }

        // Escribir la cadena en el archivo CSV
        StreamWriter outStream = System.IO.File.CreateText(filePath);
        outStream.WriteLine(sb);
        outStream.Close();

        Debug.Log("CSV file written to: " + filePath);
    }
// Plant info funtions
public void ShowHideButtonPI()
    {
        if (transitionPI.GetCurrentAnimatorStateInfo(0).IsName("HidePI"))
        {
            StartCoroutine(ShowPlantInfo());
        }
        else
        {
            StartCoroutine(HidePlantInfo());
        }
    }

    IEnumerator ShowPlantInfo()
    {
        transitionPI.SetTrigger("ShowPI");
        yield return new WaitForSeconds(1f);
    }

    IEnumerator HidePlantInfo()
    {
        transitionPI.SetTrigger("HidePI");
        yield return new WaitForSeconds(1f);
    }
}
// Hacer funcional Recieveline an send line 