# ColivriDigitalTwin_VR

[![Unity](https://img.shields.io/badge/unity-2021.3%2B-blue.svg)](https://unity.com/)
[![Quest](https://img.shields.io/badge/Platform-Meta%20Quest-green.svg)](https://www.meta.com/quest/)
[![Status: Demo](https://img.shields.io/badge/status-Demo-important.svg)]()

---

> **ColivriDigitalTwin_VR** es una demo de pistas en realidad virtual (VR) localizado en el laboratorio COLIVRI. El/la jugador(a) registra su nombre, elige dificultad (Fácil, Normal o Competitivo), y resuelve una clave de 3 dígitos a través de pistas escondidas en todo el laboratorio. El sistema registra el tiempo, actualiza el leaderboard (Top 10), y muestra estadísticas al finalizar.

---

## Tabla de contenidos

- [Características principales](#características-principales)
- [Tecnologías utilizadas](#tecnologías-utilizadas)
- [Arquitectura y estructura](#arquitectura-y-estructura)
- [Requisitos](#requisitos)
- [Debug con Quest Link](#debug-con-quest-link)
- [Guía de instalación y ejecución](#guía-de-instalación-y-ejecución)
- [Compilación para Quest/Android](#compilación-para-questandroid)
- [Cómo colaborar](#cómo-colaborar)
- [Solución de problemas](#solución-de-problemas)

---

## Características principales

- **Tres modos de juego:**
  - *Fácil/Normal*: Cronómetro ascendente (CountUp) + ayuda Teleports activos.
  - *Normal*: Cronómetro ascendente (CountUp)
  - *Competitivo*: Cronómetro descendente (CountDown) desde el mejor tiempo. Extensión automática si el tiempo llega a 00:00. Sin ayudas adicionales de Teleports.
- **Leaderboard Top 10:**  
  - Persistencia local (PlayerPrefs/JSON) 
  - Formato mm:ss  
  - Resalta el nombre del jugador actual
- **Estadísticas finales:** Nombre, tiempo y posición.
- **UI completa:** Paneles de registro, instrucciones, temporizador, pistas dinámicas, game over, ranking y estadísticas.
- **Transiciones suaves:** Fundido “blink” de cámara y enfoque al leaderboard.
- **Audio feedback** al resolver la clave.
- **Código modular:** GameController, CodeManager, PlayerDataManager, HighScoreTable, TimerDef, CameraBlink, GameStatistics

---

>  **Nota:**  
> Para una experiencia visual óptima, utiliza prefabs y materiales compatibles con el pipeline URP o Standard según la configuración de proyecto.

---

## Tecnologías utilizadas

- **Unity** versión 2022.3.5f1
- **Unity Hub** versión 3.12.1
- **C#** + **TextMeshPro** para UI
- **Meta/Oculus XR Interaction SDK** (Meta XR All-In-One SDK)
- **URP** (Universal Render Pipeline) 
- **Android Build Support** (para compilar APK)

> **Nota:**  
> Si algún modelo/prefab aparece rosado, revisa y actualiza el material/shader en el Inspector (Standard/URP/HDRP).

---

## Arquitectura y estructura

- **GameController:** Contiene los paneles de la UI, display del ranking, timer, elementos específicos de UI, objetos del juego (Teleports) y Audio source.
- **CodeManager:** Entrada y validación de la clave
- **TimerDef:** Cronómetro CountUp/CountDown y eventos de finalización
- **PlayerDataManager:** Persistencia y cálculo de mejores tiempos
- **PlayerDataRegistration:** Manejo de selección modo de juego
- **HighScoreTable:** Renderiza Top 10 con posición, nombre y tiempo
- **GameStatistics:** Estadísticas de la partida finalizada
- **CameraBlink:** Fundidos de pantalla con panel UI

---

## Requisitos

- **Unity Hub** + Unity LTS (2021.3+, 2022.3+, 2023.2+)
- **Paquetes necesarios:**
  - TextMeshPro
  - Meta/Oculus XR (Interaction SDK all-in-one)
- **Proyecto configurado en URP** 
- **Android Build Support** (para Quest/Android)

> **ADVERTENCIA:**  
> La compilación para Quest solo funciona en ARM64 y XR Plug-in Management configurado correctamente. No olvidar agregar el módulo Android Build Support en Unity Hub.

---
## Debug con Quest Link
Durante el desarrollo, es posible probar y depurar el proyecto directamente desde el editor de Unity utilizando **Quest Link** (Meta Quest Link). Esto permite iterar rápidamente sin compilar un APK en cada cambio.

### Requisitos previos

- Visor **Meta Quest 2/Pro/3** en modo desarrollador  
- **Cable USB-C** 
- Aplicación **Meta Quest Link** instalada en el PC  
- Paquetes XR correctamente configurados en el proyecto

### Pasos para activar Quest Link en Unity

1. Conecta el visor al PC mediante cable USB-C o activa Air Link en Configuración del quest.  
2. En el visor, **acepta la solicitud de conexión y acceso a datos**.  
3. En la aplicación **Meta Quest Link** del PC, activa el modo Link y selecciona escritorio virtual.  
4. En Unity:
   - Asegurar de que el **XR Plug-in Management** esté habilitado para la plataforma **PC, Mac & Linux Standalone** con **Oculus/Meta** activo.
   - Cambia la plataforma a **PC** (*File → Build Settings → PC, Mac & Linux Standalone → Switch Platform*).
5. Pulsar **Play** en Unity para probar directamente la escena en VR.

---

## Guía de instalación y ejecución

1. **Clona el repositorio:**
   ```bash
   git clone https://github.com/imagine-uniandes/ColivriDigitalTwin_VR
   ```
2. Abre la carpeta con Unity Hub y selecciona la versión LTS compatible.
3. Instala los paquetes necesarios desde Window → Package Manager.
4. Abre la escena principal: `Assets/Scenes/MainModel`.
5. Verifica asignaciones en el Inspector:
   - **GameController:** Paneles (Initial, Registration, Instructions, Code, Timer, GameOver, HighScorePanel, StatsRankingPanel)
   - **TimerDef:** Arrastra el componente del TimerPanel, asigna el TextMeshProUGUI del reloj
   - **Audio (opcional):** Asigna AudioSource y successClip
   - **CameraBlink:** Crea un FadePanel (UI → Panel) y asigna su Image a fadeImage
6. Pulsa **Play** para iniciar el flujo: Registro → Dificultad → Juego → Ranking/Estadísticas → Reset

---

> **Nota:**  
> Puedes personalizar los paneles UI y el leaderboard cambiando colores y fuentes en el Inspector para que combinen con el branding de tu laboratorio o proyecto.

---

## Compilación para Quest/Android

1. Instala **Android Build Support** (Unity Hub → Installs → Add modules)
2. Ve a **File → Build Settings → Android** y haz *Switch Platform*
3. Añade la escena principal a *Scenes In Build*
4. Configura en **Project Settings → Player → Other Settings**:
   - Scripting Backend: IL2CPP
   - Target Architectures: ARM64
   - XR Plug-in Management: habilita Oculus/Meta para Android
   - Ajusta materiales según tu pipeline
5. Haz *Build* o *Build And Run* para generar el .apk
6. Instala en Quest:
   - Activa *Developer Mode* en el visor
   - Usa Meta Quest Developer Hub o ADB:
     ```bash
     adb devices
     adb install -r path/to/ColivriDigitalTwin_VR.apk
     ```

---

>  **IMPORTANTE:**  
> Si tienes problemas con shaders o materiales al exportar a Quest/Android, convierte los materiales a URP/Lit y actualiza los prefabs antes de compilar.

---

## Cómo colaborar

- **Ramas:**  
  - `main`: estable
    Para integración de nuevas funcionalidades con permisos crea una nueva rama llamada:
    - `develop/<nombre>`: nuevas funcionalidades
- **Pull Requests:**  
  - Descripción clara de los cambios realizados
  - Screenshots/GIFs si afecta UI/VR  
  - Pruebas manuales en Editor/dispositivo
- **Estilo:**  
  - C# camelCase para campos del código  (Nombramiento de funciones, variables, etc)
  - Nombres claros en prefabs/escenas
  - Organización de carpetas (Ej: Assets/Models para los modelos 3D en la escena, Assets/Scripts para agregar nuevos archivos .cs al proyecto )
- **Issues:**  
  - Usa etiquetas (`bug`, `question`, `VR`, `UI`, `build`)  
  - Incluye pasos de reproducción, logs/stacktrace y versión de Unity/paquetes

---

> **Consejo:**  
> Antes de abrir un PR, revisa que no haya duplicados y que el código compile tanto en Editor como en Android/Quest.

---

##  Solución de problemas

- **Material rosado:** Convierte materiales a Standard o URP/Lit y reasigna en el prefab
- **Timer no actualiza:** Verifica que `TimerDef.timerText` esté asignado y evita duplicados
- **Leaderboard no resalta/actualiza:** Confirma llamada a `highScoreTable.RefreshTable()` tras guardar sesión; limita correctamente el Top 10

---
Para explorar más sobre la documentación de Meta XR All-In-One ingrese al siguiente link: https://developers.meta.com/horizon/downloads/package/meta-xr-sdk-all-in-one-upm/ /n
Si quieres investigar más sobre el funcionamiento del paquete teleport ingrese al siguiente link: https://developers.meta.com/horizon/documentation/unity/unity-isdk-teleport-interaction/

>  **ADVERTENCIA:**  
> Si experimentas errores al compilar para Quest, revisa que todos los paquetes estén actualizados y que los materiales sean compatibles con Android y URP.

---
