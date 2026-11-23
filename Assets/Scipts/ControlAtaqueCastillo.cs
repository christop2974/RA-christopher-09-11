using UnityEngine;
using UnityEngine.InputSystem;
// Asignar al objeto raíz del PREFAB TableroBatalla.
public class ControlAtaqueCastillo : MonoBehaviour
{
    [SerializeField] private Animator animadorProyectil;
    private bool ataqueEnCurso;
    private bool lanzamientoObservado;
    private void Update()
    {
        // PC: T ataca. R reinicia manualmente si se necesita depurar.
        if (Keyboard.current != null)
        {
            if (Keyboard.current.tKey.wasPressedThisFrame) IntentarAtaque();
            if (Keyboard.current.rKey.wasPressedThisFrame) ReiniciarPrueba();
        }
        if (!ataqueEnCurso || animadorProyectil == null) return;
        AnimatorStateInfo estado = animadorProyectil.GetCurrentAnimatorStateInfo(0);
        bool transicion = animadorProyectil.IsInTransition(0);
        if (estado.IsName("Lanzamiento"))
            lanzamientoObservado = true;
        // El Animator volvió a Reposo después de completar el lanzamiento.
        if (lanzamientoObservado && estado.IsName("Reposo") && !transicion)
        {
            ataqueEnCurso = false;
            lanzamientoObservado = false;
            Debug.Log("ATAQUE: Animación terminada. Listo para disparar otra vez.");
        }
    }
    public bool IntentarAtaque()
    {
        if (animadorProyectil == null)
        {
            Debug.LogError("ATAQUE: Asigna Animator del hijo ProyectilAtaque.");
            return false;
        }
        if (ataqueEnCurso || animadorProyectil.IsInTransition(0) ||
        !animadorProyectil.GetCurrentAnimatorStateInfo(0).IsName("Reposo"))
        {
            Debug.Log("ATAQUE: Espera a que termine la animación.");
            return false;
        }
        ataqueEnCurso = true;
        lanzamientoObservado = false;
        animadorProyectil.ResetTrigger("Atacar");
        animadorProyectil.SetTrigger("Atacar");
        Debug.Log("ATAQUE: Proyectil lanzado.");
        return true;
    }
    public void ReiniciarPrueba()
    {
        ataqueEnCurso = false;

        lanzamientoObservado = false;
        if (animadorProyectil != null)
        {
            animadorProyectil.ResetTrigger("Atacar");
            animadorProyectil.Play("Reposo", 0, 0f);
            animadorProyectil.Update(0f);
        }
        Debug.Log("ATAQUE: Reinicio manual de prueba.");
    }
}