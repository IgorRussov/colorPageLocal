using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveObject : MonoBehaviour
{
    public void Remove()
    {
        GameObject.Destroy(this.gameObject);
    }
}
