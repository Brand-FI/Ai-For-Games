using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwitchSprite : MonoBehaviour
{
    private Image image;
    private Sprite original;
    void Start()
    {
        image = GetComponent<Image>();
        original = image.sprite;
    }

    public void SwitchIn()
    {
        image.sprite = original;
    }
    
    public void SwitchOut(Sprite sprite)
    {
        image.sprite = sprite;
    }
}
