using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CanvasCard : MonoBehaviour, IDragHandler, IBeginDragHandler, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{

    public const float BOTTOM_MARGIN = 40;
    public const float WIDTH = 80;

    public CardDataScriptableObject cardData;
    [SerializeField] Image cardImage;
    [SerializeField] Text cardTittle;
    [SerializeField] Text description;
    [SerializeField] GameObject selectedHiglight;
    public string cardDescription = null;

    bool isMoving = false;

    Vector2 defaultPosition;

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
            desc = desc.Replace("[" + s + "]", "<b>" + GameController.instance.player.stats.getActualStat(s).ToString() + "</b>");
        }
        return desc;
    }

    public void InitializeCard(CardDataScriptableObject cardData)
    {
        this.cardData = cardData;
        cardImage.sprite = cardData.image;
        cardTittle.text = cardData.cardName;
        cardDescription = getDynamicDescription(cardData.description);
        description.text = cardDescription;
        defaultPosition = transform.localPosition;
    }

    #region interfaces
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("drag");
    }
    public void OnDrag(PointerEventData eventData)
    {
        if (GameController.instance.player.preparedCard != null || !GameController.instance.playerTurn || GameController.instance.player.busy) return;
        transform.position = new Vector3(eventData.position.x, eventData.position.y, -1);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isMoving || GameController.instance.isCardSelected || GameController.instance.player.busy) return;
        FindObjectOfType<CardPreview>().ShowCardDescription(this);
        //if (GameController.instance.player.preparedCard != null || !GameController.instance.playerTurn) return;
        //StopAllCoroutines();
        //StartCoroutine("SelectedAnimation");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isMoving || GameController.instance.isCardSelected) return;
        FindObjectOfType<CardPreview>().HideCardPreview();
        //if (GameController.instance.player.preparedCard != null || !GameController.instance.playerTurn || GameController.instance.player.busy) return;
        //StopAllCoroutines();
        //StartCoroutine("UnselectedAnimation");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!GameController.instance.playerTurn || GameController.instance.player.busy) return;
        if (GameController.instance.player.preparedCard != null)
        {
            GameController.instance.player.Undo();
            return;
        }
        isMoving = false;

        if (transform.localPosition.x < -2 * WIDTH * 1.5 - WIDTH / 2 || transform.localPosition.x > -2 * WIDTH * 1.5 + GameController.instance.player.getHandSize() * WIDTH * 1.5 + WIDTH / 2
          || transform.position.y > transform.parent.transform.position.y + 140)
        {
            GameController.instance.player.PlayCard(this);
            GameController.instance.isCardSelected = false;
            //StopAllCoroutines();
            //StartCoroutine("UnselectedAnimation");
            return;
        }

        GameController.instance.isCardSelected = false;
        transform.localPosition = defaultPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (GameController.instance.player.preparedCard != null || !GameController.instance.playerTurn || GameController.instance.player.busy) return;
        isMoving = true;
        //StopAllCoroutines();
        GameController.instance.isCardSelected = true;
        FindObjectOfType<CardPreview>().HideCardPreview();
    }
    #endregion

    public void SetHiglight(bool active)
    {
        selectedHiglight.SetActive(active);
    }
    public void UpdateDefaultPosition()
    {
        defaultPosition = transform.localPosition;
    }


    public void SetDefaultPosition()
    {
        transform.localPosition = defaultPosition;
    }
}
