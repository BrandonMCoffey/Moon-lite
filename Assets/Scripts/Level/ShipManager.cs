using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipManager : MonoBehaviour
{
    public void StartRun()
    {
        GameManager.LoadPlanetScene();
    }
}
