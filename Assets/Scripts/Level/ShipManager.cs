using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShipManager : MonoBehaviour
{
    public void StartRun()
    {
        SceneManager.LoadScene(2);
    }
}
