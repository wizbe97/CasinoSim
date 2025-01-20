using UnityEngine;

public class PlayerBlackjack : MonoBehaviour
{
    [Header("Dealer Settings")]
    private LayerMask _blackjackTableLayerMask; // Layer for blackjack tables
    [SerializeField] private float _interactionDistance = 5f; // Max distance for interacting with tables
    private PlayerInputHandler _inputHandler;
    private PlayerInteraction _playerInteraction;
    private PlayerController _playerController;

    private bool isDealer = false;
    private Transform dealerSpot;

    private void Awake()
    {
        _inputHandler = GetComponent<PlayerInputHandler>();
        _playerInteraction = GetComponent<PlayerInteraction>();
        _playerController = GetComponent<PlayerController>();
    }

    private void Start()
    {
        _blackjackTableLayerMask = _playerInteraction.PlacementLayerMask;
        Debug.Log("BlackjackTable LayerMask: " + _blackjackTableLayerMask.value);
    }


    private void OnEnable()
    {
        _inputHandler.OnJoinTable +=  JoinTable;
        _inputHandler.OnCancel += LeaveDealerSpot;
    }

    private void OnDisable()
    {
        _inputHandler.OnJoinTable -= JoinTable;
        _inputHandler.OnCancel -= LeaveDealerSpot;
    }

    private void JoinTable()
    {
        if (isDealer)
            return;

        Ray ray = _playerInteraction.PlayerCamera.ScreenPointToRay(RectTransformUtility.WorldToScreenPoint(null, _playerInteraction.ReticleUI.position));
        Debug.DrawRay(ray.origin, ray.direction * _interactionDistance, Color.red, 2f); // Debug the ray

        RaycastHit[] hits = Physics.RaycastAll(ray, _interactionDistance);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("PlacedObjects"))
            {
                GameObject hitObject = hit.collider.gameObject;

                Transform dealerSpotTransform = null;
                foreach (Transform child in hitObject.transform)
                {
                    if (child.CompareTag("DealerSpot"))
                    {
                        dealerSpotTransform = child;
                        break;
                    }
                }

                if (dealerSpotTransform != null)
                {
                    dealerSpot = dealerSpotTransform;
                    TeleportToDealerSpot();
                }
                return;
            }
        }
    }


    private void TeleportToDealerSpot()
    {
        if (dealerSpot == null) return;

        // Teleport the player to the dealer spot
        transform.position = dealerSpot.position;
        transform.rotation = dealerSpot.rotation;

        // Set as dealer and disable PlayerInteraction
        isDealer = true;
        _playerInteraction.enabled = false;
        _playerController.CanMove = false;
        Debug.Log("Player is now the dealer.");
    }

    private void LeaveDealerSpot()
    {
        if (!isDealer) return;
        _playerController.CanMove = true;
        _playerInteraction.enabled = true;


        // Reset dealer state
        isDealer = false;

        Debug.Log("Player has left the dealer spot.");
    }
}
