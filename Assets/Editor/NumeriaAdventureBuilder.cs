using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ByteAdventure.Editor
{
    public static class NumeriaAdventureBuilder
    {
        const string ScenePath = "Assets/Scenes/SampleScene.unity";
        static Transform root;
        static Material ground, stone, cyan, gold, pink, grass, bark;
        static List<NumeriaObjeto> nodes;
        static GameObject Shape(string name, PrimitiveType type, Vector3 p, Vector3 scale, Material mat, bool solid = false, Transform parent = null)
        {
            var go = NumeriaBuilder.Part(parent == null ? root : parent, name, type, p, scale, mat, solid);
            if (solid && type == PrimitiveType.Cylinder)
            {
                UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
                go.AddComponent<MeshCollider>().sharedMesh = go.GetComponent<MeshFilter>().sharedMesh;
            }
            return go;
        }
        static TextMesh Text(string text, Vector3 pos, float size = .085f, Transform parent = null)
        {
            var go = new GameObject("Rótulo " + text); go.transform.SetParent(parent == null ? root : parent, false); go.transform.localPosition = pos;
            var mesh = go.AddComponent<TextMesh>(); mesh.text = text; mesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            mesh.fontSize = 48; mesh.characterSize = size; mesh.anchor = TextAnchor.MiddleCenter; mesh.alignment = TextAlignment.Center;
            mesh.color = new Color(.87f, .97f, 1); go.GetComponent<Renderer>().sharedMaterial = mesh.font.material; return mesh;
        }
        static NumeriaObjeto Node(NumeriaObjeto.Tipo tipo, int value, string label, Vector3 pos, Material mat)
        {
            var go = new GameObject(label); go.transform.SetParent(root); go.transform.position = pos;
            var node = go.AddComponent<NumeriaObjeto>(); node.tipo = tipo; node.valor = value; node.nombre = label;
            Shape("Base", PrimitiveType.Cylinder, Vector3.zero, new Vector3(1.7f, .15f, 1.7f), stone, false, go.transform);
            Shape("Luz", PrimitiveType.Cylinder, new Vector3(0,.18f,0), new Vector3(1.5f,.025f,1.5f), mat, false, go.transform);
            node.rotulo = Text(label, new Vector3(0, 1.85f, 0), .07f, go.transform); nodes.Add(node); return node;
        }
        static void Path(Vector3 from, Vector3 to)
        {
            int count = Mathf.CeilToInt(Vector3.Distance(from, to) / 1.4f);
            for (int i = 0; i <= count; i++)
            {
                Vector3 p = Vector3.Lerp(from, to, i/(float)count); p.y = .055f;
                Shape("Paso del sendero", PrimitiveType.Cylinder, p, new Vector3(.72f,.035f,.72f), stone);
            }
        }
        static void Tree(Vector3 p, float scale)
        {
            Shape("Tronco de memoria", PrimitiveType.Cylinder, p + Vector3.up * scale, new Vector3(.35f*scale,scale,.35f*scale), bark);
            Shape("Copa de datos", PrimitiveType.Sphere, p + Vector3.up * 2.3f*scale, new Vector3(2,1.6f,2)*scale, grass);
            Shape("Fruto luminoso", PrimitiveType.Sphere, p + new Vector3(.65f,2.2f,-.55f)*scale, Vector3.one*.25f, cyan);
        }
        [MenuItem("Tools/ByteAdventure/Crear aventura del puente")]
        public static void Build()
        {
            if (Application.isPlaying) throw new Exception("Sal de Play antes de construir.");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != ScenePath) throw new Exception("Abre SampleScene primero.");
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Numeria_Anterior.unity") == null)
                AssetDatabase.CopyAsset(ScenePath, "Assets/Scenes/Numeria_Anterior.unity");
            var player = UnityEngine.Object.FindFirstObjectByType<ByteController>();
            if (player == null) throw new Exception("Falta Byte.");
            foreach (string name in new[] { "Numeria_Demo", "Numeria_Aventura", "Plane", "ZonaReto", "PuertaMatematica" })
            {
                var old = GameObject.Find(name); if (old != null) { if (name.StartsWith("Numeria_")) UnityEngine.Object.DestroyImmediate(old); else old.SetActive(false); }
            }
            var world = new GameObject("Numeria_Aventura"); root = world.transform; nodes = new List<NumeriaObjeto>();
            var game = world.AddComponent<NumeriaPuente>(); game.byteRobot = player; player.controlesAventura = true;
            player.transform.SetPositionAndRotation(new Vector3(0,1.05f,-16), Quaternion.Euler(0,180,0));
            ByteAppearanceBuilder.Build(player.gameObject);
            ground = NumeriaBuilder.Material("A_Isla", new Color(.085f,.2f,.25f));
            stone = NumeriaBuilder.Material("A_Sendero", new Color(.23f,.38f,.43f));
            cyan = NumeriaBuilder.Material("A_Luz", new Color(.12f,.8f,.87f), true);
            gold = NumeriaBuilder.Material("A_Semilla", new Color(1,.62f,.15f), true);
            pink = NumeriaBuilder.Material("A_Glitch", new Color(.6f,.22f,.75f));
            grass = NumeriaBuilder.Material("A_Copa", new Color(.13f,.48f,.4f));
            bark = NumeriaBuilder.Material("A_Tronco", new Color(.12f,.22f,.28f));
            var sea = NumeriaBuilder.Material("A_Vacio", new Color(.025f,.06f,.13f));
            Shape("Océano digital", PrimitiveType.Plane, new Vector3(0,-6,15), new Vector3(30,1,30), sea);
            Shape("Orilla biselada", PrimitiveType.Cylinder, new Vector3(0,-.8f,0), new Vector3(45,.6f,45), stone);
            Shape("Isla jardín", PrimitiveType.Cylinder, new Vector3(0,-.45f,0), new Vector3(44,.45f,44), ground, true);
            Shape("Isla del faro", PrimitiveType.Cylinder, new Vector3(0,-.45f,44), new Vector3(16,.45f,16), ground, true);
            Shape("Cimiento faro", PrimitiveType.Cylinder, new Vector3(0,.1f,46), new Vector3(5,.15f,5), stone, true);
            Shape("Torre faro", PrimitiveType.Cylinder, new Vector3(0,3,47), new Vector3(1.8f,3,1.8f), stone, true);
            Shape("Lente faro", PrimitiveType.Sphere, new Vector3(0,6.2f,47), new Vector3(2,1.5f,2), gold);
            Text("FARO DE NUMERIA", new Vector3(0,8,47), .11f);
            var target = new GameObject("Llegada al faro"); target.transform.SetParent(root); target.transform.position = new Vector3(0,1,43); game.meta = target.transform;
            var focus = new GameObject("Vista puente"); focus.transform.SetParent(root); focus.transform.position = new Vector3(0,0,29); game.focoPuente = focus.transform;
            game.tramos = new Transform[3];
            for (int i = 0; i < 3; i++)
            {
                var deck = Shape("Tramo reconstruible " + i, PrimitiveType.Cube, new Vector3(0,-4.15f,24.5f+5*i), new Vector3(4.8f,.3f,5), stone, true);
                deck.GetComponent<Collider>().enabled = false; game.tramos[i] = deck.transform;
                for (int sign = -1; sign <= 1; sign += 2)
                {
                    Shape("Pasamanos luminoso", PrimitiveType.Cube, new Vector3(sign*.47f,3,0), new Vector3(.022f,.2f,.95f), cyan, false, deck.transform);
                    for (int j = -1; j <= 1; j++) Shape("Soporte baranda", PrimitiveType.Cube, new Vector3(sign*.47f,1.5f,j*.42f), new Vector3(.03f,3,.025f), bark, false, deck.transform);
                }
            }
            // The crane, stepping route and gardens are destinations, not successive walls.
            Path(new Vector3(0,0,-16), new Vector3(-11,0,-7)); Path(new Vector3(-11,0,-7), new Vector3(-11,0,7));
            Path(new Vector3(0,0,-16), new Vector3(9,0,-7)); Path(new Vector3(9,0,-7), new Vector3(16,0,0));
            Path(new Vector3(16,0,0), new Vector3(9,0,6)); Path(new Vector3(9,0,6), new Vector3(16,0,11));
            Path(new Vector3(-11,0,7), new Vector3(0,0,14)); Path(new Vector3(16,0,11), new Vector3(0,0,14)); Path(new Vector3(0,0,14), new Vector3(0,0,21));
            var crane = Node(NumeriaObjeto.Tipo.Grua, 0, "GRÚA  0 / 5", new Vector3(-11,.2f,7), gold);
            Shape("Columna grúa", PrimitiveType.Cylinder, new Vector3(-14,2.6f,8), new Vector3(.5f,2.6f,.5f), gold);
            Shape("Brazo grúa", PrimitiveType.Cube, new Vector3(-11,5.1f,8), new Vector3(6.5f,.3f,.35f), gold);
            Shape("Cable grúa", PrimitiveType.Cylinder, new Vector3(-8,3.7f,8), new Vector3(.06f,1.3f,.06f), bark);
            Vector3[] crystals = { new Vector3(-6,.2f,-11), new Vector3(-16,.2f,-9), new Vector3(-18,.2f,5) };
            for (int i = 0; i < 3; i++)
            {
                var n = Node(NumeriaObjeto.Tipo.Carga, i+1, "CARGA " + (i+1), crystals[i], cyan);
                var crystal = Shape("Cristal", PrimitiveType.Cube, new Vector3(0,.8f,0), Vector3.one*.65f, cyan, false, n.transform); crystal.transform.localRotation = Quaternion.Euler(45,0,45);
            }
            Vector3[] pads = { new Vector3(9,.06f,-7), new Vector3(16,.06f,0), new Vector3(9,.06f,6), new Vector3(16,.06f,11) };
            for (int i = 0; i < 4; i++) Node(NumeriaObjeto.Tipo.Baliza, (i+1)*2, ((i+1)*2).ToString(), pads[i], stone);
            game.lucesJardines = new Renderer[3];
            for (int i = 0; i < 3; i++)
            {
                var garden = Node(NumeriaObjeto.Tipo.Jardin, i, "JARDÍN " + (i+1) + "  ·  0", new Vector3(-5+i*5,.2f,16), stone);
                game.lucesJardines[i] = garden.transform.Find("Luz").GetComponent<Renderer>();
                for (int j = 0; j < 6; j++)
                {
                    float a = j*Mathf.PI/3;
                    var sprout = Shape("Brote"+j, PrimitiveType.Sphere, new Vector3(Mathf.Cos(a)*.6f,.6f,Mathf.Sin(a)*.6f), new Vector3(.45f,.85f,.45f), grass, false, garden.transform); sprout.SetActive(false);
                }
            }
            Node(NumeriaObjeto.Tipo.Distribuidor, 0, "ACTIVAR JARDINES", new Vector3(0,.2f,12), gold);
            Vector3[] refuges = { new Vector3(-16,0,10), new Vector3(15,0,-10), new Vector3(3,0,4) };
            for (int i = 0; i < refuges.Length; i++)
            {
                Shape("Refugio de semillas", PrimitiveType.Cylinder, refuges[i]+new Vector3(0,.05f,0), new Vector3(3.5f,.08f,3.5f), stone);
                for (int j = 0; j < 2; j++)
                {
                    var seed = Node(NumeriaObjeto.Tipo.Semilla, 1, "SEMILLA", refuges[i]+new Vector3(j*1.7f-.85f,.2f,0), gold);
                    Shape("Semilla", PrimitiveType.Sphere, new Vector3(0,.7f,0), new Vector3(.45f,.65f,.45f), gold, false, seed.transform);
                }
            }
            Vector3[] postcards = { new Vector3(-17,.2f,-1), new Vector3(18,.2f,5), new Vector3(-7,.2f,18) };
            for (int i = 0; i < 3; i++) Node(NumeriaObjeto.Tipo.Recuerdo, i, "POSTAL", postcards[i], pink);
            Vector3[] trees = { new Vector3(-19,0,-5),new Vector3(-13,0,-13),new Vector3(-19,0,8),new Vector3(-10,0,15),new Vector3(19,0,-4),new Vector3(12,0,-14),new Vector3(19,0,8),new Vector3(7,0,17) };
            foreach (var p in trees) Tree(p, .8f + (Mathf.Abs(p.x)%3)*.12f);
            var glitch = new GameObject("Glitch"); glitch.transform.SetParent(root); glitch.transform.position = new Vector3(-3,3,-8); game.glitch = glitch.transform;
            Shape("Cabeza traviesa", PrimitiveType.Sphere, Vector3.zero, new Vector3(1.7f,1.25f,1.1f), pink, false, glitch.transform);
            for (int sign = -1; sign <= 1; sign += 2)
            {
                Shape("Ojo", PrimitiveType.Sphere, new Vector3(sign*.32f,.08f,-.52f), new Vector3(.17f,.22f,.08f), gold, false, glitch.transform);
                Shape("Antena torcida", PrimitiveType.Capsule, new Vector3(sign*.5f,.7f,0), new Vector3(.15f,.3f,.15f), pink, false, glitch.transform);
            }
            Text("GLITCH", new Vector3(-3,4.1f,-8), .065f);
            var carry = Shape("Carga transportada", PrimitiveType.Sphere, Vector3.up*3, Vector3.one*.6f, cyan); game.cargaVisible = carry.transform;
            game.cargaTexto = Text("", new Vector3(0,1,0), .15f, carry.transform); carry.SetActive(false);
            var fx = new GameObject("Chispas de reparación"); fx.transform.SetParent(root); var ps = fx.AddComponent<ParticleSystem>();
            var main = ps.main; main.playOnAwake = false; main.loop = false; main.startLifetime = .8f; main.startSpeed = 2; main.startSize = .12f; main.startColor = new Color(.2f,1,1); main.gravityModifier = .35f;
            var em = ps.emission; em.enabled = false; ps.GetComponent<ParticleSystemRenderer>().sharedMaterial = cyan; game.chispas = ps;
            game.recogerSonido = Sound("Recoger", new[] { 660f, 880f }, .08f);
            game.errorSonido = Sound("Pista", new[] { 392f, 330f }, .16f);
            game.exitoSonido = Sound("Puente", new[] { 523f, 659f, 784f, 1046f }, .16f);
            game.objetos = nodes.ToArray(); game.luzActiva = cyan;
            var camera = Camera.main; var follow = camera.GetComponent<CamaraSeguimiento>(); follow.vistaAventura = true; game.camara = follow;
            camera.fieldOfView = 48; camera.nearClipPlane = .15f; camera.farClipPlane = 180;
            camera.backgroundColor = new Color(.025f,.045f,.09f); camera.clearFlags = CameraClearFlags.SolidColor;
            camera.transform.position = player.transform.position + new Vector3(0,10,-9.8f); camera.transform.LookAt(player.transform.position+new Vector3(0,.2f,2.2f));
            RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(.55f,.65f,.75f);
            RenderSettings.fog = true; RenderSettings.fogColor = camera.backgroundColor; RenderSettings.fogMode = FogMode.Linear; RenderSettings.fogStartDistance = 55; RenderSettings.fogEndDistance = 140;
            foreach (var light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None)) if (light.type == LightType.Directional) { light.intensity = 1.6f; light.transform.rotation = Quaternion.Euler(48,-35,0); }
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath,true),new EditorBuildSettingsScene("Assets/Scenes/Numeria_Anterior.unity",false) };
            AssetDatabase.SaveAssets(); EditorSceneManager.SaveScene(scene); Debug.Log("BRIDGE_BUILD_OK");
        }
        static AudioClip Sound(string name, float[] notes, float duration)
        {
            const string folder = "Assets/ByteAdventure/Audio"; Directory.CreateDirectory(folder); string path = folder+"/"+name+".wav";
            int rate = 22050, count = Mathf.RoundToInt(rate*duration*notes.Length);
            using (var writer = new BinaryWriter(File.Create(path)))
            {
                writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36+count*2); writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16); writer.Write((short)1); writer.Write((short)1); writer.Write(rate); writer.Write(rate*2); writer.Write((short)2); writer.Write((short)16); writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(count*2);
                for (int i = 0; i < count; i++) { float t = i/(float)rate; int n = Mathf.Min(notes.Length-1,(int)(t/duration)); float envelope = Mathf.Sin(Mathf.PI*(t%duration)/duration); writer.Write((short)(Mathf.Sin(2*Mathf.PI*notes[n]*t)*envelope*11000)); }
            }
            AssetDatabase.ImportAsset(path); return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        }
        public static void BatchBuild() { EditorSceneManager.OpenScene(ScenePath); Build(); Validate(); }
        [MenuItem("Tools/ByteAdventure/Abrir escena anterior")]
        public static void OpenPrevious() { if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) EditorSceneManager.OpenScene("Assets/Scenes/Numeria_Anterior.unity"); }
        [MenuItem("Tools/ByteAdventure/Validar aventura del puente")]
        public static void Validate()
        {
            var game = UnityEngine.Object.FindFirstObjectByType<NumeriaPuente>();
            if (game == null || game.byteRobot == null || game.camara == null || game.tramos.Length != 3 || game.objetos.Length != 21 || game.recogerSonido == null) throw new Exception("Referencias incompletas.");
            if (GraphicsSettings.currentRenderPipeline == null) throw new Exception("URP no activo.");
            foreach (var rootObject in game.gameObject.scene.GetRootGameObjects()) foreach (var t in rootObject.GetComponentsInChildren<Transform>(true)) if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) > 0) throw new Exception("Script perdido en " + t.name);
            Debug.Log("BRIDGE_VALIDATE_OK: referencias, URP, audio, 21 interacciones y scripts.");
        }
    }
}
