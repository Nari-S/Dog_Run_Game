using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScorePanelView : MonoBehaviour
{
    private float scalingDuration;

    [SerializeField] private Image scorePanelImage;
    [SerializeField] private RectTransform scorePanelRectTransform;

    void Awake()
    {
        if (!TryGetComponent(out scorePanelImage)) Debug.Log("Image is not attached to this object");
        if (!TryGetComponent(out scorePanelRectTransform)) Debug.Log("Rect Transform is not attached to this object");

        scalingDuration = 1f;
    }

    public void activateScorePanelImage()
    {
        scorePanelRectTransform.localScale = Vector3.zero;
        scorePanelRectTransform.DOScale(new Vector3(1f, 1f, 1f), scalingDuration);

        scorePanelImage.enabled = true;
    }
}
