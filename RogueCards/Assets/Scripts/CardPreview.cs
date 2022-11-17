using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardPreview : MonoBehaviour
{
    [SerializeField] GameObject cardPreview;
    [SerializeField] Image cardPreviewImage;
    [SerializeField] Text cardTitleText;
    [SerializeField] Text cardDescriptionText;

    public static CardPreview Instance = null;

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
    }


    void Start()
    {
        HideCardPreview();
    }


    public void ShowCardDescription(CanvasCard card)
    {
        BasicCardDataScriptableObject data = card.cardData;
        cardPreview.SetActive(true);
        cardPreviewImage.sprite = data.image;
        cardTitleText.text = data.cardName;
        cardDescriptionText.text = card.cardDescription;
    }

    public void HideCardPreview()
    {
        cardPreview.SetActive(false);
    }
}
