using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class NumeriaPuente : MonoBehaviour
{
    public ByteController byteRobot;
    public CamaraSeguimiento camara;
    public NumeriaObjeto[] objetos;
    public Transform[] tramos;
    public Transform focoPuente, meta, glitch, cargaVisible;
    public TextMesh cargaTexto;
    public AudioClip recogerSonido, errorSonido, exitoSonido;
    public ParticleSystem chispas;
    public Renderer[] lucesJardines;
    public Material luzActiva;
    public int Etapa { get; private set; }
    public bool Finalizado { get; private set; }
    public int Carga { get; private set; }
    public int Energia { get; private set; }
    public int Semillas { get; private set; }
    public int[] Jardines { get; } = new int[3];
    public int Paso { get; private set; }
    public string Mensaje { get; private set; }
    public bool Intro { get; private set; } = true;
    public bool Pausa { get; private set; }
    public bool Cinematica { get; private set; }
    private AudioSource audioSource;
    private float mensajeHasta, tiempo, quieto;
    private int recuerdos;
    private bool cuaderno, silencio;
    private NumeriaObjeto cercano, ultimaBaliza;
    private Vector3 inicio = new Vector3(0, 1.05f, -16);
    private Material[] coloresIniciales;
    public string Objetivo => Etapa == 0 ? "1 / ENERGÍA · Lleva cargas que sumen 5 a la grúa oeste" : Etapa == 1 ? "2 / RUTA · Pisa las balizas en orden: 2, 4, 6, 8" : Etapa == 2 ? "3 / VIDA · Reparte 6 semillas: la misma cantidad en 3 jardines" : "EL PUENTE ESTÁ LISTO · Cruza hasta el faro";
    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>(); audioSource.spatialBlend = 0; audioSource.volume = .3f;
        coloresIniciales = new Material[lucesJardines.Length];
        for (int i = 0; i < lucesJardines.Length; i++) coloresIniciales[i] = lucesJardines[i].sharedMaterial;
        byteRobot.enabled = false; cargaVisible.gameObject.SetActive(false); camara.Encuadre = focoPuente;
        Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
    }
    public void Empezar() { Intro = false; camara.Encuadre = null; byteRobot.enabled = true; Aviso("Glitch: «¡He desenchufado el puente! Creo que también mi tostadora…»", 8); }
    private void Sonido(AudioClip clip) { if (!silencio && audioSource != null && clip != null) audioSource.PlayOneShot(clip); }
    private void Aviso(string texto, float duracion = 6) { Mensaje = texto; mensajeHasta = Time.unscaledTime + duracion; quieto = 0; }
    private float Distancia(Transform t) { return Vector2.Distance(new Vector2(t.position.x, t.position.z), new Vector2(byteRobot.transform.position.x, byteRobot.transform.position.z)); }
    public bool Disponible(NumeriaObjeto o)
    {
        if (!o.gameObject.activeInHierarchy || o.recogido) return false;
        if (o.tipo == NumeriaObjeto.Tipo.Recuerdo) return true;
        if (Etapa == 0) return o.tipo == NumeriaObjeto.Tipo.Carga || o.tipo == NumeriaObjeto.Tipo.Grua;
        if (Etapa == 2) return o.tipo == NumeriaObjeto.Tipo.Semilla || o.tipo == NumeriaObjeto.Tipo.Jardin || o.tipo == NumeriaObjeto.Tipo.Distribuidor;
        return false;
    }
    private void Update()
    {
        if (glitch != null) glitch.localRotation = Quaternion.Euler(0, Mathf.Sin(Time.time) * 15, Mathf.Sin(Time.time * 2) * 8);
        foreach (var o in objetos) if (o.rotulo != null && Camera.main != null) o.rotulo.transform.rotation = Camera.main.transform.rotation;
        if (cargaVisible.gameObject.activeSelf) { cargaVisible.position = byteRobot.transform.position + Vector3.up * 2.3f; cargaVisible.Rotate(0, 65 * Time.deltaTime, 0); }
        var k = Keyboard.current;
        if (k != null && k.mKey.wasPressedThisFrame) silencio = !silencio;
        if (Intro) { if (k != null && k.enterKey.wasPressedThisFrame) Empezar(); return; }
        if (Finalizado) return;
        if (k != null && k.escapeKey.wasPressedThisFrame && !Cinematica) { Pausa = !Pausa; byteRobot.enabled = !Pausa; }
        if (Pausa || Cinematica) return;
        if (k != null && k.tabKey.wasPressedThisFrame) cuaderno = !cuaderno;
        tiempo += Time.deltaTime; quieto += Time.deltaTime;
        if (byteRobot.transform.position.y < -4) { Recolocar(); Aviso("¡Te tengo! Has vuelto a tierra firme. Conservas todo lo que llevabas."); }
        cercano = null; float best = 2.35f;
        foreach (var o in objetos) if (Disponible(o)) { float distance = Distancia(o.transform); if (distance < best) { best = distance; cercano = o; } }
        if (k != null && k.eKey.wasPressedThisFrame && cercano != null) Interactuar(cercano);
        if (k != null && k.qKey.wasPressedThisFrame)
        {
            if (Etapa == 0 && Carga > 0) { DevolverCarga(); Aviso("Carga devuelta a su pedestal. Puedes elegir otra."); }
            else if (Etapa == 2 && cercano != null && cercano.tipo == NumeriaObjeto.Tipo.Jardin && Jardines[cercano.valor] > 0)
            { Jardines[cercano.valor]--; Semillas++; ActualizarJardin(cercano); Sonido(recogerSonido); }
        }
        if (Etapa == 1)
        {
            NumeriaObjeto pisada = null;
            foreach (var o in objetos) if (o.tipo == NumeriaObjeto.Tipo.Baliza && Distancia(o.transform) < 1.15f) pisada = o;
            if (pisada != null && pisada != ultimaBaliza) Pisar(pisada);
            ultimaBaliza = pisada;
        }
        if (Etapa == 3 && Distancia(meta) < 2.3f)
        {
            Finalizado = true; byteRobot.enabled = false; Sonido(exitoSonido); Efecto(meta.position);
        }
        if (quieto > 35) Pista();
    }
    private NumeriaObjeto cargaOrigen;
    public void Interactuar(NumeriaObjeto o)
    {
        if (Intro || Pausa || Cinematica || Finalizado || !Disponible(o) || Distancia(o.transform) > 2.4f) return;
        switch (o.tipo)
        {
            case NumeriaObjeto.Tipo.Carga:
                if (Carga != 0) { Aviso("Llevas una carga. Entrégala en la grúa o pulsa Q para devolverla."); return; }
                Carga = o.valor; cargaOrigen = o; o.gameObject.SetActive(false); cargaVisible.gameObject.SetActive(true); cargaTexto.text = Carga.ToString(); Sonido(recogerSonido); Aviso("Carga " + Carga + " recogida. La grúa está junto al arco naranja del oeste."); break;
            case NumeriaObjeto.Tipo.Grua:
                if (Carga == 0) { Aviso("La grúa necesita 5 unidades. Busca los cristales 1, 2 y 3 entre los árboles."); return; }
                Energia += Carga; Carga = 0; cargaVisible.gameObject.SetActive(false); cargaOrigen = null;
                if (Energia == 5) Completar(o.transform.position);
                else if (Energia > 5)
                {
                    Energia = 0; foreach (var item in objetos) if (item.tipo == NumeriaObjeto.Tipo.Carga) item.gameObject.SetActive(true);
                    Aviso("Casi: nos hemos pasado de 5. Prueba con 2 y 3; las cargas han vuelto a sus pedestales.", 9); Sonido(errorSonido);
                }
                else { Aviso("Grúa: " + Energia + " / 5. Faltan " + (5 - Energia) + " unidades. Si elegiste 1, busca también 2 y 3 para reiniciar sin perder nada."); Sonido(recogerSonido); }
                o.rotulo.text = "GRÚA  " + Energia + " / 5"; break;
            case NumeriaObjeto.Tipo.Semilla:
                Semillas++; o.recogido = true; o.gameObject.SetActive(false); Sonido(recogerSonido); Efecto(o.transform.position); Aviso("Semilla guardada. Mochila: " + Semillas + ". Los jardines están al norte.", 3); break;
            case NumeriaObjeto.Tipo.Jardin:
                if (Semillas == 0) { Aviso("Busca semillas doradas en los tres refugios. Q recupera una semilla de este jardín."); return; }
                if (Jardines[o.valor] >= 6) return;
                Jardines[o.valor]++; Semillas--; ActualizarJardin(o); Sonido(recogerSonido); break;
            case NumeriaObjeto.Tipo.Distribuidor:
                if (Jardines[0] == 2 && Jardines[1] == 2 && Jardines[2] == 2) Completar(o.transform.position);
                else { Aviso("Vamos juntos: 6 semillas entre 3 jardines son 2 en cada uno. E planta y Q recupera; nada se pierde.", 10); Sonido(errorSonido); } break;
            case NumeriaObjeto.Tipo.Recuerdo:
                recuerdos++; o.recogido = true; o.gameObject.SetActive(false); Sonido(recogerSonido);
                Aviso(new[] { "Postal de Numeria: aquí los árboles guardan recuerdos, no contraseñas.", "Diario de Glitch: «Plan malvado nº 4: aprender a contar hasta el 4». ", "Byte encontró un mensaje: «Gracias por tender puentes»." }[o.valor], 8); break;
        }
    }
    private void ActualizarJardin(NumeriaObjeto o)
    {
        o.rotulo.text = "JARDÍN " + (o.valor + 1) + "  ·  " + Jardines[o.valor];
        for (int i = 0; i < 6; i++) o.transform.Find("Brote" + i).gameObject.SetActive(i < Jardines[o.valor]);
    }
    public void Pisar(NumeriaObjeto o)
    {
        if (Etapa != 1 || Cinematica || Distancia(o.transform) > 1.2f) return;
        if (o.valor == (Paso + 1) * 2)
        {
            Paso++; o.transform.Find("Luz").GetComponent<Renderer>().sharedMaterial = luzActiva; Sonido(recogerSonido); Efecto(o.transform.position);
            if (Paso == 4) Completar(o.transform.position); else Aviso("¡Bien! " + o.valor + ". Suma 2 y busca la siguiente baliza.", 4);
        }
        else
        {
            Paso = 0; foreach (var item in objetos) if (item.tipo == NumeriaObjeto.Tipo.Baliza) item.transform.Find("Luz").GetComponent<Renderer>().sharedMaterial = coloresIniciales[0];
            Aviso("Sin prisa. Empieza en 2 y suma 2 cada vez: 2 → 4 → 6 → 8.", 8); Sonido(errorSonido);
        }
    }
    private void DevolverCarga() { if (cargaOrigen != null) cargaOrigen.gameObject.SetActive(true); cargaOrigen = null; Carga = 0; cargaVisible.gameObject.SetActive(false); }
    public void Pista()
    {
        Aviso(Etapa == 0 ? "Oeste: recoge 2 y 3, de uno en uno, y llévalos a la grúa. 2 + 3 = 5." : Etapa == 1 ? "Este: camina por 2, 4, 6 y 8. No necesitas pulsar E." : Etapa == 2 ? "Recoge seis semillas doradas. Pon dos en cada jardín y activa la fuente central." : "Camina al norte y cruza los tres tramos hasta el faro.", 10);
    }
    private void Completar(Vector3 position)
    {
        int tramo = Etapa++;
        if (tramo == 2) foreach (var luz in lucesJardines) luz.sharedMaterial = luzActiva;
        Sonido(exitoSonido); Efecto(position); StartCoroutine(Reconstruir(tramo));
        Aviso(tramo == 0 ? "¡La grúa despierta! Un tramo reconstruido. Activa ahora la ruta de balizas del este." : tramo == 1 ? "¡La ruta lleva energía al puente! Recoge las semillas doradas y da vida a los tres jardines." : "¡Los jardines florecen! Glitch: «Solo quería un puente levadizo…». Cruza hacia el faro.", 10);
    }
    private IEnumerator Reconstruir(int index)
    {
        Cinematica = true; byteRobot.enabled = false; camara.Encuadre = focoPuente;
        Vector3 end = tramos[index].position; end.y = -.15f;
        Vector3 start = end + Vector3.down * 4;
        float elapsed = 0;
        while (elapsed < 2.4f)
        {
            elapsed += Time.deltaTime; float t = Mathf.SmoothStep(0, 1, elapsed / 1.7f);
            tramos[index].position = Vector3.Lerp(start, end, t); yield return null;
        }
        tramos[index].GetComponent<Collider>().enabled = true;
        camara.Encuadre = null; Cinematica = false; byteRobot.enabled = true;
    }
    private void Efecto(Vector3 pos) { if (chispas == null) return; chispas.transform.position = pos + Vector3.up; chispas.Emit(22); }
    private void Recolocar()
    {
        var cc = byteRobot.GetComponent<CharacterController>(); cc.enabled = false;
        byteRobot.transform.position = Etapa == 3 ? new Vector3(0, 1.1f, 18) : inicio;
        cc.enabled = true; byteRobot.enabled = false; byteRobot.enabled = true;
    }
    public void Reiniciar() { UnityEngine.SceneManagement.SceneManager.LoadScene(gameObject.scene.name); }
    private string Prompt()
    {
        if (cercano == null) return Etapa == 1 ? "Pisa las balizas · Tab: cuaderno / pista" : "Explora los senderos · Tab: cuaderno / pista";
        if (cercano.tipo == NumeriaObjeto.Tipo.Jardin) return "E · Plantar una semilla    Q · Recuperar una";
        if (cercano.tipo == NumeriaObjeto.Tipo.Carga) return "E · Recoger carga " + cercano.valor + "    Q · Devolver carga";
        return "E · " + cercano.nombre;
    }
    private void OnGUI()
    {
        float s = Mathf.Min(Screen.width / 1280f, Screen.height / 720f);
        GUI.matrix = Matrix4x4.TRS(new Vector3((Screen.width - 1280*s)/2, (Screen.height - 720*s)/2), Quaternion.identity, Vector3.one*s);
        var small = new GUIStyle(GUI.skin.label) { fontSize = 19, wordWrap = true };
        var body = new GUIStyle(small) { fontSize = 21 };
        var title = new GUIStyle(small) { fontSize = 36, fontStyle = FontStyle.Bold };
        var button = new GUIStyle(GUI.skin.button) { fontSize = 23 };
        GUI.backgroundColor = new Color(.055f, .12f, .2f, .96f);
        if (Intro || Finalizado || Pausa)
        {
            GUI.Box(new Rect(230, 145, 820, 435), "");
            GUI.Label(new Rect(275, 180, 740, 60), Intro ? "NUMERIA · EL PUENTE DORMIDO" : Finalizado ? "Un pequeño robot. Un gran puente." : "Un respiro", title);
            string text = Intro ? "Glitch ha apagado el puente del faro.\n\nRecupera su energía, encuentra una ruta y haz florecer los jardines. Cada reparación reconstruye una parte del camino.\n\nWASD / flechas · Mover     E · Usar     Q · Devolver\nShift · Correr     Tab · Cuaderno     M · Sonido" : Finalizado ? "Byte ha unido las dos orillas.\n\nGlitch: «¡Era una prueba de mantenimiento!\n¿Nadie ha visto mi manual?»\n\nPostales: " + recuerdos + " / 3   ·   Tiempo: " + Mathf.FloorToInt(tiempo / 60) + ":" + ((int)tiempo % 60).ToString("00") : "El puente puede esperar.\n\nE interactúa con el objeto cercano. Q permite corregir.\nNo hay vidas ni penalización por equivocarse.";
            GUI.Label(new Rect(275, 260, 730, 210), text, body);
            if (GUI.Button(new Rect(380, 500, 520, 55), Intro ? "Ayudar a Byte · Enter" : Finalizado ? "Volver a jugar" : "Continuar · Esc", button))
            { if (Intro) Empezar(); else if (Finalizado) Reiniciar(); else { Pausa = false; byteRobot.enabled = true; } }
            return;
        }
        GUI.Box(new Rect(25, 20, 970, 85), "");
        GUI.Label(new Rect(45, 30, 930, 30), "NUMERIA / EL PUENTE DORMIDO                     Puente " + Etapa + "/3", body);
        GUI.Label(new Rect(45, 65, 930, 34), Objetivo, small);
        GUI.Box(new Rect(1010, 20, 245, 85), "");
        GUI.Label(new Rect(1025, 35, 220, 65), Etapa == 0 ? "Carga: " + Carga + "   Grúa: " + Energia + "/5" : Etapa == 1 ? "Ruta: " + Paso + "/4" : "Semillas: " + Semillas + "   Postales: " + recuerdos, small);
        if (Time.unscaledTime < mensajeHasta)
        { GUI.Box(new Rect(160, 540, 960, 82), ""); GUI.Label(new Rect(185, 554, 915, 62), Mensaje, small); }
        GUI.Box(new Rect(280, 640, 720, 48), ""); GUI.Label(new Rect(300, 650, 680, 35), Cinematica ? "Reconstruyendo el puente…" : Prompt(), small);
        if (cuaderno)
        {
            GUI.Box(new Rect(30, 130, 490, 365), "");
            GUI.Label(new Rect(55, 150, 440, 50), "CUADERNO DE BYTE", body);
            GUI.Label(new Rect(55, 205, 440, 205), "OESTE · Grúa y cargas entre los árboles.\nESTE · Balizas que crecen de dos en dos.\nNORTE · Tres jardines y el puente del faro.\n\nLas semillas están en los tres refugios dorados. Explora también para encontrar 3 postales.\n\nTab cierra el cuaderno. M activa/desactiva sonido.", small);
            if (GUI.Button(new Rect(80, 425, 390, 45), "Una pista, por favor", button)) Pista();
        }
    }
}

