using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : BaseCharacter
{

    [SerializeField] CardGameObject cardPrefab;
    Transform cardContainer;


    private List<Vector2> actionTipTilesPositions = new List<Vector2>();
    Dictionary<Card, CardGameObject> cardsGameobjects = new Dictionary<Card, CardGameObject>();

    private void Update()
    {
        //changing hand visibility
        if (Input.GetKeyDown(KeyCode.H)) 
        {
            cardContainer.gameObject.SetActive(!cardContainer.gameObject.activeSelf);
        }

        //Cancel card on esc
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            Undo();
        }

    }
    
    public override void SetGrid(Grid grid)
    {
        base.SetGrid(grid);
        updateFog(false);
    }


    #region Cards
    public void InitializeCards(Transform cardContainer)
    {
        this.cardContainer = cardContainer;
        Initialize();
        DrawFullHand();
      
    }

    public override Card CreateCard(CardDataScriptableObject cardData)
    {
        CardGameObject cardObject = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity, cardContainer);
        cardObject.InitializeCard(cardData);
        Card card = base.CreateCard(cardData);
        cardObject.gameObject.SetActive(false);
        cardsGameobjects.Add(card, cardObject);
        return card;
    }


    private void CreateCardObject(Card card)
    {
        CardGameObject cardObject = cardsGameobjects[card];
        cardObject.gameObject.SetActive(true);
        cardObject.gameObject.transform.localPosition = new Vector2(-4 + 2 * hand.Count - 1, 0);
        cardObject.UpdateDefaultPosition();
    }

    public override Card DrawCard()
    {
        Card card = base.DrawCard();
        CreateCardObject(card);
        return card;
    }

    public override void DrawFullHand()
    {
        base.DrawFullHand();
    }

    public override void DiscardCardFromHand(Card card)
    {
        base.DiscardCardFromHand(card);
        CardGameObject cardObject = cardsGameobjects[card];
        cardObject.gameObject.SetActive(false);
        cardObject.setHiglight(false);
        SetHandCardsPositions();
    }

    public override void DiscardAllCardsFromHand()
    {
        foreach(Card card in hand) 
        {
            CardGameObject cardObject = cardsGameobjects[card];
            cardObject.gameObject.SetActive(false);
            cardObject.SetDefaultPosition();
        }
        base.DiscardAllCardsFromHand();
    }

    private void SetHandCardsPositions()
    {
        for (int i = 0; i < hand.Count; i++)
        {
            cardsGameobjects[hand[i]].gameObject.transform.localPosition = new Vector2(-4 + 2 * i, 0);
            cardsGameobjects[hand[i]].UpdateDefaultPosition();
        }
    }

    public void PlayCard(CardGameObject cardGameobject)
    {
        foreach (var pair in cardsGameobjects)
        {
            if (pair.Value == cardGameobject)
            {
                bool canPLay = pair.Key.PrepareCard(this);
                if (!canPLay)
                {
                    cardGameobject.SetDefaultPosition();
                    Undo();
                }
                return;
            }
        }
    }
    #endregion

    #region Move


    
    public bool ShowWalkableTilesInRange(int range)
    {
        /*Debug.Log(range);
        for(int x = 1; x <= range; x++) 
        {
            List<Tile> tiles = new List<Tile>();
            tiles.Add(grid.getTileByPosition(transform.position.x + x, transform.position.y));
            tiles.Add(grid.getTileByPosition(transform.position.x - x, transform.position.y));
            tiles.Add(grid.getTileByPosition(transform.position.x, transform.position.y + x));
            tiles.Add(grid.getTileByPosition(transform.position.x, transform.position.y - x));
            for (int y = 1; y <= range; y++) 
            {
              
                tiles.Add(grid.getTileByPosition(transform.position.x + x, transform.position.y + y));
                tiles.Add(grid.getTileByPosition(transform.position.x + x, transform.position.y - y));
                tiles.Add(grid.getTileByPosition(transform.position.x - x, transform.position.y - y));
                tiles.Add(grid.getTileByPosition(transform.position.x - x, transform.position.y + y));
            }

            foreach(Tile tile in tiles) 
            {
                if (tile == null) continue;
                List<Node> path = AStar.findPath(grid, transform.position, tile.transform.position, range);
                if (path != null)
                {
                    if (path.Count -1 > range) continue;
                }
                else 
                {
                    continue;
                }
                if (!tile.IsWalkable() || !tile.IsEmpty())continue;
                tile.SetActiveMoveHighlight(true);
                moveTipTilesPositions.Add(tile.transform.position);
                
            }

        }*/
        List<Vector2> tilesPositions = AStar.GetWalkableAndEmptyTilesInRange(transform.position, range, grid);
        foreach (Vector2 position in tilesPositions)
        {
            Tile tile = grid.GetTileByPosition(position);
            tile.SetActiveActionHighlight(true, true);
            actionTipTilesPositions.Add(tile.transform.position);
        }
        return actionTipTilesPositions.Count != 0;
    }

    public void HideTips()
    {
        foreach (Vector2 position in actionTipTilesPositions)
        {
            Tile tile = grid.GetTileByPosition(position);
            if (tile == null)
            {
                Debug.Log(position);
                continue;
            }
            tile.SetActiveActionHighlight(false, true);
        }
        actionTipTilesPositions.Clear();
    }
    #endregion

    #region Actions


    public override bool PrepareMove(int range, Card card)
    {
        if (!ShowWalkableTilesInRange(range)) return false;
        DefaultAction(card);
        return true;
    }

    public override bool PrepareAttack(int range, Card card, bool fromDestination = false)
    {
        Vector2 startPosition = fromDestination ? destination : (Vector2)transform.position;
        List<Vector2> tiles =  AStar.GetWalkableTilesInRange(startPosition, range, grid);
        List<Tile> enemyTiles = new List<Tile>();
        foreach (Vector2 tilePosition in tiles)
        {
            Tile tileObject = grid.GetTileByPosition(tilePosition);
            if (tileObject.getOccupyingCharacter() != null)
            {
                Debug.Log(CanHit(tilePosition, fromDestination));
                if (CanHit(tilePosition, fromDestination)) enemyTiles.Add(tileObject); 
            }
        }

        foreach(Tile tile in enemyTiles) 
        {
            tile.SetActiveActionHighlight(true, false);
            actionTipTilesPositions.Add(tile.transform.position);
        }

        DefaultAction(card);

        return enemyTiles.Count > 0;
    }

    public void DefaultAction(Card card) 
    {
        cardsGameobjects[card].SetDefaultPosition();
        preparedCard = card;
        cardsGameobjects[card].setHiglight(true);
    }

    public void Undo() 
    {
        HideTips();
        if (preparedCard != null)
        {
            cardsGameobjects[preparedCard].setHiglight(false);
            preparedCard = null;
        }
    }

    #endregion

    
     public override IEnumerator MoveCoroutine(List<Node> nodes, Action callback = null)
    {
        
        foreach (Node node in nodes)
        {
            updateFog(true);
            transform.position = node.nodePosition;
            updateFog(false); 
            yield return new WaitForSeconds(0.5f);
        }
        grid.GetTileByPosition(transform.position).SetOccupyingCharacter(this);
        //grid.GetTileByPosition(transform.position).setFog(false);
        isMoving = false;
        if (callback != null) callback();
        else
        {
            EndAsyncAction();
        }
        
       
    }

    public void updateFog(bool action)
    {
        grid.GetTileByPosition(transform.position).setFog(false);
        List<Vector2> tiles = AStar.GetTilesInRange(transform.position, 5, grid);
        Tile tile1;
        foreach (Vector2 tile in tiles)
        {
            if (tile != null)
            {
            tile1 = grid.GetTileByPosition(tile);
            tile1.setFog(action);
            }

        }
    }

}
