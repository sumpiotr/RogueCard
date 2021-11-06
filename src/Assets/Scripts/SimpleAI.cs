using System.Collections.Generic;
using UnityEngine;

public class SimpleAI : BaseCharacter
{

    PlayerController player;
    [SerializeField] int triggerRange;

    private void Start()
    {
        player = GameController.gameController.player;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) 
        {
            MakeAction();
        }
    }

    public void MakeAction()
    {

        List<Node> path = AStar.findPath(grid, transform.position, player.transform.position);
        if (path == null) return;
        DrawFullHand();
        int distanceFromPlayer = AStar.findPath(grid, transform.position, player.transform.position).Count;
        if (distanceFromPlayer > triggerRange) return;
        List<Card> moveCards = new List<Card>();
        List<Card> attackCards = new List<Card>();

        foreach (Card card in hand)
        {
            if (card.cardData.move >= 0)
            {
                moveCards.Add(card);
            }
            else if (card.cardData.attack >= 0)
            {
                attackCards.Add(card);
            }
        }

        bool canHit = CanHit(player.transform.position);
        if (canHit) 
        {
            int distance = Mathf.FloorToInt(Vector2.Distance(transform.position, player.transform.position));
            List<Card> attacksToRemove = new List<Card>();
            foreach (Card card in attackCards)
            {
                if (distance > card.cardData.range)
                {
                    attacksToRemove.Add(card);
                }
            }

            foreach (Card attack in attacksToRemove)
            {
                attackCards.Remove(attack);
            }
        }
        

        Card bestCard = null;
        if (attackCards.Count > 0 && canHit)
        {
            bestCard = attackCards[0];
            foreach (Card card in attackCards)
            {
                if (bestCard.cardData.attack < card.cardData.attack)
                {
                    bestCard = card;
                }
            }
        }
        else if (moveCards.Count > 0)
        {
            bestCard = moveCards[0];
            foreach (Card card in moveCards)
            {
                if (bestCard.cardData.move < card.cardData.move)
                {
                    bestCard = card;
                }
            }
           
        }

        if (bestCard != null)
        {
            bestCard.PrepareCard(this);
            bestCard.CardPlayed(this);
        }
        DiscardAllCardsFromHand();
    }


    #region Actions
    public override bool PrepareMove(int range, Card card)
    {
        List<Node> path = AStar.findPath(grid, transform.position, player.transform.position);
        if (path == null) return false;
        if (path.Count <= 1) return false;
        path.RemoveAt(path.Count - 1);
        if (path.Count <= range)
        {
            destination = path[path.Count - 1].nodePosition;
        }
        else
        {
            destination = path[range - 1].nodePosition;
        }
        return true;
    }

    public override bool PrepareAttack(int range, Card card)
    {
        destination = player.transform.position;
        return true;
    }
    #endregion
}
