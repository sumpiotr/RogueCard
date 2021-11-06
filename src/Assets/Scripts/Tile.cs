using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField] TileDataScriptableObject tileData;

    [SerializeField] GameObject highlight;

    [SerializeField] SpriteRenderer actionHighlight;

    [SerializeField] Color moveColor;

    [SerializeField] Color attackColor;

    [SerializeField] BaseCharacter character;

    SpriteRenderer spriteRenderer;

    PlayerController player;

    GameObject occupyingObject = null;
    BaseCharacter occupyingCharacter = null;


    public void Initialize()
    {
        highlight.SetActive(false);
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = tileData.image;
    }

    private void OnMouseEnter()
    {
        highlight.SetActive(true);
    }

    //to remove
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonUp(1))
        {
            GameObject g = Instantiate(character.gameObject, transform);
            g.transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            BaseCharacter enemy = g.GetComponent<BaseCharacter>();
            enemy.SetGrid(GameController.gameController.player.GetGrid());
            enemy.Initialize();
            SetOccupyingCharacter(enemy);
        }


        GameController.gameController.tileInfoDescription.text = tileData.description;
        GameController.gameController.tileInfoTitle.text = tileData.name;
        GameController.gameController.tileInfoImage.sprite = tileData.image;
    }

    private void OnMouseExit()
    {
        highlight.SetActive(false);
        GameController.gameController.tileInfoDescription.text = "Description";
        GameController.gameController.tileInfoTitle.text = "Tile Name";
        GameController.gameController.tileInfoImage.sprite = null;
    }

    private void OnMouseUp()
    {
        PlayerController player = GameController.gameController.player;
        if (actionHighlight.gameObject.activeSelf)
        {
            player.HideTips();
            player.destination = transform.position;
            player.preparedCard.CardPlayed(player);
        }
    }

    public void SetOccupyingObject(GameObject obj = null) 
    {
        occupyingObject = obj;
    }

    public void SetOccupyingCharacter(BaseCharacter character = null)
    {
        occupyingCharacter = character;
    }

    public bool IsEmpty() 
    {
        return occupyingCharacter == null;
    }

    public BaseCharacter getOccupyingCharacter() 
    {
        return occupyingCharacter;
    }

    public bool IsWalkable() 
    {
        return tileData.walkable;
    }

    public void ChangeTileData(TileDataScriptableObject tileData) 
    {
        this.tileData = tileData;
        spriteRenderer.sprite = tileData.image;
    }

    public void SetActiveActionHighlight(bool active, bool move) 
    {
        actionHighlight.gameObject.SetActive(active);
        actionHighlight.color = move ? moveColor : attackColor;
    }

}
