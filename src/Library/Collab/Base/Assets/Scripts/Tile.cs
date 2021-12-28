using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField] TileDataScriptableObject tileData;

    [SerializeField] GameObject highlight;

    [SerializeField] GameObject moveHighlight;

    public 

    SpriteRenderer spriteRenderer;

    PlayerController player;

    GameObject occupyingObject = null;

    void Start()
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
           ChangeTileData(Resources.Load<TileDataScriptableObject>("Tiles/Wall"));
        }

        Text desc = GameObject.Find("DescriptionText").GetComponent<Text>();
        desc.text = tileData.description;
    }

    private void OnMouseExit()
    {
        highlight.SetActive(false);
    }

    private void OnMouseUp()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        player.HideMoveTips();
        player.Move(transform.position);

    }

    public void SetOccupyingObject(GameObject obj = null) 
    {
        occupyingObject = obj;
    }

    public bool IsEmpty() 
    {
        return occupyingObject == null;
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

    public void SetActiveMoveHighlight(bool active) 
    {
        moveHighlight.SetActive(active);
    }

}
