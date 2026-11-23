using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
// Este script va en ControlMesa, NO en el prefab TableroBatalla.
public class ColocarTableroEnMesa : MonoBehaviour
{
    [Header("Realidad Aumentada")]
    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private ARPlaneManager planeManager;
    [SerializeField] private GameObject tableroPrefab;
    [Header("Interfaz")]
    [SerializeField] private RectTransform panelBotones;
    [SerializeField] private Button botonAtacar;
    [SerializeField] private TMP_Text etiquetaBloqueo;
    [SerializeField] private TMP_Text textoEstado;
    private GameObject tableroActual;
    private ControlAtaqueCastillo controladorAtaque;
    private bool tableroBloqueado;
    private static readonly List<ARRaycastHit> impactos = new();
    // Habilitar el nuevo sistema táctil
    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        ActualizarInterfaz();
    }
    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
        // Restaurar la visualización al deshabilitar el controlador.
        MostrarVisualesPlanos(true);
    }
    private void Update()
    {
        // Si está bloqueado, no permitir mover el tablero.
        if (tableroBloqueado)
            return;
        // Comprobar si hay un toque en pantalla.
        if (Touch.activeTouches.Count == 0)
            return;
        var toque = Touch.activeTouches[0];
        if (toque.phase !=
        UnityEngine.InputSystem.TouchPhase.Began)
            return;
        // Evitar que tocar un botón mueva el tablero.
        if (panelBotones != null &&
        RectTransformUtility.RectangleContainsScreenPoint(
        panelBotones,
        toque.screenPosition,
   null))
            return;
        if (raycastManager == null || tableroPrefab == null)
        {
            Debug.LogError(
            "AR: Asigna Raycast Manager y Tablero Prefab."
            );
            return;
        }
        // Buscar una superficie válida.
        bool encontroSuperficie = raycastManager.Raycast(
        toque.screenPosition,
        impactos,
        TrackableType.PlaneWithinPolygon
        );
        if (!encontroSuperficie || impactos.Count == 0)
            return;
        Vector3 puntoMesa = impactos[0].pose.position;
        if (tableroActual == null)
        {
            // Crear el tablero una sola vez.
            tableroActual = Instantiate(
            tableroPrefab,
            puntoMesa,
            Quaternion.identity
            );
            controladorAtaque =
            tableroActual.GetComponent<ControlAtaqueCastillo>();
            Debug.Log("AR: Tablero colocado.");
        }
        else
        {
            // Reposicionar el tablero existente.
            tableroActual.transform.position = puntoMesa;
            Debug.Log("AR: Tablero reposicionado.");
        }
        ActualizarInterfaz();
    }
    // Ocultar continuamente los planos mientras esté bloqueado.
    private void LateUpdate()
    {
        if (tableroBloqueado)
        {
            MostrarVisualesPlanos(false);
        }
    }
    // BOTÓN 1: BLOQUEAR / DESBLOQUEAR
    public void AlternarBloqueo()
    {
        if (tableroActual == null)
        {
            Mensaje(
            "Primero coloca el tablero en una superficie."
            );
            return;
        }
        tableroBloqueado = !tableroBloqueado;
        // Ocultar o recuperar las mallas detectadas.
        MostrarVisualesPlanos(!tableroBloqueado);

        if (tableroBloqueado)
        {
            Mensaje(
            "Tablero bloqueado. Superficies ocultas."
            );
        }
        else
        {
            Mensaje(
            "Tablero desbloqueado. Puedes moverlo."
            );
        }
        ActualizarInterfaz();
    }
    // BOTÓN 2: ATACAR
    public void Atacar()
    {
        if (tableroActual == null)
        {
            Mensaje("Coloca primero el tablero.");
            return;
        }
        if (!tableroBloqueado)
        {
            Mensaje("Bloquea el tablero antes de atacar.");
            return;
        }
        if (controladorAtaque == null)
        {
            Mensaje(
            "Falta ControlAtaqueCastillo en TableroBatalla."
            );
            return;
        }
        bool ataqueEjecutado =
        controladorAtaque.IntentarAtaque();
        if (ataqueEjecutado)
        {
            Mensaje("¡Ataque ejecutado!");
        }
        else
        {
            Mensaje(
            "Espera a que termine el lanzamiento."
            );
        }
    }
    // Ocultar o mostrar TODAS las mallas de los planos AR.
    private void MostrarVisualesPlanos(bool mostrar)
    {
        if (planeManager == null)
            return;
        foreach (ARPlane plano in planeManager.trackables)
        {
            Renderer[] renderers =
            plano.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer rendererPlano in renderers)
            {
                rendererPlano.enabled = mostrar;
            }
        }
    }
    // Actualizar los botones y mensajes.
    private void ActualizarInterfaz()
    {
        if (botonAtacar != null)
        {
            botonAtacar.interactable =
            tableroActual != null && tableroBloqueado;
        }
        if (etiquetaBloqueo != null)
        {
            etiquetaBloqueo.text =
            tableroBloqueado ? "Desbloquear" : "Bloquear";
        }
        if (textoEstado != null && tableroActual == null)
        {
            textoEstado.text =
            "Busca una mesa y toca para colocar el tablero.";
        }
    }
    private void Mensaje(string mensaje)
    {
        Debug.Log("AR: " + mensaje);
        if (textoEstado != null)
        {
            textoEstado.text = mensaje;
        }
    }
}