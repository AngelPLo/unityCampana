using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BellControl : MonoBehaviour
{
    // Rigidbody _rbBell; Creo que no hay xD
    // Al final eliminaré aquello que no se haya utilizado
    GameObject Campanario_GO;
    GameObject Campana_GO;
    GameObject Badajo_GO;
    GameObject SoporteSup_GO;
    GameObject SoporteInf_GO;
    GameObject EnsambleSup_GO;
    Rigidbody Badajo_RB;
    Collider Badajo_Coll;
    Collider Campana_Coll;
    AudioSource GeneracionSonido;
    AudioClip SonidoCampana;
    AudioClip[] NEGAS;
    float[] _angulo = new float[2] { 0, 0 };

  
    void Awake()
    {
        //Se hacen las asignaciones de las variables y propiedades que podemos ocupar
        Campanario_GO = gameObject;
        EnsambleSup_GO = GameObject.Find("CampanaYSoporte").gameObject;
        SoporteInf_GO = GameObject.Find("SoporteInferior").gameObject;
        Badajo_GO = GameObject.Find("Badajo").gameObject;
        SoporteSup_GO = GameObject.Find("SoporteSuperior").gameObject;
        Campana_GO = GameObject.Find("Campana").gameObject;

        Badajo_RB = Badajo_GO.GetComponent<Rigidbody>();
        Badajo_Coll = Badajo_GO.GetComponent<Collider>();
        Campana_Coll = Campana_GO.GetComponent<Collider>();

        GeneracionSonido = Campanario_GO.GetComponent<AudioSource>();
        NEGAS = new AudioClip[4];
        
    }
    // Start is called before the first frame update
    void Start()
    {
        string Archivo;
        for (int i = 0; i <4; i++)
        {
            Archivo = "Audio/NEGAS" + i;
            NEGAS[i] = Resources.Load<AudioClip>(Archivo);
        }
        Archivo = "Audio/CAMPANA";
        SonidoCampana = Resources.Load<AudioClip>(Archivo);
        if (!NEGAS[1] || !NEGAS[2] || !NEGAS[3]) Debug.LogError("No se cargó el NEGAS :c");
        if (!SonidoCampana) Debug.LogError("Error cargando el sonido de la campana");
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.ConectadoABroker)
        {
            float rotacion = GetRotation();
            EnsambleSup_GO.transform.Rotate(Vector3.right, rotacion);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (GeneracionSonido.isPlaying) GeneracionSonido.Stop();
        //GeneracionSonido.clip = NEGAS[Random.Range(0, 3)];
        GeneracionSonido.clip = SonidoCampana;
        GeneracionSonido.volume = 1;
        GeneracionSonido.Play();
    }

    public float GetRotation()
    {
        _angulo[0] = GameManager.anguloCampana;
        float rotacion = _angulo[0] - _angulo[1];
        _angulo[1] = _angulo[0];
        return rotacion;
    }
}
