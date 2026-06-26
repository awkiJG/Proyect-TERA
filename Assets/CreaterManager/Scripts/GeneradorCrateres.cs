using UnityEngine;
using System.Collections.Generic;

// Estructura de datos serializable para poder verla en el inspector si fuera necesario.
// Almacena la información matemática básica de cada cráter antes de instanciarlo.
[System.Serializable]
public struct DatosCrater
{
    public Vector2 posicion; // Posición (X, Y) del centro del cráter en el mundo.
    public float radio;      // Radio del cráter, usado para calcular la escala (diámetro).

    // Constructor para inicializar rápidamente los datos del cráter.
    public DatosCrater(Vector2 posicion, float radio)
    {
        this.posicion = posicion;
        this.radio = radio;
    }
}

public class GeneradorCrateres : MonoBehaviour
{
    // Referencia al prefab visual (Sprite de un círculo) que se instanciará para representar los cráteres.
    public GameObject prefabCirculoVisual;

    // Arreglo que almacena los datos matemáticos calculados de los cráteres de la iteración actual.
    private DatosCrater[] crateres;
    
    // Lista para guardar las referencias a los GameObjects instanciados en la escena.
    // Esto permite limpiarlos (destruirlos) fácilmente antes de generar un nuevo set.
    private List<GameObject> circulosInstanciados = new List<GameObject>();

    void Start()
    {
        // Al iniciar el juego, generamos los datos matemáticos y luego instanciamos los visuales.
        GenerarDatosMatematicos();
        InstanciarCratelesVisuales();
    }

