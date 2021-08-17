//librerias del sistema
using System.Collections;
using System.Collections.Generic;
using System.IO.Ports;

//Librerias de Unity
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//Librerias de MQTT
using M2MqttUnity.Examples;

public class ConfigurationMQTT : MonoBehaviour
{
    //Objetos de UI y componentes
    public TMP_InputField PortsInput;
    public TMP_InputField BrokerInput;
    public TMP_InputField TopicInput;
    public Button ConnectDisconnect;
    public Button SubUnsub;
    public TMP_Dropdown TopicList;
    public M2MqttComm MQTTclient;
    public GameObject Stats;


    //Variables
    string[] PuertosDisponibles;
    string Baudios;
    int ResIndex;
    List<string> ListadoResoluciones;
    
    //Variables de dispositivo
    Resolution[] Resoluciones;

    void Awake()
    {
        // Referencia el GO del panel de configuraci�n como propiedad de la clase
        // GameManager
        GameManager.AttachGO();

        //Bloqueamos la orientaci�n a portrait
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortraitUpsideDown = false;


        // Carga la direcci�n del broker y puerto a sus
        // InputFields correspondientes

        BrokerInput.text = GameManager.broker;
        PortsInput.text = GameManager.brokerPort;

        /// DEBUG
        if ( !PortsInput || !BrokerInput || !TopicInput || !TopicList)
        {
            Debug.LogError("No se encontr� alguna de las listas de opciones");
        }

        //Conecta al broker y (autom�ticamente suscribe a los t�picos)
        MQTTclient.SetBrokerAddress(GameManager.broker);
        MQTTclient.SetBrokerPort(GameManager.brokerPort);
        MQTTclient.Connect();

    }
    // Actualiza el panel de configuraci�n al entrar a la App
    void Start()
    {
        UpdateUI();
    }

    // Se actualiza el panel de configuraci�n despu�s de todos los Update 
    void LateUpdate()
    {
        if (GameManager.UpdateUI)
        {
            UpdateUI();
        }
    }

    //Oculta el panel de la configuraci�n del GUI
    public void OcultarConfiguracion()
    {
        GameManager.OcultarConfiguracion();
    }

    //Muestra el panel de configuraci�n del GUI
    public void MostrarConfiguracion()
    {
        GameManager.MostrarConfiguracion();
    }


    //Se encarga de reescalar el GUI (no se ocupa, en Android no hay cambio din�mico del tama�o de pantalla)
    public void RescaleGUI()
    {
        Debug.Log("Resoluci�n anterior:" + Screen.currentResolution);
        //Coloca la resoluci�n del juego a la resoluci�n escogida
        //Screen.SetResolution(Resoluciones[ResolucionC.value-1].width, Resoluciones[ResolucionC.value-1].height, true);

       //Hace el factor de escalado
        GameManager.EscaladoX = Screen.width / 1920;
        GameManager.EscaladoY = Screen.height / 1080;

        //Escala los elementos del panel de configuraci�n
        //PanelConfiguracion.transform.localScale = new Vector3(GameManager.EscaladoX, GameManager.EscaladoY, 1);

        //Si se necesitan reescalar m�s GUI objects se pueden ir agregando ac�
        Debug.Log("Resoluci�n actualizada: " + Screen.currentResolution);
    }

    //Hace actualizaci�n de los botones y texto de las cajas de configuraci�n del panel
    void UpdateUI()
    {
        BrokerInput.text = GameManager.broker;
        PortsInput.text = GameManager.brokerPort;

        TopicList.ClearOptions();
        TopicList.AddOptions(GameManager.topicos);
        //TopicList.value = GameManager.valorDrop;

        ConnectDisconnect.interactable = true;

        // Si se est� conectado al Broker se bloquea la edici�n de la direcci�n
        // y del puerto del Broker as� como de los t�picos
        if (!GameManager.ConectadoABroker)
        {
            BrokerInput.interactable = true;
            PortsInput.interactable = true;
            TopicInput.interactable = false;
            SubUnsub.interactable = false;
            TopicList.interactable = false;
            ConnectDisconnect.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Conectar";
            
        }
        else
        {
            BrokerInput.interactable = false;
            PortsInput.interactable = false;
            TopicInput.interactable = true;
            SubUnsub.interactable = true;
            TopicList.interactable = true;
            ConnectDisconnect.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Desconectar";
        }

        //Cambia el texto del boton de suscribirse/desuscribirse
        if(!GameManager.SubUnsub)
        {
            //int indiceLista = TopicList.value;
            TopicInput.text = TopicInput.text;
            SubUnsub.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Unsub";
        }
        else
        {
            TopicInput.text = TopicInput.text;
            SubUnsub.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Sub";
        }

        // Si no hay texto en la caja de texto de la direcci�n del broker o el puerto de acceso y
        // no est� conectado entonces el bot�n de conectarse se bloquea, si est� conectado no se 
        // bloquea para que se pueda desconectar
        if((BrokerInput.text == "" || PortsInput.text == "") && !GameManager.ConectadoABroker)
        {
            ConnectDisconnect.interactable = false;
        }

        //Si la caja de texto de los t�picos est� vac�a se bloquea el bot�n de suscripci�n
        if(TopicInput.text == "")
        {
            SubUnsub.interactable = false;
        }

        GameManager.UpdateUI = false;

    }

    // Actualiza el valor est�tico GameManager.topico (utilizado por el m�todo de subscribeToTopic)
    // revisa si ya se est� suscrito al t�pico y actualiza el bot�n de suscripci�n acorde al resultado
    public void AsignarSub(string topico)
    {
        GameManager.topico = topico;
        GameManager.SubUnsub = true;
        if (GameManager.topicos.Contains(topico)) GameManager.SubUnsub = false;
        //Debug.LogFormat(GameManager.topico + ",{0}",GameManager.SubUnsub);
        GameManager.UpdateUI = true;

    }

    // Actualiza el valor de la caja de texto de los t�picos de acuerdo al valor seleccionado del
    // Dropdown
    public void Dropdown(int value)
    {
        GameManager.valorDrop = value;
        if(value != 0)
        {
            TopicInput.text = TopicList.options[value].text;
        }
        Debug.Log("Hola");
    }

    public void OcultarMostrarStats(bool status)
    {
        Stats.SetActive(status);
    }

}
