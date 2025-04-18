using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HudVisuals : MonoBehaviour
{
    [SerializeField] private List<ItemVisual> _ammoVisuals = new List<ItemVisual>();

    private Player _player;

    private void Start()
    {
        _player = Player.Instance;
    }
}
