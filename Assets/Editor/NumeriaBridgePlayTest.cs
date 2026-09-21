using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace ByteAdventure.Editor
{
    [InitializeOnLoad]
    public static class NumeriaBridgePlayTest
    {
        const string Flag = "Numeria.Bridge.Play";
        class Step { public Vector3 target; public Key key; public Action check; public string name; }
        static List<Step> steps;
        static int index;
        static double wait;
        static Action pending;
        static Keyboard keyboard;
        static Key[] lastKeys = Array.Empty<Key>();
        static int capturedStage = -1;
        static float cutStart;
        static int errors;
        static bool restarting;
        static NumeriaBridgePlayTest()
        {
            EditorApplication.update += Tick;
            Application.logMessageReceived += (message, stack, type) => { if ((type == LogType.Exception || type == LogType.Error) && (stack.Contains("NumeriaPuente") || stack.Contains("ByteController") || stack.Contains("CamaraSeguimiento"))) errors++; };
        }
        public static void Run()
        {
            NumeriaAdventureBuilder.BatchBuild();
            SessionState.SetBool(Flag, true); SessionState.SetString(Flag+"Time", DateTime.UtcNow.ToString("O"));
            EditorApplication.isPlaying = true;
        }
        static void Require(bool value, string text) { if (!value) throw new Exception(text); }
        static void Keys(params Key[] keys)
        {
            if (lastKeys.SequenceEqual(keys)) return;
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys)); lastKeys = keys;
        }
        static NumeriaObjeto Find(NumeriaPuente g, NumeriaObjeto.Tipo type, int value = 0, int occurrence = 0) { return g.objetos.Where(o=>o.tipo == type && o.valor == value).Skip(occurrence).First(); }
        static void Use(NumeriaPuente g, NumeriaObjeto.Tipo type, int value = 0, Action check = null, int occurrence = 0, Key key = Key.E)
        {
            var o = Find(g,type,value,occurrence);
            steps.Add(new Step { target = o.transform.position + new Vector3(0,0,-1.4f), key = key, check = check, name = type+" "+value });
        }
        static void Pad(NumeriaPuente g, int value, Action check = null) { steps.Add(new Step { target = Find(g,NumeriaObjeto.Tipo.Baliza,value).transform.position, key = Key.None, check = check, name = "Pisar "+value }); }
        static void Plan(NumeriaPuente g)
        {
            steps = new List<Step>();
            steps.Add(new Step { target = g.byteRobot.transform.position, key = Key.Escape, name = "Pausa", check = () => Require(g.Pausa && !g.byteRobot.enabled,"Pause failed") });
            steps.Add(new Step { target = g.byteRobot.transform.position, key = Key.Escape, name = "Continuar", check = () => Require(!g.Pausa && g.byteRobot.enabled,"Resume failed") });
            // Deliberate overload followed by recovery through the same in-world controls.
            Use(g,NumeriaObjeto.Tipo.Carga,1); Use(g,NumeriaObjeto.Tipo.Grua);
            Use(g,NumeriaObjeto.Tipo.Carga,2); Use(g,NumeriaObjeto.Tipo.Grua);
            Use(g,NumeriaObjeto.Tipo.Carga,3); Use(g,NumeriaObjeto.Tipo.Grua,0,()=>Require(g.Energia==0 && g.Etapa==0 && g.Mensaje.Contains("pasado"),"Overload hint/reset failed"));
            Use(g,NumeriaObjeto.Tipo.Carga,2); Use(g,NumeriaObjeto.Tipo.Grua);
            Use(g,NumeriaObjeto.Tipo.Carga,3); Use(g,NumeriaObjeto.Tipo.Grua,0,()=>Require(g.Etapa==1,"Crane repair failed"));
            Pad(g,4,()=>Require(g.Paso==0,"Wrong beacon should reset"));
            Pad(g,2); Pad(g,4); Pad(g,6); Pad(g,8,()=>Require(g.Etapa==2,"Sequence failed"));
            for (int i=0;i<6;i++) Use(g,NumeriaObjeto.Tipo.Semilla,1,null,i);
            Use(g,NumeriaObjeto.Tipo.Jardin,0); Use(g,NumeriaObjeto.Tipo.Jardin,0); Use(g,NumeriaObjeto.Tipo.Jardin,0);
            Use(g,NumeriaObjeto.Tipo.Jardin,1);
            Use(g,NumeriaObjeto.Tipo.Jardin,2); Use(g,NumeriaObjeto.Tipo.Jardin,2);
            Use(g,NumeriaObjeto.Tipo.Distribuidor,0,()=>Require(g.Etapa==2 && g.Mensaje.Contains("2 en cada"),"Distribution hint failed"));
            Use(g,NumeriaObjeto.Tipo.Jardin,0,()=>Require(g.Semillas==1,"Q must recover a seed"),0,Key.Q);
            Use(g,NumeriaObjeto.Tipo.Jardin,1);
            Use(g,NumeriaObjeto.Tipo.Distribuidor,0,()=>Require(g.Etapa==3,"Gardens repair failed"));
            Use(g,NumeriaObjeto.Tipo.Recuerdo,2);
            steps.Add(new Step { target = new Vector3(0,0,19), name="Acceso al puente" });
            steps.Add(new Step { target = new Vector3(0,0,26), name="Primer tramo", check=()=>Require(g.byteRobot.transform.position.y>.5f,"Bridge collision failed") });
            steps.Add(new Step { target = new Vector3(0,0,33), name="Segundo tramo" });
            steps.Add(new Step { target = new Vector3(0,0,41), name="Llegada", check=()=>Require(g.Finalizado,"Ending not reached") });
        }
        static void Capture(NumeriaPuente g, string name)
        {
            string folder = "C:/UnityProjects/ByteAdventure/Logs/BridgeCaptures"; Directory.CreateDirectory(folder);
            ScreenCapture.CaptureScreenshot(folder+"/"+name+"-game.png");
            var cam = Camera.main; var rt = new RenderTexture(1280,720,24); var oldTarget = cam.targetTexture; var oldActive = RenderTexture.active;
            ShaderUtil.allowAsyncCompilation = false; cam.targetTexture = rt; cam.Render(); cam.Render(); RenderTexture.active = rt;
            var tex = new Texture2D(1280,720,TextureFormat.RGB24,false); tex.ReadPixels(new Rect(0,0,1280,720),0,0); tex.Apply();
            File.WriteAllBytes(folder+"/"+name+"-world.png",tex.EncodeToPNG());
            cam.targetTexture = oldTarget; RenderTexture.active = oldActive; UnityEngine.Object.DestroyImmediate(tex); UnityEngine.Object.DestroyImmediate(rt);
        }
        static void Tick()
        {
            if (!SessionState.GetBool(Flag,false)) return;
            if (DateTime.TryParse(SessionState.GetString(Flag+"Time",""),out var start) && (DateTime.UtcNow-start.ToUniversalTime()).TotalSeconds>420) { Finish(1,"BRIDGE_PLAY_TIMEOUT"); return; }
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling || Time.timeSinceLevelLoad<1) return;
            try
            {
                var g=UnityEngine.Object.FindFirstObjectByType<NumeriaPuente>(); if (g==null) return;
                if (restarting) { Require(g.Intro && g.Etapa == 0 && g.Energia == 0 && g.Semillas == 0 && !g.Finalizado, "Restart failed"); Finish(0,"BRIDGE_RESTART_OK"); return; }
                if (steps==null)
                {
                    Application.runInBackground=true; InputSystem.settings=UnityEngine.Object.Instantiate(InputSystem.settings);
                    InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
                    InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
                    keyboard=InputSystem.AddDevice<Keyboard>("BridgeTestKeyboard"); keyboard.MakeCurrent();
                    Require(g.Intro && !g.byteRobot.enabled,"Intro should lock movement");
                    Capture(g,"00-intro"); Plan(g); Keys(Key.Enter); wait=Time.time+.4; return;
                }
                if (Time.time<wait) return;
                if (g.Cinematica)
                {
                    Keys(); if (capturedStage!=g.Etapa) { if (cutStart==0) cutStart=Time.time; if(Time.time-cutStart>1.7f) { Capture(g,"0"+g.Etapa+"-bridge"); capturedStage=g.Etapa; cutStart=0; } } return;
                }
                if (pending!=null) { var callback=pending; pending=null; callback(); }
                if(index>=steps.Count)
                {
                    Keys(); Require(g.Finalizado && g.Etapa==3 && errors==0,"End state or runtime errors"); Capture(g,"04-ending");
                    Debug.Log("BRIDGE_PLAY_OK: walked entire route with WASD/E/Q, overload and sequence recovery, unequal gardens, bridge collisions, postcard and ending. Time="+Time.timeSinceLevelLoad);
                    var camera = Camera.main; camera.transform.position = new Vector3(38,45,-32); camera.transform.LookAt(new Vector3(0,0,17)); Capture(g,"05-overview");
                    restarting=true; g.Reiniciar(); return;
                }
                var task=steps[index]; Vector3 delta=task.target-g.byteRobot.transform.position; delta.y=0;
                if(delta.magnitude<.4f || (g.Finalizado && index==steps.Count-1))
                {
                    Keys(); if(task.key!=Key.None) { InputSystem.QueueStateEvent(keyboard,new KeyboardState(task.key)); lastKeys=new[]{task.key}; }
                    pending=task.check; index++; wait=Time.time+.4;
                    Debug.Log("BRIDGE_STEP "+index+"/"+steps.Count+" "+task.name); return;
                }
                var keys=new List<Key>(); if(delta.x>.22f) keys.Add(Key.D); if(delta.x<-.22f) keys.Add(Key.A); if(delta.z>.22f) keys.Add(Key.W); if(delta.z<-.22f) keys.Add(Key.S); Keys(keys.ToArray());
            }
            catch(Exception ex) { Debug.LogException(ex); Finish(1,"BRIDGE_PLAY_FAILED"); }
        }
        static void Finish(int code,string message) { SessionState.SetBool(Flag,false); Debug.Log(message); EditorApplication.Exit(code); }
    }
}

