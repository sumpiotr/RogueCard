using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCharacter : MonoBehaviour
{

    bool isMoving;
    public CharacterDataScriptableObject data;
    public CharacterStats stats;
    [SerializeField]protected int handSize;
    [SerializeField] protected DeckDataScriptableObject deckData;
    [SerializeField] protected Grid grid;

    protected  List<Card> deck = new List<Card>();
    protected  List<Card> hand = new List<Card>();
    protected  List<Card> discard = new List<Card>();
    public Vector2 destination;
    public Card preparedCard = null;

    //change in player
    public  void Initialize() 
    {
        isMoving = false;
        stats = new CharacterStats(data);
        foreach (CardDataScriptableObject startCard in deckData.startCards)
        {
            AddCardToDeck(CreateCard(startCard));
        }
    }

    public void SetGrid(Grid grid) 
    {
        this.grid = grid;
    }

    public Grid GetGrid() 
    {
        return grid;
    }


    #region Move
    public void Move(Vector2 targetPosition, int distance) 
    {
        if (isMoving) return;
        List<Node> path = AStar.findPath(grid, transform.position, targetPosition, distance);
        if (path == null) return;
        isMoving = true;
        grid.GetTileByPosition(transform.position).SetOccupyingCharacter();
        StartCoroutine("MoveCoroutine", path);
    }


    public IEnumerator MoveCoroutine(List<Node> nodes) 
    {
        foreach(Node node in nodes) 
        {
            transform.position = node.nodePosition;
            yield return new WaitForSeconds(0.5f);
        }
        grid.GetTileByPosition(transform.position).SetOccupyingCharacter(this);
        isMoving = false;
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
        foreach(Card card in hand) 
        {
            discard.Add(card);
        }
        hand.Clear();
    }

    public virtual Card DrawCard()
    {
        if (deck.Count == 0)
        {
            deck = new List<Card>(discard);
            discard.Clear();
        }
        int randomCardIndex = Random.Range(0, deck.Count - 1);
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

    public int getHandSize()
    {
        return hand.Count;
    }

    #endregion

    #region Actions
    
    public virtual bool PrepareMove(int range, Card card) 
    {
        return true;
    }

    public virtual bool PrepareAttack(int range, Card card) 
    {
        return true;
    }
    #endregion


    public bool CanHit(Vector2 position) 
    {
        /*RaycastHit2D[] hits;
        hits = Physics2D.LinecastAll(transform.position, position);
        foreach (RaycastHit2D hit in hits)
        {


            if (hit.collider != null)
            {
                Tile hittedTile = hit.collider.GetComponent<Tile>();
                if (hittedTile != null)
                {
                    if (!hittedTile.IsWalkable() || !hittedTile.IsEmpty())
                    {
                        return false;
                    }
                }
            }
        }*/

        int distance = Mathf.FloorToInt(Vector2.Distance(transform.position, position));
        List<Node> path = AStar.findPath(grid, transform.position, position, distance);
        if (path == null) return false;
        return path.Count == distance;
    }

    public void TakeDamage(int power) 
    {
        stats.setActualStat("health", stats.getActualStat("health") - power);
    }



}
