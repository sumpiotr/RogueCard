using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : BaseCharacter
{

    [SerializeField] CanvasCard cardPrefab;
    private Transform _cardContainer;


    private List<CanvasCard> cardsGameobjects = new List<CanvasCard>();
    private List<Tile> actionTipTiles = new List<Tile>();
    private bool fog = false;

    private CardPreview _preview;

    private void Start()
    {
        _preview = FindObjectOfType<CardPreview>();
    }
    
    

    private void Update()
    {
        //changing hand visibility
        if (Input.GetKeyDown(KeyCode.H)) 
        {
            _cardContainer.gameObject.SetActive(!_cardContainer.gameObject.activeSelf);
            _preview.HideCardPreview();
        }

        //Cancel card on esc
        if (Input.GetKeyDown(KeyCode.Escape)) 
        {
            Undo();
        } 
        
        if (Input.GetKeyDown(KeyCode.P)) 
        {
            ChangeFogVisibility(fog);
            fog = !fog;
        }
        
    }
    


    #region Cards
    public void InitializeCards(Transform cardContainer)
    {
        this._cardContainer = cardContainer;
        foreach (CanvasCard card in this._cardContainer.gameObject.GetComponentsInChildren<CanvasCard>(true))
        {
            cardsGameobjects.Add(card);
        }
        Initialize();
    }

    public void HideHand()
    {
        _cardContainer.gameObject.SetActive(false);
    }

    public void ShowHand()
    {
        _cardContainer.gameObject.SetActive(true);
    }

    private CanvasCard GetFreeCard()
    {
        foreach (CanvasCard cardObject in cardsGameobjects)
        {
            if (!cardObject.gameObject.activeSelf) return cardObject;
        }

        return null;
    }

    private CanvasCard GetCardObject(Card card)
    {
        foreach (CanvasCard cardObject in cardsGameobjects)
        {
            if (cardObject.card == card) return cardObject;
        }
        return null;
    }

    /*public override Card CreateCard(BasicCardDataScriptableObject cardData)
    {
        CanvasCard cardObject = Instantiate(cardPrefab, Vector3.zero, Quaternion.identity, cardContainer);
        cardObject.transform.SetParent(cardContainer, false);
        cardObject.InitializeCard(cardData);
        Card card = base.CreateCard(cardData);
        cardObject.gameObject.SetActive(false);
        cardsGameobjects.Add(card, cardObject);
        return card;
    }*/


    private void ActiveCardObject(Card card)
    {
        CanvasCard cardObject = GetFreeCard();
        if (cardObject == null) return;
        cardObject.InitializeCard(card);
        cardObject.gameObject.SetActive(true);
        cardObject.gameObject.transform.localPosition = new Vector2(0 - ((CanvasCard.WIDTH * 1.5f) * 2) + (hand.Count - 1) * (CanvasCard.WIDTH * 1.5f), 0);
        cardObject.UpdateDefaultPosition();
    }

    public override Card DrawCard()
    {
        Card card = base.DrawCard();
        ActiveCardObject(card);
        return card;
    }

    public override void DiscardCardFromHand(Card card)
    {
        base.DiscardCardFromHand(card);
        CanvasCard cardObject = GetCardObject(card);
        if (cardObject == null) return;
        cardObject.gameObject.SetActive(false);
        SetHandCardsPositions();
    }

    public override void DiscardAllCardsFromHand()
    {
        foreach(Card card in hand) 
        {
            CanvasCard cardObject = GetCardObject(card);
            if (cardObject == null) continue;
            cardObject.gameObject.SetActive(false);
            cardObject.SetDefaultPosition();
        }
        base.DiscardAllCardsFromHand();
    }

    private void SetHandCardsPositions()
    {
        for (int i = 0; i < hand.Count; i++)
        { 
            CanvasCard cardObject = GetCardObject(hand[i]);
            cardObject.gameObject.transform.localPosition = new Vector2(0 - ((CanvasCard.WIDTH*1.5f)*2) + i * (CanvasCard.WIDTH * 1.5f), 0);
            cardObject.UpdateDefaultPosition();
        }
    }

    public void PlayCard(CanvasCard cardGameobject)
    {
        bool canPLay = cardGameobject.card.PrepareCard(this);
        if (!canPLay)
        {
            cardGameobject.SetDefaultPosition();
            Undo();
        }
        else HideHand();
    }

    public override void CardPlayed(Card card)
    {
        base.CardPlayed(card);
        ShowHand();
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
            actionTipTiles.Add(tile);
        }
        return actionTipTiles.Count != 0;
    }
    
    public override IEnumerator MoveCoroutine(List<Node> nodes, Action callback = null)
    {
        
        foreach (Node node in nodes)
        {
            UpdateFog(true); 
            transform.position = node.nodePosition;
            if (UpdateFog(false) && !GameController.Instance.battleMode) 
            {
                GameController.Instance.EnterBattleMode();
                if (callback != null) callback();
                else
                {
                    EndAction();
                }
                GameController.Instance.ChangeTurn();
                break;
            }
            yield return new WaitForSeconds(0.5f);
        }
        grid.GetTileByPosition(transform.position).SetOccupyingCharacter(this);
        //grid.GetTileByPosition(transform.position).setFog(false);
        isMoving = false;
        if (callback != null) callback();
        else
        {
            EndAction();
        }
        
       
    }

    public void HideTips()
    {
        foreach (Tile tile in actionTipTiles)
        {
            tile.SetActiveActionHighlight(false, true);
        }
        actionTipTiles.Clear();
    }
    #endregion

    #region Actions

    public override bool PrepareMove(int range, Card card)
    {
        if (!ShowWalkableTilesInRange(range)) return false;
        DefaultAction(card);
        return true;
    }

    public override bool PrepareAttack(int maxRange, int minRange, Card card, Vector2 startPosition)
    {
        List<Tile> enemyTiles = GetHittableEnemyTiles(maxRange, minRange, startPosition);

        foreach(Tile tile in enemyTiles) 
        {
            tile.SetActiveActionHighlight(true, false);
            actionTipTiles.Add(tile);
        }

        DefaultAction(card);

        return enemyTiles.Count > 0;
    }

    public override bool PrepareCharge(int maxRange, int minRange, Card card)
    {
        List<Tile> enemyTiles = GetHittableEnemyTiles(maxRange, minRange);
        foreach (Tile tile in enemyTiles)
        {
            if (CanCharge(tile.transform.position)) 
            {
                tile.SetActiveActionHighlight(true, false);
                actionTipTiles.Add(tile);
            }
        }
        DefaultAction(card);
        return actionTipTiles.Count > 0;
    }

    public void DefaultAction(Card card) 
    {
        CanvasCard cardObject = GetCardObject(card);
        cardObject.SetDefaultPosition();
        preparedCard = card;
    }

    public void Undo() 
    {
        HideTips();
        ShowHand();
        if (preparedCard != null)
        {
            preparedCard = null;
        }
    }

    #endregion


    private List<Tile> GetHittableEnemyTiles(int maxRange, int minRange)
    {
        return GetHittableEnemyTiles(maxRange, minRange, transform.position);
    }
    
    private List<Tile> GetHittableEnemyTiles(int maxRange, int minRange, Vector2 startPosition) 
    {
        List<Vector2> tiles = AStar.GetWalkableTilesInRange(startPosition, maxRange, grid);
        List<Tile> enemyTiles = new List<Tile>();
        foreach (Vector2 tilePosition in tiles)
        {
            Tile tileObject = grid.GetTileByPosition(tilePosition);
            if (tileObject.getOccupyingCharacter() != null)
            {
                if (Mathf.Abs(Vector2.Distance(tilePosition, transform.position)) < minRange) continue;
                if (CanHit(tilePosition, startPosition)) enemyTiles.Add(tileObject);
            }
        }
        return enemyTiles;
    }

    #region Fog
    public bool UpdateFog(bool action)
    {
        bool dicoveredEnemy = false;
        grid.GetTileByPosition(transform.position).SetFog(false);
        List<Vector2> tiles = AStar.GetTilesInRange(transform.position, stats.getActualStat(Stats.viewDistance), grid);
        Tile tile1;
        foreach (Vector2 tile in tiles)
        {
            bool isVisible = true;
            if (!action)
            {
                List<Vector2> path = AStar.findShortestPath(grid, transform.position, tile);
                foreach (Vector2 pos in path)
                {
                    if (!grid.GetTileByPosition(pos).IsWalkable()) {
                        isVisible = false;
                        break;
                    }
                }
            }
            else
            {
                isVisible = false;
            }

            if (tile != null)
            {
                tile1 = grid.GetTileByPosition(tile);
                if (tile1.getOccupyingCharacter() is SimpleAI)
                {
                    if (isVisible) 
                    {
                        GameController.Instance.AddActiveEnemy((SimpleAI)tile1.getOccupyingCharacter());
                        dicoveredEnemy = true;
                    }
                    tile1.getOccupyingCharacter().SetVisibility(isVisible);
                }
                tile1.SetFog(!isVisible);
            }

        }
        return dicoveredEnemy;
        
    }

    public void ChangeFogVisibility(bool action)
    {
        Dictionary<Vector2, Tile>  dict = grid.GetTiles();
        foreach(KeyValuePair<Vector2, Tile> tile in dict)
        {
            tile.Value.SetFog(action);
        }

    }

    #endregion
  

}
