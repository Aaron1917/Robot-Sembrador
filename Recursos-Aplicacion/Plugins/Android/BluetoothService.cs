using System;
using UnityEngine;


public class BluetoothService
{
    private static AndroidJavaClass unityPlayer;
    private static AndroidJavaObject activity;
    private static AndroidJavaObject context;
    private static AndroidJavaClass unitybluetooth;
    private static AndroidJavaObject BluetoothConnector;
    //private static AndroidJavaClass HandlerThreadHandler;

    public static bool ConnectionStatus = false;
    private static string bufferText;


    // creating an instance of the bluetooth class from the plugin 
    public static void CreateBluetoothObject()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            context = activity.Call<AndroidJavaObject>("getApplicationContext");
            //unity3dbluetoothplugin = new AndroidJavaClass("com.example.unity3dbluetoothplugin.BluetoothConnector");
            unitybluetooth = new AndroidJavaClass("com.example.unitybluetooth.PluginBluetooth");
            BluetoothConnector = unitybluetooth.CallStatic<AndroidJavaObject>("getInstance");
        }
    }

    public static string[] GetBluetoothDevices()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                return BluetoothConnector.Call<string[]>("GetBluetoothDevices");
            }
            catch (Exception e)
            {
                Toast("No Device found");
                return null;
            }
        }

        return null;

    }

    // starting bluetooth connection with device named "DeviceName"
    // print the status on the screen using native android Toast
    public static bool StartBluetoothConnection(string DeviceName)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                string connectionStatus = BluetoothConnector.Call<string>("StartBluetoothConnection", DeviceName);
                Toast("Iniciando conexión, Estado: " + connectionStatus); // Start connection status:
                if (connectionStatus == "Connected")
                {
                    ConnectionStatus = true;
                    return true;
                }
                else if (connectionStatus == "Error")
                {
                    return false;
                }

            }
            catch (Exception e)
            {
                Debug.Log(e);
                Toast("Error al conectar"); //Start connection error
            }
        }
        return false;
    }


    // should be called inside OnApplicationQuit
    // stop connection with the bluetooth device
    public static void StopBluetoothConnection()
    {

        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                BluetoothConnector.Call("StopBluetoothConnection");
                Toast("Desconectado"); //Connction stoped
                ConnectionStatus = false; // Disconected

            }
            catch (Exception e)
            {
                Debug.Log(e);
                Toast("Error al desconectar"); //Stop connction error
            }
        }
    }

    // write data as a string to the bluetooth device
    public static void WritetoBluetooth(string data)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                BluetoothConnector.Call("WriteData", data);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                Toast("Write data error");
            }
        }
    }


    //read data from the bluetooth device
    // if there is an error or there is no data coming, this method will return "" as an output
    public static string ReadFromBluetooth()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            try
            {
                return BluetoothConnector.Call<string>("ReadData");
            }
            catch (Exception e)
            {
                BluetoothConnector.Call("PrintOnScreen", context, "Read data error");
            }
        }
         
        return "";

    }

    public static void Toast(string data)
    {
        BluetoothConnector.Call("PrintOnScreen", context, data);
        Debug.Log(data);
    }
    public static bool BlueIsConnected()
    {
        return BluetoothConnector.Call<bool>("IsConnected");
    }
}
