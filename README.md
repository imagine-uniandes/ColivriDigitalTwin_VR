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

- **Unity** (LTS recomendado: 2021.3+, 2022.3+, 2023.2+)
- **C#** + **TextMeshPro** para UI
- **Meta/Oculus XR Interaction SDK** (Meta XR All-In-One SDK)
- **URP** (Universal Render Pipeline) 
- **Android Build Support** (para compilar APK)

> **Nota:**  
> Si algún modelo/prefab aparece rosado, revisa y actualiza el material/shader en el Inspector (Standard/URP/HDRP).

---

## Arquitectura y estructura

- **GameController:** Orquesta la UI de estados y transiciones
- **CodeManager:** Entrada y validación de la clave
- **TimerDef:** Cronómetro CountUp/CountDown y eventos de finalización
- **PlayerDataManager:** Persistencia y cálculo de mejores tiempos
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
  - `develop`: integración  
  - `feature/<nombre>`: nuevas funcionalidades
- **Pull Requests:**  
  - Descripción clara  
  - Screenshots/GIFs si afecta UI/VR  
  - Pruebas manuales en Editor/dispositivo
- **Estilo:**  
  - C# PascalCase para clases/métodos públicos, camelCase para campos privados  
  - Nombres claros en prefabs/escenas
- **Issues:**  
  - Usa etiquetas (`bug`, `enhancement`, `question`, `VR`, `UI`, `build`)  
  - Incluye pasos de reproducción, logs/stacktrace y versión de Unity/paquetes

---

> **Consejo:**  
> Antes de abrir un PR, revisa que no haya duplicados y que el código compile tanto en Editor como en Android/Quest.

---

##  Solución de problemas

- **Material rosado:** Convierte materiales a Standard o URP/Lit y reasigna en el prefab
- **Timer no actualiza:** Verifica que `TimerDef.timerText` esté asignado y evita duplicados
- **Leaderboard no resalta/actualiza:** Confirma llamada a `highScoreTable.RefreshTable()` tras guardar sesión; limita correctamente el Top-10

---
Para explorar más sobre la documentación de Meta XR All-In-One ingrese al siguiente link: https://developers.meta.com/horizon/downloads/package/meta-xr-sdk-all-in-one-upm/

>  **ADVERTENCIA:**  
> Si experimentas errores al compilar para Quest, revisa que todos los paquetes estén actualizados y que los materiales sean compatibles con Android y URP.

---
