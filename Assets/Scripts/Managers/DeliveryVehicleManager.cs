using System.Collections;
using UnityEngine;

public class DeliveryVehicleManager : MonoBehaviour
{
    public static DeliveryVehicleManager Instance { get; private set; }

    [Header("Vehicle Settings")]
    [SerializeField] private GameObject _deliveryVehiclePrefab;
    [SerializeField] private float _vehicleSpeed = 5f; 

    [Header("Coordinates")]
    [SerializeField] private Vector3 _startPosition = new Vector3(21, -6, -21);
    [SerializeField] private Vector3 _endPosition = new Vector3(21, -6, 20);

    [Header("Box Settings")]
    [SerializeField] private float _halfwayOffsetX = -2f;
    [SerializeField] private float _boxSpawnHeight = 3f;

    private void Start()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SpawnDeliveryVehicle(GameObject boxPrefab)
    {
        GameObject deliveryVehicle = Instantiate(_deliveryVehiclePrefab, _startPosition, Quaternion.identity);

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
