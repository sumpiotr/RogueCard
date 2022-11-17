using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseCharacter : MonoBehaviour, ISpawnable, ICharacter
{

    public bool isMoving;
    public bool busy = false;
    public CharacterDataScriptableObject data;
    [SerializeField] protected int handSize;
    [SerializeField] protected DeckDataScriptableObject deckData;
    [SerializeField] protected Grid grid;
    [SerializeField] Slider healthbar;


    protected List<Card> deck = new List<Card>();
    protected List<Card> hand = new List<Card>();
    protected List<Card> discard = new List<Card>();


    //played but no confirmed card
    public Card preparedCard = null;

    public Vector2 position { get  { return transform.position; } set { } }

    private CharacterStats _stats;
    private Vector2 _destination;
    private ICharacter _target;

    public CharacterStats stats { get => _stats; set => _stats = value; }


    public void Initialize()
    {
        isMoving = false;
        stats = new CharacterStats(data);
        foreach (BasicCardDataScriptableObject startCard in deckData.startCards)
        {
            AddCardToDeck(CreateCard(startCard));
        }
    }

    virtual public void SetGrid(Grid grid)
    {
        this.grid = grid;
    }

    public Grid GetGrid()
    {
        return grid;
    }


    #region Move

    //Starts MoveCoroutine and prepare move
    public virtual void Move(Vector2 targetPosition, int distance, Action callback = null)
    {
        if (isMoving) return;
        List<Node> path = AStar.findPath(grid, transform.position, targetPosition, distance);
        if (path == null) return;
        isMoving = true;
        busy = true;
        grid.GetTileByPosition(transform.position).SetOccupyingCharacter();
        StartCoroutine(MoveCoroutine(path, callback));
    }


    //Moving character
    public virtual IEnumerator MoveCoroutine(List<Node> nodes, Action callback = null)
    {
        foreach (Node node in nodes)
        {
            transform.position = node.nodePosition;
            yield return new WaitForSeconds(0.5f);
        }
        grid.GetTileByPosition(transform.position).SetOccupyingCharacter(this);
        isMoving = false;
        if (callback != null) callback();
        else
        {
            EndAction();
        }
    }

    #endregion

    #region Cards

    public virtual Card CreateCard(BasicCardDataScriptableObject cardData)
    {
        Card card = CardsFactory.getCard(cardData);
        return card;
    }
    public virtual void AddCardToDeck(Card card)
    {
        deck.Add(card);
    }

    public virtual void DiscardCardFromHand(Card card)
    {
        if (!hand.Contains(card)) return;
        discard.Add(card);
        hand.Remove(card);
    }

    public virtual void DiscardAllCardsFromHand()
    {
        foreach (Card card in hand)
        {
            discard.Add(card);
        }
        hand.Clear();
    }

    public virtual Card DrawCard()
    {
        if (deck.Count == 0)
        {
            ReshuffleDeck();
        }
        int randomCardIndex = UnityEngine.Random.Range(0, deck.Count - 1);
        Card card = deck[randomCardIndex];
        hand.Add(card);
        deck.RemoveAt(randomCardIndex);
        return card;
    }

    public virtual void DrawFullHand()
    {
        for (int i = hand.Count; i < handSize; i++)
        {
            DrawCard();
        }
    }

    public virtual void ReshuffleDeck() 
    {
        foreach(Card card in discard) 
        {
            deck.Add(card);
        }
        discard.Clear();

    }

    public int getHandSize()
    {
        return hand.Count;
    }

    #endregion

    #region Actions

   
    public virtual void EndAction() 
    {
        busy = false;
    }
    
    //fires after card play
    public virtual void CardPlayed(Card card)
    {
        //if (!hand.Contains(card)) return;
        preparedCard = null;
        DiscardCardFromHand(card);
    }

    public virtual bool PrepareMove(int range, Card card)
    {
        return true;
    }

    public virtual bool PrepareAttack(int maxRange, int minRange, Card card)
    {
        return PrepareAttack(maxRange, minRange, card, transform.position);
    }
    
    public virtual bool PrepareAttack(int maxRange, int minRange, Card card, Vector2 startPosition)
    {
        return true;
    }

    public virtual bool PrepareCharge(int maxRange, int minRange, Card card) 
    {
        return true;
    }
    #endregion


    public bool CanHit(Vector2 position)
    {
        return CanHit(position, transform.position);
    }
    
    public bool CanHit(Vector2 position, Vector2 startPosition)
    {
        if (position == (Vector2)transform.position) return false;
        float distanceX = Mathf.Abs(position.x - startPosition.x);
        float distanceY = Mathf.Abs(position.y - startPosition.y);
        //checking diagonally 
        if (distanceX == distanceY)
        {
            int signX = startPosition.x < position.x ? 1 : -1;
            int signY = startPosition.y < position.y ? 1 : -1;
            for (float i = 1; i < distanceX; i++) 
            {
                float x = i * signX;
                float y = i * signY;
                if(!grid.IsTileWalkableAndEmpty(new Vector2(startPosition.x + x, startPosition.y + y))) 
                {
                    return false;
                }
            }
            return true;
        }
        //checking all other directions
        RaycastHit2D[] hits;
        hits = Physics2D.LinecastAll(startPosition, position, GameController.Instance.obstalcesLayers);
        foreach (RaycastHit2D hit in hits)
        {
            //continue if hited tile is character or destination
            if ((hit.transform.position.x == startPosition.x && hit.transform.position.y == startPosition.y) || 
                (hit.transform.position.x == position.x && hit.transform.position.y == position.y) ) continue;
            /*Tile hittedTile = hit.collider.GetComponent<Tile>();
            if (hittedTile != null)
            {
                if ((!hittedTile.IsWalkable() || !hittedTile.IsEmpty()))
                {
                    return false;
                }
            }*/
            return false;
        }
        return true;
    }

    public bool CanCharge(Vector2 position) 
    {
        Vector2 startPosition = transform.position;
        if (position.x == startPosition.x || position.y == startPosition.y) return true;
        float distanceX = Mathf.Abs(position.x - startPosition.x);
        float distanceY = Mathf.Abs(position.y - startPosition.y);
        if (distanceX == distanceY) return true;
        return false;
    }

    public void TakeDamage(int power)
    {
        stats.setActualStat(Stats.health, stats.getActualStat(Stats.health) - power);
        healthbar.value = (float)stats.getActualStat(Stats.health) / (float)stats.maxHealth;
        if (isDead())
        {
            Kill();
            if (ShouldDrop())
            {
                
            }
        }
    }

    public virtual void Kill() 
    {

    }

    public bool isDead()
    {
        return stats.getActualStat(Stats.health) <= 0;
    }

    virtual public void SetVisibility(bool action)
    {

    }

    virtual public bool ShouldDrop()
    {
        return false;
    }

    virtual public(GameObject instance, int value) Spawn(Tile tile)
    {
        GameObject newInstance = Instantiate(gameObject, GameController.Instance.gameObject.transform);
        tile.SetOccupyingCharacter(newInstance.GetComponent<BaseCharacter>());
        newInstance.transform.position =  new Vector3(tile.transform.position.x, tile.transform.position.y, -1);
        newInstance.GetComponent<BaseCharacter>().SetGrid(GameController.Instance.grid);
        return (newInstance, data.difficulty);
    }
}
