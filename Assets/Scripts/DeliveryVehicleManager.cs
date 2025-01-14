using System.Collections;
using UnityEngine;

public class DeliveryVehicleManager : MonoBehaviour
{
    [Header("Vehicle Settings")]
    [SerializeField] private GameObject _deliveryVehiclePrefab; // The vehicle prefab
    [SerializeField] private GameObject _cardboardBoxPrefab;    // The box prefab
    [SerializeField] private float _vehicleSpeed = 5f;          // Speed of the vehicle

    [Header("Coordinates")]
    [SerializeField] private Vector3 _startPosition = new Vector3(21, -6, -21);
    [SerializeField] private Vector3 _endPosition = new Vector3(21, -6, 20);

    [Header("Box Settings")]
    [SerializeField] private float _halfwayOffsetX = 17f;
    [SerializeField] private float _boxSpawnHeight = 1f;

    /// <summary>
    /// Spawns a delivery vehicle and starts its movement process.
    /// </summary>
    public void SpawnDeliveryVehicle()
    {
        // Instantiate the delivery vehicle
        GameObject deliveryVehicle = Instantiate(_deliveryVehiclePrefab, _startPosition, Quaternion.identity);

        // Start the movement coroutine
        StartCoroutine(MoveVehicle(deliveryVehicle));
    }

    /// <summary>
    /// Moves the delivery vehicle and handles the box spawn and destruction logic.
    /// </summary>
    private IEnumerator MoveVehicle(GameObject vehicle)
    {
        Vector3 halfwayPoint = Vector3.Lerp(_startPosition, _endPosition, 0.5f);
        bool boxSpawned = false;

        while (Vector3.Distance(vehicle.transform.position, _endPosition) > 0.1f)
        {
            // Move the vehicle toward the end position
            vehicle.transform.position = Vector3.MoveTowards(
                vehicle.transform.position,
                _endPosition,
                _vehicleSpeed * Time.deltaTime
            );

            // Check if the vehicle reaches the halfway point
            if (!boxSpawned && Vector3.Distance(vehicle.transform.position, halfwayPoint) < 0.5f)
            {
                SpawnCardboardBox(halfwayPoint);
                boxSpawned = true; // Ensure the box spawns only once
            }

            yield return null; // Wait for the next frame
        }

        // Destroy the vehicle once it reaches the end position
        Destroy(vehicle);
    }

    /// <summary>
    /// Spawns the cardboard box at the halfway point with an offset.
    /// </summary>
    private void SpawnCardboardBox(Vector3 halfwayPoint)
    {
        Vector3 boxSpawnPosition = new Vector3(
            halfwayPoint.x + _halfwayOffsetX, // Apply X offset
            _boxSpawnHeight,                 // Apply Y height
            halfwayPoint.z                   // Keep Z coordinate the same
        );

        Instantiate(_cardboardBoxPrefab, boxSpawnPosition, Quaternion.identity);
    }
}
