    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardGameObject : MonoBehaviour
{
    public CardDataScriptableObject cardData;
    [SerializeField]SpriteRenderer cardImageSpriteRenderer;
    [SerializeField] Text cardTittle;
    [SerializeField] Text description;
    [SerializeField] GameObject selectedHiglight;

    [SerializeField] GameObject cardObject;
    public string cardDescription = null;

    bool isMoving = false;

    Vector2 defaultPosition;



    public void InitializeCard(CardDataScriptableObject cardData) 
    {
        this.cardData = cardData;
        cardImageSpriteRenderer.sprite = cardData.image;
        cardTittle.text = cardData.cardName;
        cardDescription = getDynamicDescription(cardData.description);
        description.text = cardDescription;
        defaultPosition = transform.localPosition;
    }

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


    private void OnMouseEnter()
    {
        if (isMoving || GameController.Instance.isCardSelected || GameController.Instance.player.busy) return;
        //FindObjectOfType<CardPreview>().ShowCardDescription(this);
        if (GameController.Instance.player.preparedCard != null || !GameController.Instance.playerTurn) return;
        StopAllCoroutines();
        StartCoroutine("SelectedAnimation");
    }

    private void OnMouseExit()
    {
        if (isMoving || GameController.Instance.isCardSelected) return;
        FindObjectOfType<CardPreview>().HideCardPreview();
        if (GameController.Instance.player.preparedCard != null || !GameController.Instance.playerTurn || GameController.Instance.player.busy) return;
        StopAllCoroutines();
        StartCoroutine("UnselectedAnimation");
    }

    private void OnMouseDown()
    {
        if (GameController.Instance.player.preparedCard != null || !GameController.Instance.playerTurn || GameController.Instance.player.busy) return;
        isMoving = true;
        StopAllCoroutines();
        GameController.Instance.isCardSelected = true;
        FindObjectOfType<CardPreview>().HideCardPreview();
    }

    private void OnMouseDrag()
    {
        if (GameController.Instance.player.preparedCard != null || !GameController.Instance.playerTurn || GameController.Instance.player.busy) return;
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, -1);
    }

    private void OnMouseUp()
    {
        if (!GameController.Instance.playerTurn || GameController.Instance.player.busy) return;
        if (GameController.Instance.player.preparedCard != null) 
        {
            GameController.Instance.player.Undo();
            return; 
        }
        isMoving = false;
        if (transform.localPosition.x < -5 || transform.localPosition.x > -3 + GameController.Instance.player.getHandSize() * 2
            || transform.position.y > transform.parent.transform.position.y + 1) 
        {
            //GameController.instance.player.PlayCard(this);
            GameController.Instance.isCardSelected = false;
            StopAllCoroutines();
            StartCoroutine("UnselectedAnimation");
            return;
        }
        GameController.Instance.isCardSelected = false;
        transform.localPosition = defaultPosition;
    }

    IEnumerator SelectedAnimation() 
    {
        while(cardObject.transform.position.y < transform.parent.position.y  + defaultPosition.y + 1) 
        {
            cardObject.transform.position = new Vector2(transform.parent.position.x + defaultPosition.x, transform.parent.position.y + cardObject.transform.localPosition.y + 0.1f);
            yield return new WaitForSeconds(0.05f);
        }
    }

    IEnumerator UnselectedAnimation()
    {
        while (cardObject.transform.position.y > transform.parent.position.y + defaultPosition.y)
        {
            cardObject.transform.position = new Vector2(transform.parent.position.x + defaultPosition.x, cardObject.transform.position.y - 0.1f);
            yield return new WaitForSeconds(0.05f);
        }
        cardObject.transform.position = new Vector2(defaultPosition.x + transform.parent.position.x, defaultPosition.y + transform.parent.position.y);
    }

    public void setHiglight(bool active) 
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


