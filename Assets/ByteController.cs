using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class ByteController : MonoBehaviour
{
    [SerializeField] private float velocidad = 5f;
    [SerializeField] private float gravedad = -20f;
    [SerializeField] private float velocidadGiro = 10f;
    public bool controlesAventura;
    private CharacterController controlador;
    private Vector3 vertical;
    private Vector3 movimiento;
    public Vector3 Movimiento => movimiento;
    private void Awake() { controlador = GetComponent<CharacterController>(); }
    private void OnEnable() { movimiento = Vector3.zero; vertical = Vector3.zero; }
    private void Update()
    {
        var k = Keyboard.current;
        Vector2 input = Vector2.zero;
        if (k != null)
        {
            input.x = ((k.dKey.isPressed || k.rightArrowKey.isPressed) ? 1 : 0) - ((k.aKey.isPressed || k.leftArrowKey.isPressed) ? 1 : 0);
            input.y = ((k.wKey.isPressed || k.upArrowKey.isPressed) ? 1 : 0) - ((k.sKey.isPressed || k.downArrowKey.isPressed) ? 1 : 0);
        }
        input = Vector2.ClampMagnitude(input, 1);
        Vector3 direction = new Vector3(input.x, 0, input.y);
        if (controlesAventura && Camera.main != null)
        {
            Vector3 forward = Vector3.ProjectOnPlane(Camera.main.transform.forward, Vector3.up).normalized;
            Vector3 right = Vector3.ProjectOnPlane(Camera.main.transform.right, Vector3.up).normalized;
            direction = forward * input.y + right * input.x;
        }
        float speed = velocidad * (controlesAventura && k != null && k.leftShiftKey.isPressed ? 1.35f : 1);
        movimiento = controlesAventura ? Vector3.MoveTowards(movimiento, direction * speed, (input.sqrMagnitude > 0 ? 32 : 45) * Time.deltaTime) : direction * speed;
        if (direction.sqrMagnitude > .01f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 1 - Mathf.Exp(-velocidadGiro * Time.deltaTime));
        if (controlador.isGrounded && vertical.y < 0) vertical.y = -2;
        vertical.y += gravedad * Time.deltaTime;
        controlador.Move((movimiento + vertical) * Time.deltaTime);
    }
}
