using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public static class GameManager
{
    //

    // Puerto Serial a configurar en sitio 
    public static SerialPort PuertoSerial;
    public static bool PuertoConfigurado = false;
    public static bool EnComunicacion = false;
    public static string NombrePuerto = "";
    public static int BAUDS = 9600;

    //UI
    static GameObject ConfigPanel;
    public static bool UpdateUI = false;

    //Factor de escalado en GUI
    public static float EscaladoX = 1;
    public static float EscaladoY = 1;

    //Variables de la sala
    public static float anguloCampana = 0;

    //Esado de conexión al Broker
    public static bool ConectadoABroker = false;


    //Acción SUB UNSUB SUB = true; UNSUB = false
    public static bool SubUnsub = true;


    //Topico a suscribir
    public static string topico = "";

    //Valor del Dropdown
    public static int valorDrop = 0;

    //comandos
    public static List<string> cmds = new List<string>();

    //Dirección del broker y tópicos
    public static string broker = "iot.inventoteca.com";
    public static string brokerPort = "1883";
    public static List<string> topicos = new List<string>() { "topicos suscritos" };

    //Metodos
    public static void AttachGO()
    {
        ConfigPanel = GameObject.FindGameObjectWithTag("configuracion");
    }

    public static void OcultarConfiguracion()
    {
        if (ConectadoABroker)
        {
            ConfigPanel.SetActive(false);
        }
    }
    public static void MostrarConfiguracion()
    {
        ConfigPanel.SetActive(true);
    }
}
