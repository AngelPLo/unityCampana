using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using UnityEngine.UI;
using TMPro;

public class ConfigurationMQTT : MonoBehaviour
{
    //Objetos de UI y componentes
    public TMP_InputField PortsInput;
    public TMP_InputField BrokerInput;
    public TMP_InputField TopicInput;
    public Button ConnectDisconnect;
    public Button SubUnsub;
    public TMP_Dropdown TopicList;

    //Variables
    string[] PuertosDisponibles;
    string Baudios;
    int ResIndex;
    List<string> ListadoResoluciones;
    
    //Variables de dispositivo
    Resolution[] Resoluciones;

    void Awake()
    {
        // Carga la dirección del broker y puerto a sus
        // InputFields correspondientes

        BrokerInput.text = GameManager.broker;
        PortsInput.text = GameManager.brokerPort;

        /// DEBUG
        if ( !PortsInput || !BrokerInput || !TopicInput || !TopicList)
        {
            Debug.LogError("No se encontró alguna de las listas de opciones");
        }

    }
    // Start is called before the first frame update
    void Start()
    {
        UpdateUI();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (GameManager.UpdateUI)
        {
            UpdateUI();
        }
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

    void UpdateUI()
    {
        Debug.Log(TopicList.value);
        BrokerInput.text = GameManager.broker;
        PortsInput.text = GameManager.brokerPort;
        TopicList.ClearOptions();
        TopicList.AddOptions(GameManager.topicos);
        TopicList.value = GameManager.valorDrop;
        ConnectDisconnect.interactable = true;
        // Si se está conectado al Broker se bloquea la edición de la dirección
        // y del puerto del Broker así como de los tópicos
        if (!GameManager.ConectadoABroker)
        {
            BrokerInput.interactable = true;
            PortsInput.interactable = BrokerInput.interactable;
            TopicInput.interactable = false;
            SubUnsub.interactable = false;
            TopicList.interactable = false;
            //ConnectDisconnect.gameObject.GetComponent<>().color = new Color(109, 133, 179, 180);
            //ConnectDisconnect.image.color = new Color(109, 133, 179, 180);
            ConnectDisconnect.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Conectar";
            
        }
        else
        {
            BrokerInput.interactable = false;
            PortsInput.interactable = BrokerInput.interactable;
            TopicInput.interactable = true;
            SubUnsub.interactable = true;
            TopicList.interactable = true;
            //ConnectDisconnect.image.color = new Color(255, 47, 0, 180);
            ConnectDisconnect.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Desconectar";
        }

        if(!GameManager.SubUnsub)
        {
            //GameManager.topicos.IndexOf()
            int indiceLista = TopicList.value;
            TopicInput.text = TopicInput.text;
            //TopicInput.text = GameManager.topicos[indiceLista];
            //SubUnsub.image.color = new Color(255, 47, 0, 180);
            SubUnsub.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Unsub";
        }
        else
        {
            TopicInput.text = TopicInput.text;
            //SubUnsub.image.color = new Color(255, 47, 0, 180);
            SubUnsub.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Sub";
        }

        if(BrokerInput.text == "" || PortsInput.text == "")
        {
            ConnectDisconnect.interactable = false;
        }
        if(TopicInput.text == "")
        {
            SubUnsub.interactable = false;
        }
        GameManager.UpdateUI = false;

    }


    public void AsignarSub(string topico)
    {
        GameManager.topico = topico;
        //TopicList.value = 0;
        GameManager.SubUnsub = true;
        if (GameManager.topicos.Contains(topico)) GameManager.SubUnsub = false;
        Debug.LogFormat(GameManager.topico + ",{0}",GameManager.SubUnsub);
        GameManager.UpdateUI = true;

    }

    public void Dropdown(int value)
    {
        GameManager.valorDrop = value;
        //GameManager.UpdateUI = true;
        //GameManager.SubUnsub = true;
        if(value != 0)
        {
            TopicInput.text = TopicList.options[value].text;
        }
        Debug.Log("Hola");
        //if (GameManager.topicos.Contains(TopicList.options[value].text)) GameManager.SubUnsub = false;
    }
}
