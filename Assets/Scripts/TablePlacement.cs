using UnityEngine;

public class TablePlacement : MonoBehaviour
{
    public GameObject tablePrefab; // The actual table prefab
    public GameObject tablePreviewPrefab; // The translucent preview prefab
    public RectTransform reticleUI; // The UI reticle (RectTransform)
    public Camera playerCamera; // The player's camera
    public Transform playerTransform; // The player's transform (to calculate distance)

    private GameObject currentPreview; // Instance of the table preview
    private GameObject pickedUpTable; // Reference to the currently picked-up table
    private bool isPlacingTable = false; // Whether the player is in placement mode
    private bool canPlace = false; // Whether the current position is valid for placement

    public LayerMask placementLayerMask; // Layer mask for valid placement surfaces
    public LayerMask collisionLayerMask; // Layer mask for collision checks
    public LayerMask placedTableLayerMask; // Layer mask for placed tables
    public float maxPlacementDistance = 10f; // Maximum allowable distance for placement
    public float placementCooldown = 1.0f; // Cooldown time in seconds for recently placed tables

    void Start()
    {
        // Initialize all existing tables in the scene
        InitializeExistingTables();
    }

    void Update()
    {
        // Handle starting placement mode
        if (Input.GetKeyDown(KeyCode.B) && !isPlacingTable)
        {
            StartPlacingTable();
        }

        // Handle picking up a table
        if (Input.GetKeyDown(KeyCode.F))
        {
            TryPickUpTable();
        }

        // Handle table preview updates and placement
        if (isPlacingTable)
        {
            UpdateTablePreview();
            HandleRotation();

            if (Input.GetMouseButtonDown(0) && canPlace)
            {
                PlaceTable();
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CancelPlacement();
            }
        }
    }

    void StartPlacingTable()
    {
        isPlacingTable = true;

        if (pickedUpTable != null)
        {
            // Move the picked-up table into preview mode
            currentPreview = Instantiate(tablePreviewPrefab, pickedUpTable.transform.position, pickedUpTable.transform.rotation);
            Destroy(pickedUpTable); // Ensure the original table is destroyed
            pickedUpTable = null;
        }
        else
        {
            // Start a new placement
            currentPreview = Instantiate(tablePreviewPrefab);
        }
    }

    void TryPickUpTable()
    {
        Ray ray = playerCamera.ScreenPointToRay(RectTransformUtility.WorldToScreenPoint(null, reticleUI.position));

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, placedTableLayerMask))
        {
            if (hit.collider != null && hit.collider.gameObject != currentPreview)
            {
                // Check if the table can be picked up
                PlacedTable table = hit.collider.GetComponent<PlacedTable>();
                if (table != null && table.CanBePickedUp())
                {
                    pickedUpTable = hit.collider.gameObject; // Store the table reference
                    StartPlacingTable(); // Move to preview mode
                }
            }
        }
    }

    void UpdateTablePreview()
    {
        Vector2 screenPosition = RectTransformUtility.WorldToScreenPoint(null, reticleUI.position);
        Ray ray = playerCamera.ScreenPointToRay(screenPosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, placementLayerMask))
        {
            Vector3 newPosition = hit.point;
            float gridSize = 1.0f;
            newPosition.x = Mathf.Round(newPosition.x / gridSize) * gridSize;
            newPosition.z = Mathf.Round(newPosition.z / gridSize) * gridSize;
            newPosition.y = hit.point.y;

            Vector3 directionFromPlayer = newPosition - playerTransform.position;
            if (directionFromPlayer.magnitude > maxPlacementDistance)
            {
                directionFromPlayer = directionFromPlayer.normalized * maxPlacementDistance;
                newPosition = playerTransform.position + directionFromPlayer;

                if (Physics.Raycast(newPosition + Vector3.up * 5f, Vector3.down, out RaycastHit clampHit, Mathf.Infinity, placementLayerMask))
                {
                    newPosition.y = clampHit.point.y;
                }
            }

            currentPreview.transform.position = newPosition;

            BoxCollider collider = currentPreview.GetComponent<BoxCollider>();
            Collider[] colliders = Physics.OverlapBox(
                newPosition,
                collider.size / 2,
                currentPreview.transform.rotation,
                collisionLayerMask
            );

            canPlace = colliders.Length == 0;
            UpdatePreviewMaterial(canPlace);
        }
        else
        {
            canPlace = false;
            UpdatePreviewMaterial(false);
        }
    }

    void UpdatePreviewMaterial(bool isValid)
    {
        Color color = isValid ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);
        Renderer[] renderers = currentPreview.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            renderer.material.color = color;
        }
    }

    void HandleRotation()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentPreview.transform.Rotate(0, 22.5f, 0);
        }
    }

    void PlaceTable()
    {
        if (canPlace)
        {
            GameObject placedTable = Instantiate(tablePrefab, currentPreview.transform.position, currentPreview.transform.rotation);
            PlacedTable tableScript = placedTable.GetComponent<PlacedTable>();
            if (tableScript != null)
                tableScript.SetPickupCooldown(placementCooldown);

            CancelPlacement();
        }
    }

    void CancelPlacement()
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }
        isPlacingTable = false;
    }

    void InitializeExistingTables()
    {
        GameObject[] placedTables = GameObject.FindGameObjectsWithTag("PlacedTable");
        foreach (GameObject table in placedTables)
        {
            PlacedTable tableScript = table.GetComponent<PlacedTable>();
            tableScript.EnablePickup();
        }
    }
}
