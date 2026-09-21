using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace ByteAdventure.Editor
{
    [InitializeOnLoad]
    public static class NumeriaSmokeTest
    {
        private const string Key = "Numeria.Smoke";
        private static int stage;
        private static double next;
        private static Vector3 before;
        static NumeriaSmokeTest() { EditorApplication.update += Tick; }
        public static void Run()
        {
            NumeriaBuilder.BatchBuild();
            SessionState.SetBool(Key, true); SessionState.SetString(Key + ".Start", DateTime.UtcNow.ToString("O"));
            EditorApplication.isPlaying = true;
        }
        private static object Read(NumeriaDemo d, string name) { return typeof(NumeriaDemo).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(d); }
        private static void Call(NumeriaDemo d, string name, params object[] args) { typeof(NumeriaDemo).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(d, args); }
        private static void Require(bool value, string message) { if (!value) throw new Exception(message); }
        private static void Teleport(NumeriaDemo d, Vector3 position)
        {
            var cc = d.jugador.GetComponent<CharacterController>(); cc.enabled = false; d.jugador.transform.position = position; cc.enabled = true;
        }
        private static void Tick()
        {
            if (SessionState.GetBool(Key, false) && DateTime.TryParse(SessionState.GetString(Key + ".Start", ""), out var started) && (DateTime.UtcNow - started.ToUniversalTime()).TotalSeconds > 120) { SessionState.SetBool(Key, false); Debug.LogError("NUMERIA_PLAY_TIMEOUT"); EditorApplication.Exit(1); return; }
            if (!SessionState.GetBool(Key, false) || !EditorApplication.isPlaying || EditorApplication.isCompiling || Time.timeSinceLevelLoad < .5f || Time.time < next) return;
            next = Time.time + .5f;
            try
            {
                var d = UnityEngine.Object.FindFirstObjectByType<NumeriaDemo>(); Require(d != null, "No hay demo en Play.");
                if (Keyboard.current == null) InputSystem.AddDevice<Keyboard>();
                if (stage == 0)
                {
                    Application.runInBackground = true;
                    InputSystem.settings = UnityEngine.Object.Instantiate(InputSystem.settings);
                    InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
                    InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    InputSystem.AddDevice<Keyboard>("NumeriaTestKeyboard").MakeCurrent();
                    Require((int)Read(d, "pantalla") == 0 && !d.jugador.enabled, "Introducción no bloquea el movimiento.");
                    Call(d, "CambiarPantalla", 1); before = d.jugador.transform.position;
                    InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(UnityEngine.InputSystem.Key.W));
                }
                else if (stage == 1)
                {
                    InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
                    Require(d.jugador.transform.position.z > before.z + .05f, "W no mueve a Byte.");
                    Require(d.jugador.transform.position.y > .5f, "Byte atraviesa el suelo.");
                    Teleport(d, new Vector3(0, 1.05f, 2));
                }
                else if (stage == 2) InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(UnityEngine.InputSystem.Key.E));
                else if (stage == 3)
                {
                    InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
                    Require((int)Read(d, "pantalla") == 2 && !d.jugador.enabled, "E no abre el reto.");
                    d.Responder(0); Require(!d.retos[0].Completado && ((string)Read(d, "mensaje")).Contains("Pista"), "Falta pista tras fallo.");
                    d.Responder(1); Require(d.retos[0].Completado, "Primer reto no se completa.");
                    Call(d, "CambiarPantalla", 1);
                    Teleport(d, new Vector3(2, 1.05f, 6));
                    before = d.jugador.transform.position;
                    InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(UnityEngine.InputSystem.Key.W));
                }
                else if (stage == 4)
                {
                    InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
                    Require(d.jugador.transform.position.z > 7.3f, "Puerta resuelta bloquea el paso.");
                    Teleport(d, new Vector3(0, 1.05f, 10));
                }
                else if (stage == 5) InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(UnityEngine.InputSystem.Key.E));
                else if (stage == 6)
                {
                    InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
                    Require((int)Read(d, "pantalla") == 2, "Segundo terminal inaccesible.");
                    d.Responder(1); Require(!d.retos[1].Completado, "Segundo reto acepta error."); d.Responder(0);
                    Call(d, "CambiarPantalla", 1); Teleport(d, new Vector3(0, 1.05f, 18));
                }
                else if (stage == 7) InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(UnityEngine.InputSystem.Key.E));
                else if (stage == 8)
                {
                    InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState());
                    Require((int)Read(d, "pantalla") == 2, "Tercer terminal inaccesible.");
                    d.Responder(0); Require(!d.retos[2].Completado, "Tercer reto acepta error."); d.Responder(2);
                    Require(d.Resueltos == 3, "Progreso incorrecto.");
                    Call(d, "CambiarPantalla", 1); Teleport(d, d.portal.position);
                }
                else if (stage == 9)
                {
                    Require((int)Read(d, "pantalla") == 3 && !d.jugador.enabled, "Portal no muestra final.");
                    d.Reiniciar(); Require(d.Resueltos == 0 && (int)Read(d, "pantalla") == 0, "Reinicio no limpia progreso.");
                    Debug.Log("NUMERIA_PLAY_OK: introducción, WASD real, suelo, E, pistas, tres retos, paso por puerta, final y reinicio.");
                    SessionState.SetBool(Key, false); EditorApplication.Exit(0);
                }
                stage++;
            }
            catch (Exception ex) { Debug.LogException(ex); SessionState.SetBool(Key, false); EditorApplication.Exit(1); }
        }
    }
}



