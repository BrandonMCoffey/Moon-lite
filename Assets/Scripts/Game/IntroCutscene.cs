using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroCutscene : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(CutsceneRoutine());
    }

    private IEnumerator CutsceneRoutine()
    {
        yield return new WaitForSeconds(5);
        GameManager.LoadPlanetScene();
    }
}
