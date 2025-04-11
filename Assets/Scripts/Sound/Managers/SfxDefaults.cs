using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxDefaults : MonoBehaviour
{
    [SerializeField] private SfxReference _uiButtonHover;
    [SerializeField] private SfxReference _uiButtonClick;

    public SfxReference UIButtonHover => _uiButtonHover;
    public SfxReference UIButtonClick => _uiButtonClick;
}
