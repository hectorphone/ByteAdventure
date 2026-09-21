using UnityEngine;
using UnityEngine.InputSystem;

public class NumeriaDemo : MonoBehaviour
{
    public ByteController jugador;
    public RetoMatematico[] retos;
    public Transform portal;
    public Transform glitch;
    private Vector3 inicio;
    private Quaternion giro;
    private int pantalla; // 0 introduction, 1 exploration, 2 challenge, 3 ending, 4 pause
    private RetoMatematico activo;
    private string mensaje = "";
    private bool acierto;
    public int Resueltos { get { int n = 0; foreach (var r in retos) if (r.Completado) n++; return n; } }
    private void Start()
    {
        inicio = jugador.transform.position; giro = jugador.transform.rotation;
        CambiarPantalla(0);
    }
    private void CambiarPantalla(int valor)
    {
        pantalla = valor;
        jugador.enabled = valor == 1;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
    private RetoMatematico Cercano()
    {
        foreach (var r in retos)
            if (!r.Completado && Vector3.Distance(jugador.transform.position, r.transform.position) < 3.1f) return r;
        return null;
    }
    private void Update()
    {
        if (glitch != null) glitch.localRotation = Quaternion.Euler(0, Mathf.Sin(Time.time * 1.5f) * 12, Mathf.Sin(Time.time * 3) * 6);
        var k = Keyboard.current;
        if (k == null) return;
        if (k.escapeKey.wasPressedThisFrame)
        {
            if (pantalla == 1) CambiarPantalla(4);
            else if (pantalla == 2 || pantalla == 4) CambiarPantalla(1);
        }
        if (pantalla == 1)
        {
            if (jugador.transform.position.y < -5) Reposicionar();
            var cerca = Cercano();
            if (cerca != null && k.eKey.wasPressedThisFrame)
            {
                activo = cerca; mensaje = ""; acierto = false; CambiarPantalla(2);
            }
            if (Resueltos == retos.Length && Vector3.Distance(jugador.transform.position, portal.position) < 2.2f) CambiarPantalla(3);
        }
        else if (pantalla == 2 && !acierto)
        {
            if (k.digit1Key.wasPressedThisFrame) Responder(0);
            else if (k.digit2Key.wasPressedThisFrame) Responder(1);
            else if (k.digit3Key.wasPressedThisFrame) Responder(2);
        }
        else if (k.enterKey.wasPressedThisFrame && (pantalla == 0 || (pantalla == 2 && acierto))) CambiarPantalla(1);
    }
    public void Responder(int opcion)
    {
        acierto = activo.Responder(activo.opciones[opcion]);
        mensaje = acierto ? "¡Correcto! Circuito restaurado y puerta abierta." : "Todavía no. Pista: " + activo.pista;
    }
    private void Reposicionar()
    {
        var cc = jugador.GetComponent<CharacterController>();
        cc.enabled = false; jugador.transform.SetPositionAndRotation(inicio, giro); cc.enabled = true;
    }
    public void Reiniciar()
    {
        foreach (var r in retos) r.Reiniciar();
        Reposicionar(); activo = null; acierto = false; mensaje = ""; CambiarPantalla(0);
    }
    private void OnDestroy() { if (jugador != null) jugador.enabled = true; }
    private void OnGUI()
    {
        float escala = Mathf.Min(Screen.width / 1000f, Screen.height / 700f);
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1000 * escala) / 2, (Screen.height - 700 * escala) / 2), Quaternion.identity, Vector3.one * escala);
        var texto = new GUIStyle(GUI.skin.label) { fontSize = 22, wordWrap = true, richText = true };
        var titulo = new GUIStyle(texto) { fontSize = 34, fontStyle = FontStyle.Bold };
        var boton = new GUIStyle(GUI.skin.button) { fontSize = 23, wordWrap = true };
        GUI.backgroundColor = new Color(0.08f, 0.15f, 0.29f, 0.98f);
        GUI.Box(new Rect(20, 18, 960, 90), "");
        GUI.Label(new Rect(40, 25, 650, 38), "BYTE ADVENTURE  /  NUMERIA", titulo);
        GUI.Label(new Rect(40, 68, 920, 35), "WASD · Mover    E · Terminal    Esc · Pausa                 Circuitos " + Resueltos + " / 3", texto);
        if (pantalla == 1)
        {
            string aviso = Cercano() != null ? "Pulsa E para reparar este circuito" : Resueltos == 3 ? "¡Numeria restaurada! Entra en el portal dorado." : "Sigue la ruta luminosa hasta el siguiente terminal.";
            GUI.Box(new Rect(110, 615, 780, 62), ""); GUI.Label(new Rect(135, 627, 730, 42), aviso, texto); return;
        }
        GUI.Box(new Rect(140, 150, 720, 430), "");
        if (pantalla == 0)
        {
            GUI.Label(new Rect(175, 175, 650, 55), "Glitch ha desordenado Numeria", titulo);
            GUI.Label(new Rect(175, 245, 650, 210), "GLITCH: «¡He cambiado todos los números! Ahora tengo… ¿tres narices? Un momento…»\n\nByte, repara los tres circuitos con sumas, restas y grupos. Cada respuesta abre el camino. Si fallas, tendrás una pista y podrás volver a intentarlo.", texto);
            if (GUI.Button(new Rect(265, 490, 470, 58), "Entrar en Numeria  ·  Enter", boton)) CambiarPantalla(1);
        }
        else if (pantalla == 2)
        {
            GUI.Label(new Rect(175, 170, 650, 42), "CIRCUITO " + (activo.indice + 1), titulo);
            GUI.Label(new Rect(175, 225, 650, 70), activo.pregunta, texto);
            GUI.enabled = !acierto;
            for (int i = 0; i < activo.opciones.Length; i++)
                if (GUI.Button(new Rect(180 + i * 215, 305, 200, 60), (i + 1) + "  →  " + activo.opciones[i], boton)) Responder(i);
            GUI.enabled = true;
            GUI.Label(new Rect(180, 390, 640, 95), mensaje, texto);
            if (GUI.Button(new Rect(265, 495, 470, 55), acierto ? "Continuar  ·  Enter" : "Volver a explorar", boton)) CambiarPantalla(1);
        }
        else if (pantalla == 3)
        {
            GUI.Label(new Rect(175, 185, 650, 55), "¡Numeria vuelve a brillar!", titulo);
            GUI.Label(new Rect(175, 265, 650, 170), "Has restaurado los tres circuitos.\n\nGLITCH: «¡Exijo una revancha! En cuanto encuentre el botón… ¿era este tostador?»\n\nGracias por ayudar a Byte.", texto);
            if (GUI.Button(new Rect(265, 490, 470, 58), "Jugar otra vez", boton)) Reiniciar();
        }
        else
        {
            GUI.Label(new Rect(175, 200, 650, 55), "Pausa", titulo);
            if (GUI.Button(new Rect(265, 305, 470, 58), "Continuar", boton)) CambiarPantalla(1);
            if (GUI.Button(new Rect(265, 395, 470, 58), "Reiniciar demo", boton)) Reiniciar();
        }
    }
}
