# Numeria · El puente dormido

## Jugar

1. Abre `Assets/Scenes/SampleScene.unity` en Unity 6000.4.4f1 y espera la importación.
2. Si el editor tenía cargada la versión antigua, vuelve a abrir la escena guardada. También puedes ejecutar `Tools > ByteAdventure > Crear aventura del puente`: reconstruye y guarda todo automáticamente.
3. Pulsa Play, haz clic en Game y pulsa Enter.
4. WASD o flechas mueven a Byte; Shift permite correr. E interactúa; Q devuelve una carga o recupera una semilla del jardín cercano. Tab abre el cuaderno con una pista opcional; Esc pausa y M silencia los sonidos.

## Misión y ritmo previsto

Glitch apagó el puente del faro. Cada reparación levanta un tramo que se ve en una breve panorámica. El objetivo es una primera partida de 3–5 minutos incluyendo lectura, orientación, intentos y exploración; no hay esperas artificiales ni límite de tiempo. Una ruta conocida puede completarse más deprisa.

- **Energía / oeste:** busca las cargas entre los árboles y llévalas, una cada vez, a la grúa. Necesita cinco: 2 + 3. Si sobrecargas, vuelven todas a sus pedestales y recibes una pista. Q devuelve la carga que llevas.
- **Ruta / este:** camina por las balizas 2, 4, 6 y 8. No hay botones de respuesta. Se iluminan al acertar. Equivocarse reinicia la secuencia, sin quitar progreso del puente.
- **Vida / norte:** recoge las seis semillas de los tres refugios dorados y planta dos en cada jardín. E planta, Q recupera. Activa la fuente central para comprobar el reparto. Los brotes muestran lo que has colocado.
- Cruza el puente hasta el faro. Hay tres postales opcionales para explorar y un final con tiempo de partida y postales encontradas.

Si caes al océano digital, vuelves a tierra conservando el progreso y lo que llevas. Los efectos y sonidos son procedurales y no requieren recursos externos.

## Volver atrás

- `Tools > ByteAdventure > Abrir escena anterior` abre `Assets/Scenes/Numeria_Anterior.unity`, la demo de cuestionarios. No ejecuta el constructor de la aventura sobre ella.
- Los modos nuevos de movimiento y cámara solo están activados en la aventura. La escena anterior conserva sus opciones anteriores.
- `Logs/BeforeBridge-project.zip` contiene Assets, Packages y ProjectSettings previos al rediseño. Para una restauración exacta, extráelo en una carpeta de proyecto separada y ábrela con Unity; así no se mezclan versiones.
- El constructor conserva la primera copia de `Numeria_Anterior.unity`; ejecutarlo de nuevo no la sobrescribe.

## Herramientas y verificación

`Tools > ByteAdventure > Validar aventura del puente` verifica las referencias, URP, audio y ausencia de scripts perdidos. El constructor es repetible: reemplaza exclusivamente el mundo generado y reutiliza Byte y la cámara.

La prueba nueva es `ByteAdventure.Editor.NumeriaBridgePlayTest.Run`. Ejecutar en una copia del proyecto con Unity cerrado en esa copia:

```powershell
& 'C:\Program Files\Unity\Hub\Editor\6000.4.4f1\Editor\Unity.exe' -batchmode -projectPath 'C:\UnityProjects\ByteAdventureVerification' -executeMethod ByteAdventure.Editor.NumeriaBridgePlayTest.Run -logFile 'C:\UnityProjects\ByteAdventure\Logs\Bridge-play.log'
```

No usar `NumeriaSmokeTest.Run` para verificar esta aventura: es la prueba de la demo anterior y reconstruye esa versión.

## Provisional

- Geometría de editor y materiales planos: Byte, Glitch, árboles y faro aún no son modelos artísticos finales.
- Interfaz IMGUI; no incluye navegación completa con mando ni opciones avanzadas de accesibilidad.
- Sonidos sintetizados; faltan música, voces y mezcla final.
- La cámara tiene seguimiento y panorámicas, pero no un sistema general de evasión de obstáculos.
- No hay guardado entre sesiones, animación esquelética ni ejecutable de distribución.
- El objetivo de 3–5 minutos necesita validación con jugadores nuevos; el recorrido automatizado no mide tiempo de lectura ni comprensión.

## Archivos del rediseño

- Modificados: `Assets/ByteController.cs` y `Assets/CamaraSeguimiento.cs` (modos de aventura), `Assets/Scenes/SampleScene.unity`, `ProjectSettings/EditorBuildSettings.asset` y la cabecera de la guía histórica `DEMO_NUMERIA.md`.
- Nuevos scripts: `Assets/ByteAdventure/NumeriaPuente.cs`, `Assets/ByteAdventure/NumeriaObjeto.cs`, `Assets/Editor/NumeriaAdventureBuilder.cs` y `Assets/Editor/NumeriaBridgePlayTest.cs`.
- Nuevos recursos: materiales con prefijo `A_`, tres WAV en `Assets/ByteAdventure/Audio`, sus `.meta`, `Assets/Scenes/Numeria_Anterior.unity` y esta guía.
- Evidencias y copia de seguridad: `Logs/Bridge-build.log`, `Logs/Bridge-play-final.log`, `Logs/BridgeCaptures/` y `Logs/BeforeBridge-project.zip`.
- No se editaron `.csproj` ni se añadieron paquetes.

## Resultado verificado (21/09/2026)

- Compilación de scripts runtime y editor con Unity 6000.4.4f1: correcta, sin errores C#.
- `BRIDGE_BUILD_OK` y `BRIDGE_VALIDATE_OK` en `Logs/Bridge-play-final.log`.
- `BRIDGE_PLAY_OK`: 38 pasos de prueba con teclado virtual del Input System, caminando físicamente. Incluye pausa/reanudación, sobrecarga y recuperación, baliza incorrecta, reparto desigual, recuperación con Q, recogida de postal, cruce de los colliders de los tres tramos y final.
- `BRIDGE_RESTART_OK`: el reinicio vuelve a la introducción con energía, semillas y progreso a cero.
- Ruta automatizada: 100,9 segundos de tiempo de juego. No lee ni busca; la previsión de 3–5 minutos para una primera partida sigue pendiente de probarse con personas.
- Capturas reales de la cámara durante Play, revisadas: `Logs/BridgeCaptures/00-intro-world.png`, `01-bridge-world.png`, `02-bridge-world.png`, `03-bridge-world.png`, `04-ending-world.png`, `05-overview-world.png` y `06-exploration-world.png`. Las capturas de cámara no incluyen la interfaz IMGUI; el modo batch no produjo una captura de Game con dicha interfaz.
- Se compararon mediante SHA-256 los scripts y la escena entregados con la copia probada: coinciden. La escena anterior coincide exactamente con el respaldo previo.
- Unity sigue mostrando avisos de licencia y una excepción de su indexador `UnityEditor.Search.SearchDatabase` durante el arranque. No hubo excepciones de los scripts de juego y las comprobaciones terminaron correctamente.
- No se generó un ejecutable Windows ni se realizó una sesión manual con un jugador nuevo.
