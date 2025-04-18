using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveDecoration : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private bool _flipCeilingSprites;
    [SerializeField] private bool _randomFlipX;
    [SerializeField] private List<Sprite> _caveFloorSprites = new List<Sprite>();
    [SerializeField] private List<Sprite> _caveCeilingSprites = new List<Sprite>();

    private void OnValidate()
    {
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Randomize(bool floor)
    {
        _spriteRenderer.sprite = GetRandomSprite(floor);
        _spriteRenderer.flipX = _randomFlipX ? Random.value > 0.5f : false;
        _spriteRenderer.flipY = floor ? false : _flipCeilingSprites;
    }

    private Sprite GetRandomSprite(bool floor)
    {
        if (floor) return _caveFloorSprites[Random.Range(0, _caveFloorSprites.Count - 1)];
        return _caveCeilingSprites[Random.Range(0, _caveCeilingSprites.Count - 1)];
    }
}
