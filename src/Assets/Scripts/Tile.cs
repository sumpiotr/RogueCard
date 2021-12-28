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
            GameController.instance.SpawnEnemy(character, transform.position);
        }

        if (seen) 
        {
            GameController.instance.tileInfoDescription.text = tileData.description;
            GameController.instance.tileInfoTitle.text = tileData.name;
            GameController.instance.tileInfoImage.sprite = tileData.image;
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
        PlayerController player = GameController.instance.player;
        if(!GameController.instance.battleMode && !player.busy && seen && IsWalkable()) 
        {
            player.Move(transform.position, player.stats.getActualStat(Stats.viewDistance), ()=> {
                player.EndAsyncAction();
                if(!GameController.instance.battleMode)GameController.instance.StartCoroutine("EnemyTurn");
            });
            return;
        }
        if (actionHighlight.gameObject.activeSelf)
        {
            player.HideTips();
            player.destination = transform.position;
            player.preparedCard.CardPlayed(player);
        }
    }

    private void SetDefaultInfo()
    {
        GameController.instance.tileInfoDescription.text = "Description";
        GameController.instance.tileInfoTitle.text = "Tile Name";
        GameController.instance.tileInfoImage.sprite = null;
    }

    public void SetOccupyingObject(GameObject obj = null) 
    {
        occupyingObject = obj;
    }

    public void SetOccupyingCharacter(BaseCharacter character = null)
    {
        occupyingCharacter = character;
        if(character != null && onCharacterEnter != null) 
        {
            onCharacterEnter(character);
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
