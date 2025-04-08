using Sirenix.OdinInspector;
using UnityEngine;

public class StructureData : MonoBehaviour
{
    [SerializeField] private float _width;
    [SerializeField] private float _height;
    [SerializeField] private float _entryWidth;
    [SerializeField] private bool _entryLeft = true;
    [SerializeField] private bool _entryRight = true;

    public Vector4 Sizes => new Vector4(_entryLeft ? _entryWidth : 0, _width, _entryRight ? _entryWidth : 0, _height);

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + new Vector3(_width / 2, _height / 2, 0), new Vector3(_width, _height, 1));
        if (_entryLeft)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + new Vector3(-_entryWidth / 2 - 0.005f, _height / 2, 0), new Vector3(_entryWidth, _height, 1));
        }
        if (_entryRight)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position + new Vector3(_width + _entryWidth / 2 - 0.005f, _height / 2, 0), new Vector3(_entryWidth, _height, 1));
        }
    }
}
