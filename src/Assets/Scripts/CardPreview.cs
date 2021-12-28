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

    void Start()
    {
        HideCardPreview();
    }


    public void ShowCardDescription(CanvasCard card)
    {
        CardDataScriptableObject data = card.cardData;
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
