using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Kardx.UI.Scenes;

namespace Kardx.UI.Components
{
    using Kardx.Core;

    /// <summary>
    /// Specialized drag handler for deploying order cards.
    /// </summary>
    public class OrderDeployDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField]
        private float dragOffset = 0.5f;

        private CardView cardView;
        private MatchView matchView;
        private Vector3 originalPosition;
        private Transform originalParent;
        private CanvasGroup canvasGroup;
        private bool isDragging = false;

        private void Awake()
        {
            cardView = GetComponent<CardView>();
            canvasGroup = GetComponent<CanvasGroup>();
            
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            
            matchView = GetComponentInParent<MatchView>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!CanDrag())
                return;

            isDragging = true;
            originalPosition = transform.position;
            originalParent = transform.parent;

            // Disable raycast blocking so we can detect drop targets underneath
            canvasGroup.blocksRaycasts = false;
            
            // Move to front of UI
            transform.SetParent(transform.root);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging)
                return;

            // Move the card with the cursor
            transform.position = eventData.position + new Vector2(0, dragOffset);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!isDragging)
                return;

            isDragging = false;
            
            // Re-enable raycast blocking
            canvasGroup.blocksRaycasts = true;
            
            // Check if the card can be played
            if (CanPlayOrderCard())
            {
                // Play the order card
                matchView.DeployOrderCard(cardView.Card);
            }
            else
            {
                // Return to original position
                transform.SetParent(originalParent);
                transform.position = originalPosition;
            }
            
            // Clear highlights
            if (matchView != null)
            {
                matchView.ClearAllHighlights();
            }
        }

        private bool CanPlayOrderCard()
        {
            if (cardView == null || cardView.Card == null || matchView == null)
                return false;
                
            return matchView.CanDeployOrderCard(cardView.Card);
        }

        private bool CanDrag()
        {
            if (cardView == null || cardView.Card == null || matchView == null)
                return false;
                
            var card = cardView.Card;
            
            // Only order cards in player's hand can be dragged during player's turn
            return card.IsOrderCard && 
                   matchView.IsPlayerTurn() && 
                   matchView.GetCurrentPlayer().Hand.Contains(card);
        }
    }
}
