/*
The MIT License (MIT)

Copyright (c) 2018 Giovanni Paolo Vigano'

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

//librerias de sistema
using System;
using System.Collections;
using System.Collections.Generic;

//librerias de Unity
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//librerias para MQTT
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using M2MqttUnity;


/// <summary>
/// Examples for the M2MQTT library (https://github.com/eclipse/paho.mqtt.m2mqtt),
/// </summary>
namespace M2MqttUnity.Examples
{
    /// <summary>
    /// Script for testing M2MQTT with a Unity UI
    /// </summary>
    public class M2MqttComm : M2MqttUnityClient
    {
        [Tooltip("Set this to true to perform a testing cycle automatically on startup")]
        public bool autoTest = false;

        [Header("User Interface")]

       
        //string[] topicos = {
        //    "prueba/M2MQTT/campana/angulo", //topico de recepcion de datos
        //    "prueba/M2MQTT/campana/cmd",    //topico de recepción de comandos
        //                    };

        // propiedades de clase
        public TMP_InputField consoleInputField;
        public TMP_InputField puerto;
        public TMP_InputField broker;
        public Button conectar;
        public Button desconectar;
        public Button testPublishButton;

        //mensaje de prueba
        string mensaje = "hola";

        private List<string> eventMessages = new List<string>();
        private bool updateUI = false;

        
        #region conexionBroker

        //Se carga la dirección IP  del Broker a memoria (como propiedad de la clase base M2MQTTUnityClient)
        //Solo se ocupa cuando se quiera cambiar el broker
        //Broker default: 'iot.inventoteca.com'
        public void SetBrokerAddress(string brokerAddress)
        {
            if (broker && !updateUI)
            {
                GameManager.broker = brokerAddress;
                this.brokerAddress = GameManager.broker;
                Debug.Log(this.brokerAddress);
                GameManager.UpdateUI = true;
            }
        }

        //Se carga el valor del puerto del broker en memoria (como propiedad de la clase base M2MQTTUnityClient)
        //Solo se ocupa cuando se quiera cambiar el puerto
        //Puerto default: '1883'
        public void SetBrokerPort(string brokerPort)
        {
            if (puerto && !updateUI)
            {
                int.TryParse(brokerPort, out this.brokerPort);
                GameManager.brokerPort = this.brokerPort.ToString();
                GameManager.UpdateUI = true;
            }
        }




        protected override void OnConnecting()
        {
            base.OnConnecting();
            SetUiMessage("Connecting to broker on " + brokerAddress + ":" + brokerPort.ToString() + "...\n");
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            SetUiMessage("Connected to broker on " + brokerAddress + "\n");
            GameManager.ConectadoABroker = true;
            GameManager.UpdateUI = true;
        }


        // Método para suscribirte al tópico "topico" con QoS = QoS, llamado si se quiere agregar un tópico extra a 
        // las suscripciones
        void SubscribeToTopic(string topico, byte QoS)
        {
            GameManager.topicos.Add(topico);
            client.Subscribe(new string[] { topico }, new byte[] { QoS });
            GameManager.UpdateUI = true;
        }


        // Método para suscribirse a los tópicos presentes en la Lista de tópicos del GameManager
        // Se llama en la función OnConnected de la clase base M2MQTTUnityClient
        protected override void SubscribeTopics()
        {
            int nTopicos = GameManager.topicos.Count;
            //Si no hay ningún tópico suscrito se suscribe a los obligatorios (angulo y cmd)
            if (nTopicos == 1)
            {
                string[] topicos = new string[] { "prueba/M2MQTT/campana/angulo",
                                                  "prueba/M2MQTT/campana/cmd" };
                GameManager.topicos.AddRange(topicos);
                nTopicos = 3;
            }

            //Se suscribe a todos los tópicos de la lista con un QoS = 2
            for(int i = 1; i < nTopicos; i++)
            {
                client.Subscribe(new string[] { GameManager.topicos[i] }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            }
            GameManager.UpdateUI = true;

        }

        // Método para des-suscribirte al tópico "topico" 
        void UnsubscribeToTopic(string topico)
        {
            if(topico != "topicos suscritos")
            {
                GameManager.topicos.Remove(topico);
                client.Unsubscribe(new string[] { topico });
                GameManager.UpdateUI = true;
            }
        }

        // Método para des-suscribirte a todos los tópicos. Se llama en la función CloseConnection de la
        // clase base M2MqttUnityClient o de forma manual.
        protected override void UnsubscribeTopics()
        {
            int nTopicos = GameManager.topicos.Count;
            for (int i  = 0;  i < nTopicos; i++)
            {
                client.Unsubscribe(new string[] { GameManager.topicos[i] });
            }
            GameManager.topicos.Clear();
            GameManager.topicos.Add("topicos suscritos");
            GameManager.UpdateUI = true;
        }

        //Se ejecuta cuando la conección falla 
        protected override void OnConnectionFailed(string errorMessage)
        {
            AddUiMessage("CONNECTION FAILED! " + errorMessage);
        }

        // Se ejecuta cuando se desconecta del broker
        protected override void OnDisconnected()
        {
            AddUiMessage("Disconnected.");
            GameManager.ConectadoABroker = false;
        }

        //Se ejecuta cuando se pierde la conexión
        protected override void OnConnectionLost()
        {
            AddUiMessage("CONNECTION LOST!");
            GameManager.ConectadoABroker = false;
        }

        #endregion

        #region MonoBehaviour

        protected override void Awake()
        {
            base.Awake();
            SetBrokerAddress(GameManager.broker);
            SetBrokerPort(GameManager.brokerPort);
        }

        protected override void Start()
        {
            SetUiMessage("Ready.");
            updateUI = true;
            base.Start();
        }

        protected override void Update()
        {
            float valorRecibido = 0;

            base.Update(); // call ProcessMqttEvents()

            if (eventMessages.Count > 0)
            {
                //Procesa los mensajes de la lista eventMessages
                foreach (string msg in eventMessages)
                {
                    valorRecibido = ProcessMessage(msg);
                    
                }
                //Si el último valor procesado es >= 0 lo envía al valor del ángulo de la campana
                if (valorRecibido >= 0)
                {
                    GameManager.anguloCampana = valorRecibido;
                }
                else
                {
                    //Sino muestra un error de que el parseo falló
                    Debug.LogError("Valor no parseable");
                }

                //limpia la lista de eventMessages
                eventMessages.Clear();
            }
            if (updateUI)
            {
                UpdateUI();
            }


        }

        //Se ejecuta después del Update cada frame
        void LateUpdate()
        {
            //Procesa los comandos
            processCmds();
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        private void OnValidate()
        {
            if (autoTest)
            {
                autoConnect = true;
            }
        }


        #endregion

        #region Comunicacion

        // Publica el 'mensaje' al 'tópico' con un 'QoS' sin bandera de retención
        public void Publish(string topico, string mensaje, byte QoS)
        {
            byte[] msg = System.Text.Encoding.UTF8.GetBytes(mensaje);
            client.Publish(topico, msg, QoS, false);
            Debug.LogFormat("Publicado: {0}, en {1}", mensaje, topico);
        }
        
        //Publica un mensaje "PruebaCMD" en el tópico de comandos con un QoS = 2;
        //public void TestPublish()
        //{
        //    Publish(topicos[1], "pruebaCMD", MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE);
        //}

        //Obtiene el mensaje como string y lo anexa a la lista eventMessages
        protected override void DecodeMessage(string topic, byte[] message)
        {
            string msg = System.Text.Encoding.UTF8.GetString(message);
            Debug.Log("Received: " + msg);
            StoreMessage(msg);

            //Si el mensaje proviene del topico cmd, añade el mensaje a la cola de comandos
            if (topic == "prueba/M2MQTT/campana/cmd")
            {
                GameManager.cmds.Add(msg);
            }
        }

        private void StoreMessage(string eventMsg)
        {
            eventMessages.Add(eventMsg);
        }

        //Se usará para proporcionar el valor del mensaje al script de control de campana
        private float ProcessMessage(string msg)
        {
            float valorRecibido = 0;
            AddUiMessage("Received: " + msg);
            //Si el valor recibido es parseable a float
            if (float.TryParse(msg ,out valorRecibido))
            {
                //retorna el valor recibido como float
                return valorRecibido;
            }

            //sino retorna -1
            return -1;
        }

        #endregion

        #region UI

        public void SetUiMessage(string msg)
        {
            if (consoleInputField != null)
            {
                consoleInputField.text = msg;
                updateUI = true;
            }
        }

        public void AddUiMessage(string msg)
        {
            if (consoleInputField != null)
            {
                consoleInputField.text += msg + "\n";
                updateUI = true;
            }
        }

        //Función que muestra los mensajes de MQTT en un espacio de texto en el UI de la app.
        private void UpdateUI()
        {
            if (client == null)
            {
                if (conectar != null)
                {
                    conectar.interactable = true;
                    desconectar.interactable = false;
                    testPublishButton.interactable = false;
                }
            }
            else
            {
                if (testPublishButton != null)
                {
                    testPublishButton.interactable = client.IsConnected;
                }
                if (desconectar != null)
                {
                    desconectar.interactable = client.IsConnected;
                }
                if (conectar != null)
                {
                    conectar.interactable = !client.IsConnected;
                }
            }
            if (broker != null && conectar != null)
            {
                broker.interactable = conectar.interactable;
                broker.text = brokerAddress;
            }
            if (puerto != null && conectar != null)
            {
                puerto.interactable = conectar.interactable;
                puerto.text = brokerPort.ToString();
            }
            if (/*clearButton != null &&*/ conectar != null)
            {
                //clearButton.interactable = conectar.interactable;
            }
            updateUI = false;
        }

        public void ConnectDisconnect()
        {
            //Si está conectado al Broker lo desconecta, si no, conecta
            if (GameManager.ConectadoABroker)
            {
                Disconnect();
            }
            else
            {
                Connect();
            }
        }

        public void SubUnsub()
        {
            if (!GameManager.SubUnsub)
            {
                if (GameManager.topicos.Contains(GameManager.topico))
                {
                    int indice = GameManager.topicos.IndexOf(GameManager.topico);
                    UnsubscribeToTopic(GameManager.topicos[indice]);
                    GameManager.valorDrop = 0;
                    Debug.Log("Desuscrito");
                }
            }
            else
            {
                SubscribeToTopic(GameManager.topico, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE);
                Debug.Log("suscrito");
            }
            GameManager.UpdateUI = true;
        }

        #endregion

        #region comandos

        private void processCmds()
        {
            if(GameManager.cmds.Count > 0)
            {
                foreach (string cmd in GameManager.cmds)
                {
                    executeCMD(cmd);
                }
                GameManager.cmds.Clear();
            }
            
        }

        private void executeCMD(string cmd)
        {
            string command = cmd.ToUpper();
            // Proponer una lista de comandos para ejecutar a distancia
            if (command == "EXIT") { Debug.Log("Saliendo de App"); }
            if (command == "PAUSE") { Debug.Log("App en pausa"); }
            if (command == "PLAY") { Debug.Log("Reanudando"); }
            if (command.Contains("M:"))
            {
                int lenght = command.Length;
                string msg = command.Substring("M:".Length, lenght - "M:".Length);
                Debug.Log("mensaje: " + msg);
            }
        }

        #endregion


    }




}




//Metodo para publicar mensajes




//public void SetClientID(int ID)
//{
//    client.ClientId
//}

//Se carga el valor de la dirección IP del Broker en memoria


//public void SetEncrypted(bool isEncrypted)
//{
//    this.isEncrypted = isEncrypted;
//}






