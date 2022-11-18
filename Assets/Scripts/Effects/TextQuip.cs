using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextQuip : MonoBehaviour
{
    public TMPro.TMP_Text text;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetProperties(string text, Color color, bool goingLeft)
    {
        this.text.text = text;
        this.text.outlineColor = color;
        GetComponent<Animator>().SetInteger("FlyDirection", goingLeft ? -1 : 1);
    }

    public void RemoveQuip()
    {
        GameObject.Destroy(gameObject.transform.parent.gameObject);
        GameObject.Destroy(this.gameObject);
       
    }


}
