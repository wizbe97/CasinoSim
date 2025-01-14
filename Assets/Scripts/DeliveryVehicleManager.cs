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

    public void SpawnDeliveryVehicle(GameObject boxPrefab)
    {
        // Instantiate the delivery vehicle
        GameObject deliveryVehicle = Instantiate(_deliveryVehiclePrefab, _startPosition, Quaternion.identity);

        // Start the movement coroutine and pass the cardboard box prefab
        StartCoroutine(MoveVehicle(deliveryVehicle, boxPrefab));
    }

    private IEnumerator MoveVehicle(GameObject vehicle, GameObject boxPrefab)
    {
        Vector3 halfwayPoint = Vector3.Lerp(_startPosition, _endPosition, 0.5f);
        bool boxSpawned = false;

        while (Vector3.Distance(vehicle.transform.position, _endPosition) > 0.1f)
        {
            vehicle.transform.position = Vector3.MoveTowards(
                vehicle.transform.position,
                _endPosition,
                _vehicleSpeed * Time.deltaTime
            );

            if (!boxSpawned && Vector3.Distance(vehicle.transform.position, halfwayPoint) < 0.5f)
            {
                SpawnCardboardBox(halfwayPoint, boxPrefab);
                boxSpawned = true;
            }

            yield return null;
        }

        Destroy(vehicle);
    }

    private void SpawnCardboardBox(Vector3 halfwayPoint, GameObject boxPrefab)
    {
        Vector3 boxSpawnPosition = new Vector3(
            halfwayPoint.x + _halfwayOffsetX,
            _boxSpawnHeight,
            halfwayPoint.z
        );

        Instantiate(boxPrefab, boxSpawnPosition, Quaternion.identity);
    }
}
