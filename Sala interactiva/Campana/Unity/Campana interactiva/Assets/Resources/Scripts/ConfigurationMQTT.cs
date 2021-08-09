using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using UnityEngine.UI;
using TMPro;

public class ConfigurationMQTT : MonoBehaviour
{
    //Objetos de juego y componentes
    public GameObject PortsInput;
    public GameObject BrokerInput;
    TMP_InputField BrokerI;
    TMP_InputField PortsI;

    //Variables
    string[] PuertosDisponibles;
    string Baudios;
    int ResIndex;
    List<string> ListadoResoluciones;
    
    //Variables de dispositivo
    Resolution[] Resoluciones;

    void Awake()
    {
        // Asignamos los componentes a sus variables
        PortsI = PortsInput.GetComponent<TMP_InputField>();
        BrokerI = BrokerInput.GetComponent<TMP_InputField>();

        /// DEBUG
        if ( !PortsI || !BrokerI)
        {
            Debug.LogError("No se encontró alguna de las listas de opciones");
        }

    }
    // Start is called before the first frame update
    void Start()
    {
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OcultarConfiguracion()
    {
        if (GameManager.ConectadoABroker)
        {
            gameObject.SetActive(false);
        }
    }

    public void MostrarConfiguracion()
    {
        gameObject.SetActive(true);
    }

    public void RescaleGUI()
    {
        Debug.Log("Resolución anterior:" + Screen.currentResolution);
        //Coloca la resolución del juego a la resolución escogida
        //Screen.SetResolution(Resoluciones[ResolucionC.value-1].width, Resoluciones[ResolucionC.value-1].height, true);

       //Hace el factor de escalado
        GameManager.EscaladoX = Screen.width / 1920;
        GameManager.EscaladoY = Screen.height / 1080;

        //Escala los elementos del panel de configuración
        //PanelConfiguracion.transform.localScale = new Vector3(GameManager.EscaladoX, GameManager.EscaladoY, 1);

        //Si se necesitan reescalar más GUI objects se pueden ir agregando acá
        Debug.Log("Resolución actualizada: " + Screen.currentResolution);
    }
}
