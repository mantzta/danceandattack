using UnityEngine;

public class BlockControllerSolo : MonoBehaviour
{
    public GameObject cutDirectionArrow;

    private Vector3[] cutDirectionRotations = new Vector3[]
    {
        new Vector3(0, 0, 0),      // Up
        new Vector3(0, 0, 180),    // Down
        new Vector3(0, 0, 90),     // Left
        new Vector3(0, 0, -90),    // Right
        new Vector3(0, 0, -45),    // Up-Left
        new Vector3(0, 0, -135),   // Up-Right
        new Vector3(0, 0, 135),    // Down-Left
        new Vector3(0, 0, 45)      // Down-Right
    };

    private Vector3 moveDirection = Vector3.back;
    public float moveSpeed;

    private void OnDisable()
    {
        GetComponentInChildren<Light>().enabled = false;
    }

    public void Initialize(BeatSaberBlockSpawnerSolo.BeatSaberBlockDataSolo blockData)
    {
        int cutDirection = blockData._cutDirection;

        if (cutDirection >= 0 && cutDirection < cutDirectionRotations.Length)
        {
            transform.eulerAngles = cutDirectionRotations[cutDirection];
        }
        else
        {
            // Hide the arrow for "Any" direction (cutDirection == 8)
            cutDirectionArrow.SetActive(false);
        }
    }

    private void Update()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        if (transform.position.magnitude > 50)
        {
            Destroy(gameObject);
        }
    }
}
