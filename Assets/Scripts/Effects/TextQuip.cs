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

    public void SetTextAndColor(string text, Color color)
    {
        this.text.text = text;
        this.text.outlineColor = color;
    }

    public void RemoveQuip()
    {
        GameObject.Destroy(gameObject.transform.parent.gameObject);
        GameObject.Destroy(this.gameObject);
       
    }


}
