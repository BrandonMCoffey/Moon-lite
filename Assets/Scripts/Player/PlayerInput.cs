using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    // Mouse Inputs
    public static Vector3 MousePosition => Input.mousePosition;
    public static Vector3 MousePositionWorld(float z)
    {
        var ray = Camera.main.ScreenPointToRay(MousePosition);
        Plane xy = new Plane(Vector3.forward, new Vector3(0, 0, z));
        xy.Raycast(ray, out float distance);
        return ray.GetPoint(distance);
    }
    public static bool MouseLeftPressed => Input.GetMouseButtonDown(0);
    public static bool MouseLeftHeld => Input.GetMouseButton(0);
    public static bool MouseLeftReleased => Input.GetMouseButtonUp(0);
    public static bool MouseRightPressed => Input.GetMouseButtonDown(1);
    public static bool MouseRightHeld => Input.GetMouseButton(1);
    public static bool MouseRightReleased => Input.GetMouseButtonUp(1);
    public static float MouseScroll => Input.GetAxis("Mouse ScrollWheel");

    // Movement Inputs
    public static float PlayerMoveHorz => Input.GetAxisRaw("Horizontal");
    public static float PlayerMoveVert => Input.GetAxisRaw("Vertical");
    public static Vector3 PlayerMove => new Vector3(PlayerMoveHorz, PlayerMoveVert, 0);
    public static bool PlayerJump => Input.GetKey(KeyCode.Space);
    public static bool PlayerDash => false;// Input.GetKey(KeyCode.X);
    public static bool PlayerJetpack => PlayerJump;

    // Interactions
    public static bool InteractOrSubmit => Input.GetKeyDown(KeyCode.F) && MouseControllerManager.ActionsThisFrame == 0;
    public static bool Cancel => Input.GetKeyDown(KeyCode.Escape);

    // Weapon System
    public static bool SwitchToPreviousAmmo => Input.GetKeyDown(KeyCode.Q);
    public static bool SwitchToNextAmmo => Input.GetKeyDown(KeyCode.E);
    public static bool ReloadAmmo => Input.GetKeyDown(KeyCode.R);

    // Game Special Actions
    public static bool Throwable01 => Input.GetKeyDown(KeyCode.Z);
    public static bool Throwable02 => Input.GetKeyDown(KeyCode.X);
    public static bool Throwable03 => Input.GetKeyDown(KeyCode.C);
    public static bool Placeable01 => Input.GetKeyDown(KeyCode.V);
    public static bool Placeable02 => Input.GetKeyDown(KeyCode.G);
    public static bool Placeable03 => Input.GetKeyDown(KeyCode.T);

    // Debug Keys
    public static bool ToggleInfiniteAmmo => Input.GetKeyDown(KeyCode.I);
}