    void Update()
    {
        // Detectar si el jugador presiona la tecla Espacio para resetear y volver a generar los cráteres.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerarDatosMatematicos();
            InstanciarCratelesVisuales();
        }
    }

    // Método encargado única y exclusivamente de calcular posiciones y tamaños (radios)
    // aplicando lógica condicional y trigonometría para evitar superposiciones.
    private void GenerarDatosMatematicos()
    {
        // Determina de forma aleatoria cuántos cráteres se van a generar (entre 1 y 4).
        int cantidadCrateres = Random.Range(1, 5);
        
        // Inicializa el arreglo con el tamaño exacto de la cantidad decidida.
        crateres = new DatosCrater[cantidadCrateres];

        if (cantidadCrateres == 1)
        {
            // Caso 1 Cráter: Solo un cráter grande en el centro exacto (0,0).
            float radioCentral = Random.Range(12f, 15f);
            crateres[0] = new DatosCrater(Vector2.zero, radioCentral);
        }
        else if (cantidadCrateres == 2)
        {
            // Caso 2 Cráteres: Un cráter central y uno secundario adherido a su borde.
            
            // 1. Crear el cráter central.
            float radioCentral = Random.Range(11f, 14f);
            crateres[0] = new DatosCrater(Vector2.zero, radioCentral);

            // 2. Crear el segundo cráter.
            float radioSecundario = Random.Range(6f, 8f);
            float anguloGrados = Random.Range(0f, 360f); // Ángulo aleatorio en 360 grados.
            float anguloRadianes = anguloGrados * Mathf.Deg2Rad; // Conversión a radianes para Mathf.Sin/Cos.
            
            // La posición se calcula usando trigonometría (Seno y Coseno) multiplicada por el radio central.
            // Esto asegura que el centro del segundo cráter quede exactamente sobre la circunferencia del primero.
            Vector2 posicionSecundario = new Vector2(Mathf.Cos(anguloRadianes), Mathf.Sin(anguloRadianes)) * radioCentral;

            crateres[1] = new DatosCrater(posicionSecundario, radioSecundario);
        }
        else if (cantidadCrateres == 3)
        {
            // Caso 3 Cráteres: Un central y dos periféricos. El tercero debe evitar chocar con el segundo.
            
            // 1. Cráter central.
            float radioCentral = Random.Range(10f, 13f);
            crateres[0] = new DatosCrater(Vector2.zero, radioCentral);

            // 2. Segundo cráter (periférico).
            float radioSecundario = Random.Range(5f, 7f);
            float anguloGrados2 = Random.Range(0f, 360f);
            float anguloRadianes2 = anguloGrados2 * Mathf.Deg2Rad;
            Vector2 posicionSecundario = new Vector2(Mathf.Cos(anguloRadianes2), Mathf.Sin(anguloRadianes2)) * radioCentral;
            crateres[1] = new DatosCrater(posicionSecundario, radioSecundario);

            // 3. Tercer cráter.
            float radioTerciario = Random.Range(5f, 7f);
            Vector2 posicionTerciario = Vector2.zero;
            
            bool colisiona = true;
            int maxIntentos = 1000; // Límite de seguridad para evitar un bucle infinito (while loop).
            int intentos = 0;

            // Bucle que busca una posición válida para el tercer cráter hasta que deje de colisionar.
            while (colisiona && intentos < maxIntentos)
            {
                // Calcula una posición aleatoria sobre el perímetro del cráter central.
                float anguloGrados3 = Random.Range(0f, 360f);
                float anguloRadianes3 = anguloGrados3 * Mathf.Deg2Rad;
                posicionTerciario = new Vector2(Mathf.Cos(anguloRadianes3), Mathf.Sin(anguloRadianes3)) * radioCentral;

                // Validación matemática usando distancia (Teorema de Pitágoras con Vector2.Distance).
                // Si la distancia entre los centros es MAYOR O IGUAL a la suma de sus radios, NO colisionan.
                if (Vector2.Distance(posicionSecundario, posicionTerciario) >= (radioSecundario + radioTerciario))
                {
                    colisiona = false; // Posición válida encontrada, salir del bucle.
                }
                intentos++;
            }

            crateres[2] = new DatosCrater(posicionTerciario, radioTerciario);
        }
        else if (cantidadCrateres == 4)
        {
            // Caso 4 Cráteres: Un central y tres periféricos. 
            // El tercero evita al segundo. El cuarto evita al segundo y al tercero.
            
            // 1. Cráter central.
            float radioCentral = Random.Range(9f, 12f);
            crateres[0] = new DatosCrater(Vector2.zero, radioCentral);

            // 2. Segundo cráter.
            float radioSecundario = Random.Range(5f, 7f);
            float anguloGrados2 = Random.Range(0f, 360f);
            float anguloRadianes2 = anguloGrados2 * Mathf.Deg2Rad;
            Vector2 posicionSecundario = new Vector2(Mathf.Cos(anguloRadianes2), Mathf.Sin(anguloRadianes2)) * radioCentral;
            crateres[1] = new DatosCrater(posicionSecundario, radioSecundario);

            // 3. Tercer cráter (se verifica que no choque con el segundo).
            float radioTerciario = Random.Range(5f, 7f);
            Vector2 posicionTerciario = Vector2.zero;
            bool colisionaTercero = true;
            int maxIntentos = 1000;
            int intentos = 0;

            while (colisionaTercero && intentos < maxIntentos)
            {
                float anguloGrados3 = Random.Range(0f, 360f);
                float anguloRadianes3 = anguloGrados3 * Mathf.Deg2Rad;
                posicionTerciario = new Vector2(Mathf.Cos(anguloRadianes3), Mathf.Sin(anguloRadianes3)) * radioCentral;

                // Comprobar colisión con el cráter 2.
                if (Vector2.Distance(posicionSecundario, posicionTerciario) >= (radioSecundario + radioTerciario))
                {
                    colisionaTercero = false;
                }
                intentos++;
            }
            crateres[2] = new DatosCrater(posicionTerciario, radioTerciario);

            // 4. Cuarto cráter (se verifica que no choque ni con el segundo ni con el tercero).
            float radioCuarto = Random.Range(5f, 7f);
            Vector2 posicionCuarto = Vector2.zero;
            bool colisionaCuarto = true;
            intentos = 0; // Reiniciamos el contador para este nuevo bucle.

            while (colisionaCuarto && intentos < maxIntentos)
            {
                float anguloGrados4 = Random.Range(0f, 360f);
                float anguloRadianes4 = anguloGrados4 * Mathf.Deg2Rad;
                posicionCuarto = new Vector2(Mathf.Cos(anguloRadianes4), Mathf.Sin(anguloRadianes4)) * radioCentral;

                // Calculamos si hay suficiente espacio entre el 4to y el 2do, y entre el 4to y el 3ro.
                bool noChocaConSegundo = Vector2.Distance(posicionSecundario, posicionCuarto) >= (radioSecundario + radioCuarto);
                bool noChocaConTercero = Vector2.Distance(posicionTerciario, posicionCuarto) >= (radioTerciario + radioCuarto);

                // Solo si NO choca con NINGUNO de los dos anteriores, la posición es válida.
                if (noChocaConSegundo && noChocaConTercero)
                {
                    colisionaCuarto = false;
                }
                intentos++;
            }
            crateres[3] = new DatosCrater(posicionCuarto, radioCuarto);
        }
    }

    // Método encargado de tomar los datos matemáticos y generar una representación visual en la escena.
    private void InstanciarCratelesVisuales()
    {
        // Primero, limpiamos los círculos instanciados en una generación anterior (Reset).
        // Recorremos la lista de objetos guardados.
        foreach (GameObject go in circulosInstanciados)
        {
            if (go != null)
            {
                Destroy(go); // Destruye el GameObject de la escena.
            }
        }
        circulosInstanciados.Clear(); // Vaciamos la lista para empezar de cero.

        // Validación de seguridad para evitar errores si olvidamos asignar el prefab en Unity.
        if (prefabCirculoVisual == null)
        {
            Debug.LogWarning("Prefab Círculo Visual no asignado en el Inspector.");
            return;
        }

        // Iteramos sobre todos los cráteres generados matemáticamente.
        foreach (DatosCrater crater in crateres)
        {
            // Instanciar el prefab en su posición (X, Y). Z se mantiene en 0 para un entorno 2D (top-down).
            Vector3 posicionInstanciacion = new Vector3(crater.posicion.x, crater.posicion.y, 0f);
            
            // Se instancia el prefab, sin rotación (Quaternion.identity), y haciéndolo hijo de este GameObject (transform).
            GameObject craterVisual = Instantiate(prefabCirculoVisual, posicionInstanciacion, Quaternion.identity, transform);
            
            // Guardamos la referencia en la lista para poder borrarlo en el futuro (cuando se presione Espacio).
            circulosInstanciados.Add(craterVisual);

            // Escalar a partir del diámetro. Como el radio es la mitad del ancho, multiplicamos por 2.
            float diametro = crater.radio * 2f;
            // Aplicamos la escala en X e Y (ancho y alto). Mantenemos la escala Z original del prefab.
            craterVisual.transform.localScale = new Vector3(diametro, diametro, craterVisual.transform.localScale.z);

            // Como solo es visual, nos aseguramos de que no incluya componentes de colisión por accidente.
            // Si tiene un Collider2D, lo eliminamos.
            Collider2D col2D = craterVisual.GetComponent<Collider2D>();
            if (col2D != null)
            {
                Destroy(col2D);
            }
            // Si tiene un Collider 3D, también lo eliminamos.
            Collider col3D = craterVisual.GetComponent<Collider>();
            if (col3D != null)
            {
                Destroy(col3D);
            }
        }
    }
}
