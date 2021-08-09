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
        string[] topicos = {
            "prueba/M2MQTT/campana/angulo", //topico de recepcion de datos
            "prueba/M2MQTT/campana/cmd",    //topico de recepción de comandos
                            };

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
        public void SetBrokerAddress(string brokerAddress)
        {
            if (broker && !updateUI)
            {
                this.brokerAddress = brokerAddress;
                Debug.Log(this.brokerAddress);
            }
        }

        //Se carga el valor del puerto del broker en memoria (como propiedad de la clase base M2MQTTUnityClient)
        public void SetBrokerPort(string brokerPort)
        {
            if (puerto && !updateUI)
            {
                int.TryParse(brokerPort, out this.brokerPort);
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
        }

        // Método para suscribirte al tópico "topico". Se llama en la función OnConnected de la clase
        // base M2MqttUnityClient
        protected override void SubscribeTopics()
        {
            //Se suscribe al tópico del ángulo de la campana
            client.Subscribe(new string[] { topicos[0] }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
            //Se suscribe al tópico de comandos
            client.Subscribe(new string[] { topicos[1] }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }

        // Método para des-suscribirte al tópico "topico". Se llama en la función CloseConnection de la
        // clase base M2MqttUnityClient
        protected override void UnsubscribeTopics()
        {
            //Se des-suscribe de los topicos suscritos anteriormente
            client.Unsubscribe(new string[] { topicos[0] });
            client.Unsubscribe(new string[] { topicos[1] });
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

        // Publica el 'mensaje' al 'tópico' con un 'QoS'
        public void Publish(string topico, string mensaje, byte QoS)
        {
            byte[] msg = System.Text.Encoding.UTF8.GetBytes(mensaje);
            client.Publish(topico, msg, QoS, false);
            Debug.LogFormat("Publicado: {0}, en {1}", mensaje, topico);
        }
        
        //Publica un mensaje "PruebaCMD" en el tópico de comandos con un QoS = 2;
        public void TestPublish()
        {
            Publish(topicos[1], "pruebaCMD", MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE);
        }

        //Obtiene el mensaje como string y lo anexa a la lista eventMessages
        protected override void DecodeMessage(string topic, byte[] message)
        {
            string msg = System.Text.Encoding.UTF8.GetString(message);
            Debug.Log("Received: " + msg);
            StoreMessage(msg);

            //Si el mensaje proviene del topico cmd, añade el mensaje a la cola de comandos
            if (topic == topicos[1])
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
            if (cmd == "EXIT") { Debug.Log("Saliendo de App"); }
            if (cmd == "PAUSE") { Debug.Log("App en pausa"); }
            if (cmd == "PLAY") { Debug.Log("Reanudando"); }
            if (cmd.Contains("M:"))
            {
                int lenght = cmd.Length;
                string msg = cmd.Substring("M:".Length, lenght - "M:".Length);
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






