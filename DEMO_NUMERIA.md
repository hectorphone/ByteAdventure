> Documento histórico de la demo anterior. Para la aventura actual, consulta AVENTURA_NUMERIA.md. Abre Numeria_Anterior.unity para usar las instrucciones de esta página.

# ByteAdventure · Demo Numeria

## Jugar en Unity 6 (6000.4.4f1, URP)

1. Abre `Assets/Scenes/SampleScene.unity`. Si ya estaba abierta antes de recibir los cambios, vuelve a abrirla para cargar la versión guardada.
2. Espera a que Unity termine de importar y compilar.
3. Pulsa Play y haz clic en la pestaña Game. En la introducción, pulsa Enter o el botón Entrar en Numeria.
4. Muévete con WASD. Acércate al terminal y pulsa E. Selecciona una respuesta con el ratón o las teclas 1, 2 y 3 de la fila superior.
5. Si fallas, lee la pista y vuelve a responder. Tras acertar, pulsa Continuar o Enter; la puerta permanece abierta.
6. Repara los tres circuitos y entra en el portal dorado. Usa Jugar otra vez para reiniciar. Esc permite pausar o salir del reto.

Las respuestas son 5, 5 y 6 (botones 2, 1 y 3).

## Preparación automática

`Tools > ByteAdventure > Preparar demo Numeria` reconstruye y guarda la demo en la escena existente, incluidos materiales y referencias. No requiere arrastrar objetos. Se puede repetir sin duplicar el mundo ni el aspecto de Byte. Los antiguos ZonaReto y PuertaMatematica se conservan desactivados. Se mantienen ByteController y CamaraSeguimiento.

La herramienta guarda SampleScene; guarda antes en otra escena cualquier modificación manual que quieras conservar. La escena anterior está respaldada fuera de Assets en `Logs/SampleScene.before-demo.unity`.

`Tools > ByteAdventure > Completar aspecto provisional de Byte` actualiza solo su aspecto. `Tools > ByteAdventure > Validar demo` comprueba referencias, URP, respuestas, reinicio y ausencia de duplicados. Ejecutar fuera de Play.

## Comprobaciones automatizadas

Con Unity cerrado para el proyecto que se va a verificar:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.4.4f1\Editor\Unity.exe' -batchmode -nographics -projectPath 'C:\UnityProjects\ByteAdventure' -executeMethod ByteAdventure.Editor.NumeriaBuilder.BatchBuild -quit -logFile 'Logs/Numeria-build.log'
& 'C:\Program Files\Unity\Hub\Editor\6000.4.4f1\Editor\Unity.exe' -batchmode -nographics -projectPath 'C:\UnityProjects\ByteAdventure' -executeMethod ByteAdventure.Editor.NumeriaSmokeTest.Run -logFile 'Logs/Numeria-play.log'
```

La segunda prueba entra en Play, inyecta teclas W y E en Input System, verifica pistas y progreso, atraviesa una puerta resuelta y comprueba final y reinicio. Se cierra por sí sola con código 0 si pasa. Las respuestas de la prueba se envían por el mismo método que utiliza la interfaz. No sustituye una revisión visual manual de Game.

Para generar un ejecutable: File > Build Profiles > Windows > Build. SampleScene está incluida como escena de inicio.

## Resultado de esta entrega

- Unity 6000.4.4f1 compiló los ensamblados de juego y editor en `C:\UnityProjects\ByteAdventureVerification`, copia aislada del proyecto para no cerrar el editor abierto.
- Validación de escena: `NUMERIA_BUILD_OK` y `NUMERIA_VALIDATE_OK`.
- Prueba real en Play: `NUMERIA_PLAY_OK` en `Logs/Numeria-play-final.log`. Verificó movimiento W, interacción E, suelo, pistas, los tres retos, cruce de puerta abierta, final y reinicio.
- En la importación inicial Unity actualizó APIs de sus paquetes. La compilación posterior terminó correctamente, sin errores C# de la demo.
- El registro conserva avisos del servicio de licencias y una excepción de `UnityEditor.Search.SearchDatabase` al indexar. No impidieron las comprobaciones; no se han modificado esos paquetes ni las credenciales.
- No se ha generado un ejecutable Windows ni realizado una partida manual completa. La interfaz y los controles deben revisarse también desde Game.
- No se modificaron archivos `.csproj`, `ByteController.cs` ni `CamaraSeguimiento.cs`.

Archivos editados: `Assets/RetoMatematico.cs`, `Assets/Editor/ByteAppearanceBuilder.cs`, `Assets/Scenes/SampleScene.unity`, `ProjectSettings/EditorBuildSettings.asset`.

Archivos añadidos: `Assets/ByteAdventure/NumeriaDemo.cs`, `Assets/Editor/NumeriaBuilder.cs`, `Assets/Editor/NumeriaSmokeTest.cs`, ocho materiales en `Assets/ByteAdventure/Materials/`, sus archivos `.meta`, esta guía y los registros/respaldo de `Logs/`.

Revisión visual: se renderizó y revisó la vista inicial con URP y compilación síncrona de shaders. Imagen: `Logs/Numeria-preview.png`. Se comprobó el color azul de Byte, su pantalla oscura, ojos cian y materiales del recorrido. Esta imagen no incluye la interfaz OnGUI.

Los scripts y SampleScene entregados se compararon mediante SHA-256 con la copia verificada: coinciden.

