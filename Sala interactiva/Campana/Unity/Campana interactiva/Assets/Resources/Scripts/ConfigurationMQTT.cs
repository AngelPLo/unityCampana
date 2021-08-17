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
        // Referencia el GO del panel de configuración como propiedad de la clase
        // GameManager
        GameManager.AttachGO();

        //Bloqueamos la orientación a portrait
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortraitUpsideDown = false;


        // Carga la dirección del broker y puerto a sus
        // InputFields correspondientes

        BrokerInput.text = GameManager.broker;
        PortsInput.text = GameManager.brokerPort;

        /// DEBUG
        if ( !PortsInput || !BrokerInput || !TopicInput || !TopicList)
        {
            Debug.LogError("No se encontró alguna de las listas de opciones");
        }

        //Conecta al broker y (automáticamente suscribe a los tópicos)
        MQTTclient.SetBrokerAddress(GameManager.broker);
        MQTTclient.SetBrokerPort(GameManager.brokerPort);
        MQTTclient.Connect();

    }
    // Actualiza el panel de configuración al entrar a la App
    void Start()
    {
        UpdateUI();
    }

    // Se actualiza el panel de configuración después de todos los Update 
    void LateUpdate()
    {
        if (GameManager.UpdateUI)
        {
            UpdateUI();
        }
    }

    //Oculta el panel de la configuración del GUI
    public void OcultarConfiguracion()
    {
        GameManager.OcultarConfiguracion();
    }

    //Muestra el panel de configuración del GUI
    public void MostrarConfiguracion()
    {
        GameManager.MostrarConfiguracion();
    }


    //Se encarga de reescalar el GUI (no se ocupa, en Android no hay cambio dinámico del tamaño de pantalla)
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

    //Hace actualización de los botones y texto de las cajas de configuración del panel
    void UpdateUI()
    {
        BrokerInput.text = GameManager.broker;
        PortsInput.text = GameManager.brokerPort;

        TopicList.ClearOptions();
        TopicList.AddOptions(GameManager.topicos);
        //TopicList.value = GameManager.valorDrop;

        ConnectDisconnect.interactable = true;

        // Si se está conectado al Broker se bloquea la edición de la dirección
        // y del puerto del Broker así como de los tópicos
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

        // Si no hay texto en la caja de texto de la dirección del broker o el puerto de acceso y
        // no está conectado entonces el botón de conectarse se bloquea, si está conectado no se 
        // bloquea para que se pueda desconectar
        if((BrokerInput.text == "" || PortsInput.text == "") && !GameManager.ConectadoABroker)
        {
            ConnectDisconnect.interactable = false;
        }

        //Si la caja de texto de los tópicos está vacía se bloquea el botón de suscripción
        if(TopicInput.text == "")
        {
            SubUnsub.interactable = false;
        }

        GameManager.UpdateUI = false;

    }

    // Actualiza el valor estático GameManager.topico (utilizado por el método de subscribeToTopic)
    // revisa si ya se está suscrito al tópico y actualiza el botón de suscripción acorde al resultado
    public void AsignarSub(string topico)
    {
        GameManager.topico = topico;
        GameManager.SubUnsub = true;
        if (GameManager.topicos.Contains(topico)) GameManager.SubUnsub = false;
        //Debug.LogFormat(GameManager.topico + ",{0}",GameManager.SubUnsub);
        GameManager.UpdateUI = true;

    }

    // Actualiza el valor de la caja de texto de los tópicos de acuerdo al valor seleccionado del
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
