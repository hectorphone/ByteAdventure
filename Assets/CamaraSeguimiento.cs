using UnityEngine;
public class CamaraSeguimiento : MonoBehaviour
{
    [SerializeField] private Transform objetivo;
    [SerializeField] private Vector3 desplazamiento = new Vector3(0, 4, -6);
    [SerializeField] private float suavidad = 5;
    public bool vistaAventura;
    public Transform Encuadre { get; set; }
    private Vector3 velocity;
    private void LateUpdate()
    {
        if (objetivo == null) return;
        if (!vistaAventura)
        {
            transform.position = Vector3.Lerp(transform.position, objetivo.position + desplazamiento, suavidad * Time.deltaTime);
            transform.LookAt(objetivo); return;
        }
        Vector3 focus = Encuadre != null ? Encuadre.position : objetivo.position + new Vector3(0, .2f, 2.2f);
        Vector3 offset = Encuadre != null ? new Vector3(12, 20, -25) : new Vector3(0, 10, -12);
        transform.position = Vector3.SmoothDamp(transform.position, focus + offset, ref velocity, .16f, Mathf.Infinity, Time.unscaledDeltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(focus - transform.position), 1 - Mathf.Exp(-10 * Time.unscaledDeltaTime));
    }
}

