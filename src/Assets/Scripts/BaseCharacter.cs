using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseCharacter : MonoBehaviour
{

    public bool isMoving;
    public bool busy = false;
    public CharacterDataScriptableObject data;
    public CharacterStats stats;
    [SerializeField] protected int handSize;
    [SerializeField] protected DeckDataScriptableObject deckData;
    [SerializeField] protected Grid grid;
    [SerializeField] Slider healthbar;


    protected List<Card> deck = new List<Card>();
    protected List<Card> hand = new List<Card>();
    protected List<Card> discard = new List<Card>();

    //variable for temporary card destinations like move dest or attack dest
    public Vector2 destination;

    //played but no confirmed card
    public Card preparedCard = null;

    
    public void Initialize()
    {
        isMoving = false;
        stats = new CharacterStats(data);
        foreach (CardDataScriptableObject startCard in deckData.startCards)
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
            EndAsyncAction();
        }
    }

    #endregion

    #region Cards

    public virtual Card CreateCard(CardDataScriptableObject cardData)
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

   
    public virtual void EndAsyncAction() 
    {
        busy = false;
    }

    public virtual bool PrepareMove(int range, Card card)
    {
        return true;
    }

    public virtual bool PrepareAttack(int range, Card card, bool fromDestination = false)
    {
        return true;
    }
    #endregion


    public bool CanHit(Vector2 position, bool fromDestination = false)
    {
        Vector2 startPosition = fromDestination ? destination : (Vector2)transform.position;
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
        hits = Physics2D.LinecastAll(startPosition, position);
        foreach (RaycastHit2D hit in hits)
        {
            //continue if hited tile is character or destination
            if ((hit.transform.position.x == startPosition.x && hit.transform.position.y == startPosition.y) || 
                (hit.transform.position.x == position.x && hit.transform.position.y == position.y) ) continue;
            Tile hittedTile = hit.collider.GetComponent<Tile>();
            if (hittedTile != null)
            {
                if ((!hittedTile.IsWalkable() || !hittedTile.IsEmpty()))
                {
                    return false;
                }
            }
        }
        return true;


        /*int distance = Mathf.FloorToInt(Vector2.Distance(transform.position, position));
        List<Node> path = AStar.findPath(grid, transform.position, position, distance);
        if (path == null) return false;
        return path.Count == distance;*/
    }

    public void TakeDamage(int power)
    {
        Debug.Log("dame taken!");
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

}
