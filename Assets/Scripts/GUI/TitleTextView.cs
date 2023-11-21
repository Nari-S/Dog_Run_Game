using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleTextView : MonoBehaviour
{
    [SerializeField] private Image titleTextImage;

    void Awake()
    {
        if (!TryGetComponent(out titleTextImage)) Debug.Log("Image is not attached to this object");
    }

    public void DeactivateTitleText()
    {
        titleTextImage.enabled = false;
    }
}
