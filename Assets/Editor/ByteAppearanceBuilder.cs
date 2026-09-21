using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ByteAdventure.Editor
{
    public static class ByteAppearanceBuilder
    {
        [MenuItem("Tools/ByteAdventure/Completar aspecto provisional de Byte")]
        public static void BuildAppearance()
        {
            var byteObject = UnityEngine.Object.FindFirstObjectByType<ByteController>();
            if (byteObject == null) throw new InvalidOperationException("La escena necesita un ByteController.");
            Build(byteObject.gameObject);
            EditorSceneManager.MarkSceneDirty(byteObject.gameObject.scene);
        }
        public static void ValidateCompile() { Debug.Log("ByteAdventure: runtime y editor compilados."); }
        public static void Build(GameObject target)
        {
            string[] names = { "ByteVisual", "Cuerpo", "Body", "Torso", "Cabeza", "Head", "Pantalla", "Screen", "Cara", "EyeLeft", "EyeRight", "LeftEye", "RightEye", "OjoIzquierdo", "OjoDerecho", "ArmLeft", "ArmRight", "LeftArm", "RightArm", "BrazoIzquierdo", "BrazoDerecho", "LegLeft", "LegRight", "LeftLeg", "RightLeg", "PiernaIzquierda", "PiernaDerecha" };
            foreach (Transform child in target.transform.Cast<Transform>().ToArray())
                if (names.Contains(child.name)) Undo.DestroyObjectImmediate(child.gameObject);
            // The old root capsule is visual only; retain its CharacterController and scripts.
            var renderer = target.GetComponent<Renderer>();
            if (renderer != null) { Undo.RecordObject(renderer, "Ocultar cápsula provisional"); renderer.enabled = false; }
            var root = new GameObject("ByteVisual"); Undo.RegisterCreatedObjectUndo(root, "Crear Byte"); root.transform.SetParent(target.transform, false);
            var blue = NumeriaBuilder.Material("ByteAzul", new Color(.16f, .36f, .95f));
            var pale = NumeriaBuilder.Material("ByteClaro", new Color(.35f, .6f, 1));
            var dark = NumeriaBuilder.Material("PantallaOscura", new Color(.015f, .035f, .09f));
            var cyan = NumeriaBuilder.Material("Cian", new Color(.15f, .95f, 1), true);
            NumeriaBuilder.Part(root.transform, "Cuerpo", PrimitiveType.Sphere, new Vector3(0, -.2f, 0), new Vector3(.85f, .85f, .65f), blue, false);
            NumeriaBuilder.Part(root.transform, "Cabeza", PrimitiveType.Sphere, new Vector3(0, .7f, 0), new Vector3(1.55f, 1.4f, 1.05f), pale, false);
            NumeriaBuilder.Part(root.transform, "Marco", PrimitiveType.Cube, new Vector3(0, .7f, .45f), new Vector3(1.22f, .84f, .19f), blue, false);
            NumeriaBuilder.Part(root.transform, "Pantalla", PrimitiveType.Cube, new Vector3(0, .7f, .555f), new Vector3(1.06f, .68f, .045f), dark, false);
            for (int sign = -1; sign <= 1; sign += 2)
            {
                NumeriaBuilder.Part(root.transform, "Brazo" + sign, PrimitiveType.Capsule, new Vector3(sign * .58f, -.27f, 0), new Vector3(.27f, .25f, .29f), blue, false);
                NumeriaBuilder.Part(root.transform, "Pie" + sign, PrimitiveType.Sphere, new Vector3(sign * .25f, -.79f, .1f), new Vector3(.35f, .4f, .48f), blue, false);
                NumeriaBuilder.Part(root.transform, "Ojo" + sign, PrimitiveType.Cube, new Vector3(sign * .25f, .78f, .59f), new Vector3(.12f, .17f, .025f), cyan, false);
            }
            NumeriaBuilder.Part(root.transform, "Sonrisa", PrimitiveType.Cube, new Vector3(0, .53f, .59f), new Vector3(.3f, .045f, .025f), cyan, false);
            NumeriaBuilder.Part(root.transform, "Nucleo", PrimitiveType.Sphere, new Vector3(0, -.16f, .32f), new Vector3(.22f, .22f, .055f), cyan, false);
        }
    }
}
