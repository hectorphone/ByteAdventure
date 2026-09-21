using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

namespace ByteAdventure.Editor
{
    public static class NumeriaBuilder
    {
        public const string ScenePath = "Assets/Scenes/SampleScene.unity";
        public static Material Material(string name, Color color, bool emission = false)
        {
            const string folder = "Assets/ByteAdventure/Materials";
            Directory.CreateDirectory(folder);
            string path = folder + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) throw new InvalidOperationException("URP/Lit no está disponible.");
                material = new Material(shader); AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", .45f);
            material.SetColor("_EmissionColor", emission ? color * 1.5f : Color.black);
            if (emission) material.EnableKeyword("_EMISSION"); else material.DisableKeyword("_EMISSION");
            EditorUtility.SetDirty(material); return material;
        }
        public static GameObject Part(Transform parent, string name, PrimitiveType shape, Vector3 pos, Vector3 scale, Material material, bool collision = true)
        {
            var go = GameObject.CreatePrimitive(shape); go.name = name; go.transform.SetParent(parent, false);
            go.transform.localPosition = pos; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!collision) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }
        private static void Label(Transform parent, string text, Vector3 pos, int size = 45)
        {
            var go = new GameObject("Rótulo " + text); go.transform.SetParent(parent, false); go.transform.localPosition = pos;
            go.transform.localRotation = Quaternion.identity;
            var label = go.AddComponent<TextMesh>(); label.text = text; label.fontSize = size; label.characterSize = .07f;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); go.GetComponent<Renderer>().sharedMaterial = label.font.material;
            label.anchor = TextAnchor.MiddleCenter; label.alignment = TextAlignment.Center; label.color = new Color(.7f, 1, 1);
        }
        [MenuItem("Tools/ByteAdventure/Preparar demo Numeria")]
        public static void Build()
        {
            if (Application.isPlaying) throw new InvalidOperationException("Sal de Play antes de preparar la demo.");
            var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (scene.path != ScenePath) throw new InvalidOperationException("Abre Assets/Scenes/SampleScene.unity antes de preparar la demo.");
            var player = UnityEngine.Object.FindFirstObjectByType<ByteController>();
            if (player == null) throw new InvalidOperationException("Falta ByteController en la escena existente.");
            var existing = GameObject.Find("Numeria_Demo");
            if (existing != null) Undo.DestroyObjectImmediate(existing);
            foreach (string old in new[] { "ZonaReto", "PuertaMatematica" })
            {
                var go = GameObject.Find(old); if (go != null) { Undo.RecordObject(go, "Sustituir prototipo"); go.SetActive(false); }
            }
            ByteAppearanceBuilder.Build(player.gameObject);
            player.transform.position = new Vector3(0, 1.05f, 0); player.transform.rotation = Quaternion.Euler(0, 180, 0);
            var root = new GameObject("Numeria_Demo"); Undo.RegisterCreatedObjectUndo(root, "Preparar Numeria");
            var demo = root.AddComponent<NumeriaDemo>(); demo.jugador = player;
            var floor = Material("Suelo", new Color(.035f, .07f, .16f));
            var edge = Material("Bordes", new Color(.08f, .17f, .29f));
            var cyan = Material("Cian", new Color(.15f, .95f, 1), true);
            var purple = Material("Glitch", new Color(.7f, .16f, .85f), true);
            var gold = Material("Portal", new Color(1, .65f, .12f), true);
            var baseFloor = GameObject.Find("Plane");
            if (baseFloor != null) { baseFloor.transform.position = new Vector3(0, -.12f, 12); baseFloor.transform.localScale = new Vector3(1.6f, 1, 3.6f); baseFloor.GetComponent<Renderer>().sharedMaterial = floor; }
            Part(root.transform, "Isla digital", PrimitiveType.Cube, new Vector3(0, -.35f, 12), new Vector3(16, .7f, 36), floor);
            for (int i = -1; i <= 1; i += 2)
            {
                Part(root.transform, "Límite lateral", PrimitiveType.Cube, new Vector3(i * 7.8f, 1, 12), new Vector3(.35f, 2, 36), edge);
                Part(root.transform, "Guía luminosa", PrimitiveType.Cube, new Vector3(i * 2, .025f, 12), new Vector3(.08f, .03f, 35), cyan, false);
            }
            Part(root.transform, "Límite inicio", PrimitiveType.Cube, new Vector3(0, 1, -5.8f), new Vector3(16, 2, .35f), edge);
            Part(root.transform, "Límite final", PrimitiveType.Cube, new Vector3(0, 1, 29.8f), new Vector3(16, 2, .35f), edge);
            for (int z = -4; z <= 28; z += 2)
                Part(root.transform, "Circuito suelo", PrimitiveType.Cube, new Vector3(0, .015f, z), new Vector3(15.3f, .02f, .025f), edge, false);
            demo.retos = new RetoMatematico[3];
            string[] questions = { "SUMA · Hay 2 bits azules y llegan 3 más. ¿Cuántos hay?", "RESTA · Glitch tenía 9 errores. Byte corrige 4. ¿Cuántos quedan?", "GRUPOS · Hay 3 cajas con 2 bits cada una. ¿Cuántos bits hay?" };
            string[] hints = { "Empieza en 2 y añade tres: 3, 4, 5.", "Quita cuatro a nueve: 8, 7, 6, 5.", "Suma los tres grupos: 2 + 2 + 2." };
            for (int i = 0; i < 3; i++)
            {
                float z = 4 + i * 8;
                var barrier = Part(root.transform, "Puerta " + (i + 1), PrimitiveType.Cube, new Vector3(0, 1.4f, z + 3), new Vector3(15.3f, 2.8f, .3f), purple);
                var terminal = Part(root.transform, "Terminal " + (i + 1), PrimitiveType.Cylinder, new Vector3(0, .65f, z), new Vector3(1.1f, .65f, 1.1f), edge);
                Part(terminal.transform, "Luz", PrimitiveType.Sphere, new Vector3(0, 1.2f, 0), new Vector3(.5f, .35f, .5f), cyan, false);
                Label(root.transform, "0" + (i + 1) + " / " + new[] { "SUMA", "RESTA", "GRUPOS" }[i], new Vector3(0, 2.3f, z));
                var reto = terminal.AddComponent<RetoMatematico>(); reto.indice = i; reto.pregunta = questions[i]; reto.pista = hints[i];
                reto.opciones = i == 0 ? new[] { 4, 5, 6 } : i == 1 ? new[] { 5, 6, 4 } : new[] { 5, 7, 6 };
                reto.respuesta = i == 2 ? 6 : 5; reto.Configurar(barrier); demo.retos[i] = reto;
                for (int s = -1; s <= 1; s += 2)
                {
                    Part(root.transform, "Torre de datos", PrimitiveType.Cube, new Vector3(s * 5.7f, 1, z - 1), new Vector3(1, 2, 1), edge);
                    Part(root.transform, "Bit de datos", PrimitiveType.Cube, new Vector3(s * 5.7f, 2.4f, z - 1), Vector3.one * .55f, cyan, false);
                }
            }
            var portal = new GameObject("Portal final"); portal.transform.SetParent(root.transform); portal.transform.position = new Vector3(0, 1, 27); demo.portal = portal.transform;
            foreach (int s in new[] { -1, 1 }) Part(portal.transform, "Pilar", PrimitiveType.Cube, new Vector3(s * 1.5f, .7f, 0), new Vector3(.3f, 3.4f, .4f), gold);
            Part(portal.transform, "Dintel", PrimitiveType.Cube, new Vector3(0, 2.4f, 0), new Vector3(3.3f, .3f, .4f), gold);
            Label(root.transform, "NÚCLEO DE NUMERIA", new Vector3(0, 4.2f, 27));
            var glitch = new GameObject("Glitch"); glitch.transform.SetParent(root.transform); glitch.transform.position = new Vector3(-3, 2.4f, 2); demo.glitch = glitch.transform;
            Part(glitch.transform, "Cabeza", PrimitiveType.Cube, Vector3.zero, new Vector3(1.3f, 1, .8f), purple, false);
            var dark = Material("PantallaOscura", new Color(.015f, .035f, .09f));
            for (int s = -1; s <= 1; s += 2) Part(glitch.transform, "Ojo", PrimitiveType.Cube, new Vector3(s * .3f, .15f, -.43f), new Vector3(.16f, .22f, .1f), dark, false);
            Part(glitch.transform, "Bigote absurdo", PrimitiveType.Cube, new Vector3(0, -.17f, -.45f), new Vector3(.7f, .08f, .1f), dark, false);
            Label(root.transform, "GLITCH\n«¡Todo bajo descontrol!»", new Vector3(-3, 3.8f, 2), 32);
            var cam = Camera.main;
            if (cam != null)
            {
                cam.backgroundColor = new Color(.015f, .025f, .07f); cam.clearFlags = CameraClearFlags.SolidColor;
                var follow = cam.GetComponent<CamaraSeguimiento>();
                if (follow != null) { var so = new SerializedObject(follow); so.FindProperty("objetivo").objectReferenceValue = player.transform; so.ApplyModifiedPropertiesWithoutUndo(); }
            }
            RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(.5f, .6f, .85f);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.SaveAssets(); EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
            Debug.Log("NUMERIA_BUILD_OK: escena y referencias guardadas.");
        }
        public static void BatchBuild()
        {
            EditorSceneManager.OpenScene(ScenePath); Build(); Validate();
        }
        [MenuItem("Tools/ByteAdventure/Validar demo")]
        public static void Validate()
        {
            var demo = UnityEngine.Object.FindFirstObjectByType<NumeriaDemo>();
            if (demo == null || demo.jugador == null || demo.portal == null || demo.retos.Length != 3) throw new Exception("Demo incompleta.");
            if (GraphicsSettings.currentRenderPipeline == null) throw new Exception("URP no está activo.");
            foreach (var r in demo.retos)
            {
                if (r == null || r.opciones.Length != 3 || !r.opciones.Contains(r.respuesta)) throw new Exception("Reto mal configurado.");
                if (r.Responder(-999) || r.Completado) throw new Exception("Respuesta incorrecta aceptada.");
                if (!r.Responder(r.respuesta) || !r.Completado) throw new Exception("Respuesta correcta rechazada.");
                r.Reiniciar(); if (r.Completado) throw new Exception("Reinicio incorrecto.");
            }
            int parts = demo.jugador.GetComponentsInChildren<Renderer>().Length;
            ByteAppearanceBuilder.Build(demo.jugador.gameObject);
            if (parts != demo.jugador.GetComponentsInChildren<Renderer>().Length) throw new Exception("Piezas duplicadas al reconstruir Byte.");
            EditorSceneManager.SaveScene(demo.gameObject.scene);
            Debug.Log("NUMERIA_VALIDATE_OK: URP, referencias, tres retos, respuestas, reinicio y aspecto idempotente.");
        }
    }
}



