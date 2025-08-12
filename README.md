# ColivriDigitalTwin_VR

**ColivriDigitalTwin_VR** es un demo de pistas en VR localizado en el laboratorio COLIVRI. El/la jugador(a) registra su nombre, elige dificultad (Fácil, Normal o Competitivo) y resuelve una clave de 3 dígitos.
El sistema registra el tiempo, actualiza un leaderboard (Top-10) con posición, nombre y mejor tiempo, y muestra un panel de estadísticas al finalizar.

---

## Contenidos

- [Características](#-características)
- [Tecnologías](#-tecnologías)
- [Estructura lógica](#-estructura-lógica)
- [Requisitos](#-requisitos)
- [Ejecutar en Unity (Editor)](#-ejecutar-en-unity-editor)
- [Construir y ejecutar APK (Quest/Android)](#-construir-y-ejecutar-apk-questandroid)
- [Cómo colaborar](#-cómo-colaborar)
- [Solución de problemas](#-solución-de-problemas)


---

## Características
- **Tres modos de juego**
  - **Fácil / Normal:** Cronómetro **ascendente** (CountUp).
  - **Competitivo:** Cronómetro **descendente** (CountDown) que parte del **mejor tiempo** del Top-1. Si llega a 00:00, se otorga **una extensión** igual a la diferencia **(tiempo 2.º − tiempo 1.º)**.
- **Leaderboard Top-10** con persistencia local (PlayerPrefs/JSON), formato de tiempo **mm:ss** y **resaltado** del jugador actual (si entra en Top-10).
- **Estadísticas de fin de partida** (nombre, tiempo, posición).
- **IU completa** con panel de registro, instrucciones, temporizador, “game over”, panel de ranking y estadísticas.
- **Transiciones suaves**: fundido/“blink” de cámara y enfoque hacia el leaderboard.
- **Audio de acierto** al resolver la clave.
- **Código modular:** (GameController, CodeManager, PlayerDataManager, HighScoreTable, TimerDef, CameraBlink, GameStatistics)

---

## Tecnologías
- **Unity** (LTS recomendado).
- **C#** con **TextMeshPro** para UI.
- **Meta/Oculus XR Interaction SDK** (interacción VR).
- **URP** opcional (si el proyecto se configura con Universal Render Pipeline).
> **Nota:** si importas modelos/prefabs y aparecen **rosado**, revisa y actualiza **materiales/shaders** a los de tu *render pipeline* (Standard o URP/HDRP) y reasigna materiales en el prefab.


---

## Estructura lógica

- **GameController:** Orquesta UI, flujo de estados, dificultades y transiciones (incluye audio de acierto).
- **CodeManager:** Entrada y validación de la **clave de 3 dígitos**; emite evento al acertar.
- **TimerDef:** Cronómetro **CountUp/CountDown** con `OnTimerFinished`.
- **PlayerDataManager:** Persistencia de jugadores y sesiones; cálculo de **BestTime**.
- **HighScoreTable:** Renderiza el **Top-10**: posición, nombre y tiempo **mm:ss**.
- **GameStatistics:** Estadísticas de la partida recién finalizada.
- **CameraBlink:** Fundidos (panel UI con `Image` a pantalla completa).

---

## Requisitos

-Unity Hub + Unity LTS (2021.3+ / 2022.3+ / 2023.2+).

-Paquetes:

  >TextMeshPro.

-Meta/Oculus XR (Meta Interaction SDK all-in-one).

- Proyecto configurado en URP (Universal Rendering Pipeline)

-Android Build Support (para compilar APK).

## Ejecutar en Unity (Editor)
Clonar el repositorio:

bash
Copy
Edit
git clone https://github.com/imagine-uniandes/ColivriDigitalTwin_VR

Abrir la carpeta del proyecto con Unity Hub (elige versión LTS compatible).

Instalar paquetes necesarios desde Window → Package Manager.

Abrir la escena principal (Assets/Scenes/MainModel)

Revisar asignaciones en Inspector (resumen):

GameController

Paneles: Initial, Registration, Instructions, Code, Timer, GameOver, HighScorePanel, StatsRankingPanel.

TimerDef: arrastra el componente del TimerPanel.

Audio (opcional): AudioSource (sin Play On Awake) y successClip.

TimerDef (en TimerPanel)

Arrastrar el TextMeshProUGUI del reloj al campo timerText.

El evento OnTimerFinished se suscribe también por código.

CameraBlink

Crear un FadePanel (UI → Panel) que cubra toda la pantalla; asignar su Image a fadeImage.

Pulsar Play:

Inicio → Registro → Dificultad → Juego → Ranking/Estadísticas → Reset.

## Construir y ejecutar APK (Quest/Android)
Instalar Android Build Support (Unity Hub → Installs → Add modules).

File → Build Settings → Android → Switch Platform.
Añadir la escena principal a Scenes In Build.

Project Settings:

Player → Other Settings:

Scripting Backend: IL2CPP

Target Architectures: ARM64

XR Plug-in Management: habilitar Oculus/Meta para Android.

(URP/HDRP) Asegurar coherencia con el pipeline y materiales.

Build o Build And Run para generar el .apk.

Instalar en Quest:

Activar Developer Mode.

O bien Meta Quest Developer Hub (Install APK), o con ADB:

bash
Copy
Edit
adb devices
adb install -r path/to/ColivriDigitalTwin_VR.apk
## Cómo colaborar
Flujo de ramas

main: estable.

develop: integración.

feature/<nombre-corto>: nuevas funcionalidades.

Pull Requests

Descripción clara del cambio.

Screenshots/GIFs si afecta a UI/VR.

Pruebas manuales: Editor y (si aplica) dispositivo.

Estilo

C#: PascalCase para clases/métodos públicos, camelCase para campos privados.

Prefabs/Scenes con nombres claros (UI_Registration, Panel_Timer, etc.).

Issues

Etiquetas: bug, enhancement, question, VR, UI, build.

Incluir pasos de reproducción, logs/stacktrace y versión de Unity/paquetes.

## Solución de problemas
Modelo/prefab magenta (rosado): materiales/shaders incompatibles. Convierte materiales a Standard o URP/Lit y re-asigna en el prefab; usa las utilidades de Upgrade Materials si estás en URP.

Avatar humanoide (Animator): para Humanoid, configura Rig = Humanoid y crea/arrastra el Avatar al Animator. Para Generic, no necesitas Avatar.

Timer no actualiza: verifica que el TimerDef.timerText esté asignado, evita duplicados de TimerDef y no mezcles lógicas antiguas (elimina countdownTime/timerRunning del GameController).

Leaderboard no resalta/actualiza: confirma llamada a highScoreTable.RefreshTable() tras guardar sesión y que el Top-10 se limita correctamente en HighScoreTable.
