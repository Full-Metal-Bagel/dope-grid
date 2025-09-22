using UnityEngine;

public class Items : MonoBehaviour
{
    private int _itemInstanceId = -1;

    public int NextItemInstanceId
    {
        get
        {
            _itemInstanceId++;
            return _itemInstanceId;
        }
    }
}
