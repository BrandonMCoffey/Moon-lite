using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipManager : MonoBehaviour
{
    public void StartRun()
    {
        SceneManager.LoadScene(2);
    }
}
