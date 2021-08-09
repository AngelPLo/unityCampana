using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using UnityEngine.UI;
using TMPro;

public class Configuration : MonoBehaviour
{
    //Objetos de juego y componentes
    public GameObject PanelConfiguracion;
    public GameObject PortsCombo;
    public GameObject ResCombo;
    public GameObject BaudInput;
    TMP_Dropdown ResolucionC;
    TMP_Dropdown PortsC;
    TMP_InputField BaudI;

    //Scripts (clase)
    DataAcquisition ComunicacionSerial;

    //Variables
    string[] PuertosDisponibles;
    string Baudios;
    int ResIndex;
    List<string> ListadoPuertos;
    List<string> ListadoResoluciones;
    
    //Variables de dispositivo
    Resolution[] Resoluciones;

    void Awake()
    {
        // Buscamos el script de Adquisición de datos en el GO MainCamera 
        ComunicacionSerial = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<DataAcquisition>();

        // Asignamos los componentes a sus variables
        ResolucionC = ResCombo.GetComponent<TMP_Dropdown>();
        PortsC = PortsCombo.GetComponent<TMP_Dropdown>();
        BaudI = BaudInput.GetComponent<TMP_InputField>();

        /// DEBUG
        if (!ResolucionC || !PortsC || !BaudI)
        {
            Debug.LogError("No se encontró alguna de las listas de opciones");
        }

        //Obtenemos los nombres de los puertos seriales disponibles
        PuertosDisponibles = SerialPort.GetPortNames();

        //Generamos la lista de opciones con los puertos disponibles
        ListadoPuertos = new List<string> { "Seleccione Puerto" };
        ListadoPuertos.AddRange(PuertosDisponibles);

        //Obtenemos los nombres de los puertos seriales disponibles
        Resoluciones = Screen.resolutions;

        //Generamos la lista de opciones con las resoluciones
        ListadoResoluciones = new List<string>{"Seleccione Resolucion"};
        for (int i = 0; i < Resoluciones.Length; i++)
        {
            string Res = Resoluciones[i].width + "x" + Resoluciones[i].height;
            ListadoResoluciones.Add(Res);

            if (Resoluciones[i].height == Screen.currentResolution.height && 
                Resoluciones[i].width == Screen.currentResolution.width)
            {
                ResIndex = i;
            }
        }

    }
    // Start is called before the first frame update
    void Start()
    {
        //Hacemos que las opciones de puertos se actualicen en el listado de opciones
        PortsC.ClearOptions();
        PortsC.AddOptions(ListadoPuertos);
        
        //Hacemos que las opciones de resoluciones se actualicen en el listado de opciones
        ResolucionC.ClearOptions();
        ResolucionC.AddOptions(ListadoResoluciones);
        ResolucionC.value = ResIndex;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OcultarConfiguracion()
    {
        if (GameManager.PuertoConfigurado)
        {
           //ComunicacionSerial.AbrirPuertoSerial();
            StartCommunication();
            gameObject.SetActive(false);
        }
    }

    public void MostrarConfiguracion()
    {
        gameObject.SetActive(true);
    }
    public void AsignarNombrePuerto()
    {
        GameManager.NombrePuerto = ListadoPuertos[PortsC.value];
        Debug.Log(GameManager.NombrePuerto);
    }
    public void AsignarBaudios()
    {
        bool BaudioValido;
        BaudioValido = int.TryParse(BaudI.text, out GameManager.BAUDS);
        if (!BaudioValido) 
            Debug.LogWarning("BaudRate no valido, se mantiene 9600");
        else
            Debug.Log(GameManager.BAUDS);
    }
    public void StartCommunication()
    {
        GameManager.PuertoSerial.DiscardOutBuffer();
        GameManager.PuertoSerial.WriteLine("START");
        GameManager.EnComunicacion = true;
    }
    public void StopCommunication()
    {
        GameManager.PuertoSerial.DiscardOutBuffer();
        GameManager.PuertoSerial.WriteLine("STOP");
    }
    public void RescaleGUI()
    {
        Debug.Log("Resolución anterior:" + Screen.currentResolution);
        //Coloca la resolución del juego a la resolución escogida
        Screen.SetResolution(Resoluciones[ResolucionC.value-1].width, Resoluciones[ResolucionC.value-1].height, true);

       //Hace el factor de escalado
        GameManager.EscaladoX = Screen.width / 1920;
        GameManager.EscaladoY = Screen.height / 1080;

        //Escala los elementos del panel de configuración
        //PanelConfiguracion.transform.localScale = new Vector3(GameManager.EscaladoX, GameManager.EscaladoY, 1);

        //Si se necesitan reescalar más GUI objects se pueden ir agregando acá
        Debug.Log("Resolución actualizada: " + Screen.currentResolution);
    }
}
