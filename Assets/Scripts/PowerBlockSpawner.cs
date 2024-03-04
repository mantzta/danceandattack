using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerBlockSpawner : MonoBehaviour
{
    public GameObject PowerupBlockPrefab;
    public GameObject AttackBlockPrefab;
    public int Id = -1;

    public PowerBlockData? SpawnPowerBlocks(bool isLeftPlayer)
    {
        if (CheckIfBothPlayersAreReady())
        {
            Vector3 powerPosition = new Vector3(1, Random.Range(1, 3), 15);
            Vector3 attackPosition = new Vector3(4.5f, Random.Range(1, 3), 15);

            if (isLeftPlayer)
            {
                powerPosition = new Vector3(-3.5f, Random.Range(1, 3), 15);
                attackPosition = new Vector3(0, Random.Range(1, 3), 15);
            }
            GameObject powerBlockInstance = Instantiate(PowerupBlockPrefab, powerPosition, Quaternion.identity);
            GameObject attackBlockInstance = Instantiate(AttackBlockPrefab, attackPosition, Quaternion.identity);

            int powerId = Id + 1;
            int attackId = Id + 2;
            Id += 2;
            InitControllers(powerBlockInstance, isLeftPlayer, powerId);
            InitControllers(attackBlockInstance, isLeftPlayer, attackId);

            return new PowerBlockData(powerPosition, attackPosition, powerId, attackId);
        }

        return null;
    }

    public void SpawnOpponentBlocks(bool isLeftPlayer, PowerBlockData data)
    {
        GameObject powerBlockInstance = Instantiate(PowerupBlockPrefab, data.PowerPosition, Quaternion.identity);
        GameObject attackBlockInstance = Instantiate(AttackBlockPrefab, data.AttackPosition, Quaternion.identity);

        InitControllers(powerBlockInstance, isLeftPlayer, data.PowerId);
        InitControllers(attackBlockInstance, isLeftPlayer, data.AttackId);
    }

    private void InitControllers(GameObject instance, bool isLeftPlayer, int id)
    {
        PowerBlockController powerBlockController = instance.GetComponent<PowerBlockController>();
        Vector3 moveDirection = new Vector3(0, 0, -1);

        powerBlockController.Initialize(moveDirection, isLeftPlayer, id);
    }

    private bool CheckIfBothPlayersAreReady()
    {
        return FindObjectOfType<StartGameForBoth>().CheckIfAllPlayersReady();
    }
}

public class PowerBlockData {
    public Vector3 PowerPosition { get; set; }
    public Vector3 AttackPosition { get; set; }
    public int PowerId { get; set; }
    public int AttackId { get; set; }

    public PowerBlockData(Vector3 powerPosition, Vector3 attackPosition, int powerId, int attackId)
    {
        PowerPosition = powerPosition;
        AttackPosition = attackPosition;
        PowerId = powerId;
        AttackId = attackId;
    }
}
