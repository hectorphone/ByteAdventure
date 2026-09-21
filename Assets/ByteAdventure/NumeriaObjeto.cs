using UnityEngine;
public class NumeriaObjeto : MonoBehaviour
{
    public enum Tipo { Carga, Grua, Baliza, Semilla, Jardin, Distribuidor, Recuerdo }
    public Tipo tipo;
    public int valor;
    public string nombre;
    public TextMesh rotulo;
    [System.NonSerialized] public bool recogido;
}
