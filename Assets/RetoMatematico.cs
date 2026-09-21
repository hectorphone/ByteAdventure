using UnityEngine;

public class RetoMatematico : MonoBehaviour
{
    [SerializeField] private GameObject puerta;
    public int indice;
    public string pregunta = "¿Cuánto es 2 + 3?";
    public int[] opciones = { 4, 5, 6 };
    public int respuesta = 5;
    public string pista = "Empieza en 2 y cuenta tres pasos: 3, 4, 5.";
    public bool Completado { get; private set; }
    public void Configurar(GameObject barrera) { puerta = barrera; }
    public bool Responder(int valor)
    {
        if (Completado) return true;
        if (valor != respuesta) return false;
        Completado = true;
        if (puerta != null) puerta.SetActive(false);
        return true;
    }
    public void Reiniciar()
    {
        Completado = false;
        if (puerta != null) puerta.SetActive(true);
    }
}
