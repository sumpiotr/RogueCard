using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CanvasCard : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{

    public const float BOTTOM_MARGIN = 40;
    public const float WIDTH = 80;

    //data
    public Card card = null;
    public BasicCardDataScriptableObject cardData;
    public string cardDescription = null;
    
    //canvas objects
    [SerializeField] Image cardImage;
    [SerializeField] Text cardTittle;
    [SerializeField] Text description;

    bool _isMoving = false;

    Vector2 _defaultPosition;

    private string getDynamicDescription(string data)
    {
        string tmp = data;
        List<string> arguments = new List<string>();
        while (true)
        {
            if (tmp.IndexOf("[") == -1) break;
            string m1 = tmp.Substring(tmp.IndexOf("[") + 1, tmp.IndexOf("]") - tmp.IndexOf("[") - 1);
            arguments.Add(m1);
            if (tmp.IndexOf("]") + 1 >= tmp.Length - 1 || tmp.IndexOf("]") == -1) break;
            tmp = tmp.Substring(tmp.IndexOf("]") + 1, tmp.Length - tmp.IndexOf("]") - 1);
            if (tmp.IndexOf("]") + 1 >= tmp.Length - 1 || tmp.IndexOf("]") == -1) break;
        }
        string desc = cardData.description;
        foreach (string s in arguments)
        {
            desc = desc.Replace("[" + s + "]", "<b>" + GameController.Instance.player.stats.getActualStat(s).ToString() + "</b>");
        }
        return desc;
    }

    public void InitializeCard(Card card)
    {
        this.card = card;
        cardData = card.cardData;
        cardImage.sprite = cardData.image;
        cardTittle.text = cardData.cardName;
        cardDescription = getDynamicDescription(cardData.description);
        description.text = cardDescription;
        _defaultPosition = transform.localPosition;
    }

    #region interfaces

    public void OnDrag(PointerEventData eventData)
    {
        if (GameController.Instance.player.preparedCard != null || !GameController.Instance.playerTurn || GameController.Instance.player.busy) return;
        transform.position = new Vector3(eventData.position.x, eventData.position.y, -1);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isMoving || GameController.Instance.isCardSelected || GameController.Instance.player.busy) return;
        FindObjectOfType<CardPreview>().ShowCardDescription(this);
        //if (GameController.instance.player.preparedCard != null || !GameController.instance.playerTurn) return;
        //StopAllCoroutines();
        //StartCoroutine("SelectedAnimation");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isMoving || GameController.Instance.isCardSelected) return;
        FindObjectOfType<CardPreview>().HideCardPreview();
        //if (GameController.instance.player.preparedCard != null || !GameController.instance.playerTurn || GameController.instance.player.busy) return;
        //StopAllCoroutines();
        //StartCoroutine("UnselectedAnimation");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!GameController.Instance.playerTurn || GameController.Instance.player.busy) return;
        if (GameController.Instance.player.preparedCard != null)
        {
            GameController.Instance.player.Undo();
            return;
        }
        _isMoving = false;

        if (transform.localPosition.x < -2 * WIDTH * 1.5 - WIDTH / 2 || transform.localPosition.x > -2 * WIDTH * 1.5 + GameController.Instance.player.getHandSize() * WIDTH * 1.5 + WIDTH / 2
          || transform.position.y > transform.parent.transform.position.y + 140)
        {
            GameController.Instance.player.PlayCard(this);
            GameController.Instance.isCardSelected = false;

            //StopAllCoroutines();
            //StartCoroutine("UnselectedAnimation");
            return;
        }

        GameController.Instance.isCardSelected = false;
        transform.localPosition = _defaultPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (GameController.Instance.player.preparedCard != null || !GameController.Instance.playerTurn || GameController.Instance.player.busy) return;
        _isMoving = true;
        //StopAllCoroutines();
        GameController.Instance.isCardSelected = true;
        FindObjectOfType<CardPreview>().HideCardPreview();
    }
    #endregion
    
    public void UpdateDefaultPosition()
    {
        _defaultPosition = transform.localPosition;
    }


    public void SetDefaultPosition()
    {
        transform.localPosition = _defaultPosition;
    }
}
