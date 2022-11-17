using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [SerializeField] TileDataScriptableObject tileData;

    [SerializeField] GameObject highlight;

    [SerializeField] SpriteRenderer actionHighlight;

    [SerializeField] SpriteRenderer fog;

    [SerializeField] Color moveColor;

    [SerializeField] Color attackColor;

    [SerializeField] SimpleAI character;


    SpriteRenderer spriteRenderer;

    PlayerController player;

    GameObject occupyingObject = null;
    BaseCharacter occupyingCharacter = null;

    private Action<BaseCharacter> onCharacterEnter = null;

    public bool isFogEnabled = false;
    bool seen = false;
    public void Initialize()
    {
        fog.gameObject.SetActive(true);
        highlight.SetActive(false);
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = tileData.image;
        gameObject.layer = tileData.layer;
    }

    private void OnMouseEnter()
    {
        highlight.SetActive(true);
    }

    //to remove
    private void OnMouseOver()
    {
        if (seen) 
        {
            GameController.Instance.tileInfoDescription.text = tileData.description;
            GameController.Instance.tileInfoTitle.text = tileData.name;
            GameController.Instance.tileInfoImage.sprite = tileData.image;
        }
        else 
        {
            SetDefaultInfo();
        }
    }

    private void OnMouseExit()
    {
        highlight.SetActive(false);
        SetDefaultInfo();
    }

    private void OnMouseUp()
    {
        PlayerController player = GameController.Instance.player;
        if(!GameController.Instance.battleMode && !player.busy && seen && IsWalkable()) 
        {
            player.Move(transform.position, player.stats.getActualStat(Stats.viewDistance), ()=> {
                player.EndAction();
                if(!GameController.Instance.battleMode)GameController.Instance.StartCoroutine("EnemyTurn");
            });
            return;
        }
        if (actionHighlight.gameObject.activeSelf)
        {
            player.HideTips();
            List<Node> path = AStar.findPath(player.GetGrid(), player.transform.position, transform.position);
            player.preparedCard.destination = transform.position;
            if(path != null && path.Count > 1 && getOccupyingCharacter() != null) 
            {
                player.preparedCard.destination = path[path.Count - 2].nodePosition;
            }
            player.preparedCard.target = getOccupyingCharacter();
            player.preparedCard.EndPreperation(player);
        }
    }

    private void SetDefaultInfo()
    {
        GameController.Instance.tileInfoDescription.text = "Description";
        GameController.Instance.tileInfoTitle.text = "Tile Name";
        GameController.Instance.tileInfoImage.sprite = null;
    }

    public void SetOccupyingObject(GameObject obj = null) 
    {
        occupyingObject = obj;
    }

    public void SetOccupyingCharacter(BaseCharacter character = null)
    {
        occupyingCharacter = character;
        if(occupyingCharacter != null && onCharacterEnter != null) 
        {
            onCharacterEnter(occupyingCharacter);
        }
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
        onCharacterEnter = null;
        gameObject.layer = (int)Mathf.Log(tileData.layer, 2);
    }

    public void SetActiveActionHighlight(bool active, bool move) 
    {
        actionHighlight.gameObject.SetActive(active);
        actionHighlight.color = move ? moveColor : attackColor;
    }
    
    public void SetFog(bool action)
    {
        fog.gameObject.SetActive(action);
        isFogEnabled = action;
        if (!seen && !action)
        {
            seen = true;
            fog.color = new Color(0, 0, 0, 0.5f);
        }
        
    }

    public void SetOnCharacterEnter(Action<BaseCharacter> action) 
    {
        onCharacterEnter = action;
    }

    

}
