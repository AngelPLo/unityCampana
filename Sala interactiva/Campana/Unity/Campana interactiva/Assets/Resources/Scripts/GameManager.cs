using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;

public static class GameManager
{
    // Puerto Serial a configurar en sitio 
    public static SerialPort PuertoSerial;
    public static bool PuertoConfigurado = false;
    public static bool EnComunicacion = false;
    public static string NombrePuerto = "";
    public static int BAUDS = 9600;

    //Factor de escalado en GUI
    public static float EscaladoX = 1;
    public static float EscaladoY = 1;

    //Variables de la sala
    public static float anguloCampana = 0;

    //Esado de conexión al Broker
    public static bool ConectadoABroker = false;

    //comandos
    public static List<string> cmds = new List<string>();
    
}
