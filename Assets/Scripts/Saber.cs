using EzySlice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using static AssignColor;
using System;

public class Saber : MonoBehaviourPunCallbacks
{
    public Saber OtherSaber;
    public ColorType SaberColor;
    public string sliceTag;
    public string powerTag;
    public string attackTag;
    public Transform targetTransform; // targetTransform of the tip of the saber
    public Vector3 upAxis = Vector3.up;
    public float cutSpeedMultiplier = 0.1f;
    public float cutForce = 1000;
    public string whichSaber;

    public ParticleSystem slashParticles;

    public Material crossSectionMaterial; // the red/blue light inside the cube when it gets sliced

    public PhotonView ParentPhotonView;

    public PowerUpType PowerUpType;
    public AttackType AttackType;

    private Vector3 previousPosition;
    private Vector3 speed;
    private Vector3 perpendicularVector;
    private Vector3 up;

    private GameObject toBeDestroyed;

    private Rigidbody rigidBody;
    private Vector3 previousPositionFixedUpdate;
    private Vector3 velocity;
    private Vector3 previousVelocity;

    private ScoreManager scoreManager;
    private bool isPlayerLeft;

    private int lastOpponentBlockId = -1;
    private int lastOpponentPowerBlockId = -1;

    public int score = 10;
    private int powerFactor = 1;
    private int attackFactor = 1;
    private int factorOnBeingAttacked = 1;
    private bool updatePowerBlocksScore = false;
    private bool updateAttackBlocksScore = false;

    public bool allowAllDirectionsHit = false;
    private bool updatePowerBlocksNoDirections = false;
    private bool updatePowerBlocksMultiDirections = false;
    private bool updateAttackBlocksRotate = false;

    public bool allowMultiDirectionsHit = false;

    private bool updatePowerBlocksCombi = false;
    private bool updateAttackBlocksCombi = false;

    private bool isPlayerAttackingOpponent = false;
    private bool hasPlayerPowerUpActivated = false;

    public AudioSource audioSource;
    public AudioSource audioSourceFail;
    public AudioSource audioSourceAttack;
    public AudioSource audioSourcePowerUp;

    private enum BlockSides {
        Down,          // 0 Down
        Up,            // 1 Up
        Right,         // 2 Right
        Left,          // 3 Left
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (ParentPhotonView.IsMine && collision.gameObject.tag == sliceTag)
        {
            // only slice the right objects
            Slice(collision.collider, transform.position, perpendicularVector, false, false);

            var cube = collision.gameObject;
            BlockController blockController = cube.GetComponentInParent<BlockController>();

            string material = crossSectionMaterial.name;
            string hitDataString = SerializeHitData(blockController.Id, transform.position, perpendicularVector, material, false);
            
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "HitBlock", hitDataString } });

            int cutDirection = cube.GetComponentInParent<BlockController>().cutDirection;
            var assignColor = cube.GetComponent<AssignColor>();

            bool hitCorrectSide = HitCorrectSide(collision, cutDirection);

            if (assignColor != null)
            {
                if (allowAllDirectionsHit || (allowMultiDirectionsHit && assignColor.color == SaberColor) || (assignColor.color == SaberColor && hitCorrectSide))
                {
                    audioSource.Play();
                    AddScore(blockController);
                } else
                {
                    Debug.Log("Hit was a miss allowAllDirectionsHit: " + allowAllDirectionsHit);
                    audioSourceFail.Play();
                    LoseScore(blockController);
                }
               
            }
        }

        if (ParentPhotonView.IsMine && collision.gameObject.tag == powerTag)
        {
            PowerBlockController powerBlockController = collision.gameObject.GetComponentInParent<PowerBlockController>();
            string hitDataString = SerializeHitData(powerBlockController.Id, transform.position, perpendicularVector, "Red Emissive", true);
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "HitBlock", hitDataString } });

            Slice(collision.collider, transform.position, perpendicularVector, true, false);

            ActivatePowerUp(PowerUpType);

            audioSourcePowerUp.Play();
        }

        if (ParentPhotonView.IsMine && collision.gameObject.tag == attackTag)
        {
            PowerBlockController powerBlockController = collision.gameObject.GetComponentInParent<PowerBlockController>();
            string hitDataString = SerializeHitData(powerBlockController.Id, transform.position, perpendicularVector, "Blue Emissive", true);
            PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "HitBlock", hitDataString } });

            Slice(collision.collider, transform.position, perpendicularVector, true, false);

            ActivateAttack(AttackType);

            audioSourceAttack.Play();
        }

    }

    private void ActivatePowerUp(PowerUpType power)
    {
        switch(power)
        {
            case PowerUpType.Score:
                StartCoroutine(PowerUpScore());
                break;
            case PowerUpType.NoDirections:
                StartCoroutine(PowerUpNoDirections());
                break;
            case PowerUpType.MultiDirections:
                StartCoroutine(PowerUpMultiDirections());
                break;
            case PowerUpType.Combi:
                StartCoroutine(PowerUpCombi());
                break;
            default:
                break;
        }
    }

    private void ActivateAttack(AttackType attack)
    {
        switch (attack)
        {
            case AttackType.Score:
                StartCoroutine(AttackScore());
                break;
            case AttackType.Rotate:
                StartCoroutine(AttackRotate());
                break;
            case AttackType.Combi:
                StartCoroutine(AttackCombi());
                break;
            default:
                break;
        }
    }

    private void StopPowerUp(PowerUpType power)
    {
        switch (power)
        {
            case PowerUpType.Score:
                powerFactor--;
                if (powerFactor == 1)
                {
                    updatePowerBlocksScore = false;
                }
                break;
            case PowerUpType.NoDirections:
                OtherSaber.allowAllDirectionsHit = false;
                allowAllDirectionsHit = false;
                updatePowerBlocksNoDirections = false;
                break;
            case PowerUpType.MultiDirections:
                allowMultiDirectionsHit = false;
                updatePowerBlocksMultiDirections = false;
                break;
            case PowerUpType.Combi:
                OtherSaber.allowAllDirectionsHit = false;
                allowAllDirectionsHit = false;
                powerFactor /= 2;
                if (powerFactor == 1)
                {
                    updatePowerBlocksCombi = false;
                }
                break;
            default:
                break;
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "ActivatePowerUp", false } });
        hasPlayerPowerUpActivated = false;
    }

    private void StopAttack(AttackType attack)
    {
        switch (attack)
        {
            case AttackType.Score:
                attackFactor--;
                if (attackFactor == 1)
                {
                    updateAttackBlocksScore = false;
                }
                break;
            case AttackType.Rotate:
                updateAttackBlocksRotate = false;
                break;
            case AttackType.Combi:
                attackFactor /= 2;
                if (attackFactor == 1)
                {
                    updateAttackBlocksCombi = false;
                }
                break;
            default:
                break;
        }

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "ActivateAttack", false } });
        isPlayerAttackingOpponent = false;
    }

    private IEnumerator PowerUpScore()
    {
        hasPlayerPowerUpActivated = true;
        powerFactor++;
        updatePowerBlocksScore = true;
        var powerData = new PowerUpData(true, powerFactor, PowerUpType.Score);
        var dataString = PowerUpData.Serialize(powerData);
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "ActivatePowerUp", dataString } });

        yield return new WaitForSeconds(10);

        StopPowerUp(PowerUpType.Score);
    }

    private IEnumerator AttackScore()
    {
        isPlayerAttackingOpponent = true;
        attackFactor++;
        updateAttackBlocksScore = true;
        var attackData = new AttackData(true, attackFactor, AttackType.Score);
        var dataString = AttackData.Serialize(attackData);
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "ActivateAttack", dataString } });

        yield return new WaitForSeconds(10);

        StopAttack(AttackType.Score);
    }

    private IEnumerator PowerUpMultiDirections()
    {
        hasPlayerPowerUpActivated = true;
        allowMultiDirectionsHit = true;
        updatePowerBlocksMultiDirections = true;
        var powerData = new PowerUpData(true, 1, PowerUpType.MultiDirections); // Factor is 1 -> means no adjustments to score
        var dataString = PowerUpData.Serialize(powerData);
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "ActivatePowerUp", dataString } });

        yield return new WaitForSeconds(10);

        StopPowerUp(PowerUpType.MultiDirections);
    }

    private IEnumerator PowerUpNoDirections()
    {
        hasPlayerPowerUpActivated = true;
        allowAllDirectionsHit = true;
        OtherSaber.allowAllDirectionsHit = true;
        updatePowerBlocksNoDirections = true;
        var powerData = new PowerUpData(true, 1, PowerUpType.NoDirections); // Factor is 1 -> means no adjustments to score
        var dataString = PowerUpData.Serialize(powerData);
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "ActivatePowerUp", dataString } });

        yield return new WaitForSeconds(10);

        StopPowerUp(PowerUpType.NoDirections);
    }

    private IEnumerator PowerUpCombi()
    {
        hasPlayerPowerUpActivated = true;
        powerFactor *= 2;
        allowAllDirectionsHit = true;
        OtherSaber.allowAllDirectionsHit = true;
        updatePowerBlocksCombi = true;
        var powerData = new PowerUpData(true, powerFactor, PowerUpType.Combi);
        var dataString = PowerUpData.Serialize(powerData);
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "ActivatePowerUp", dataString } });

        yield return new WaitForSeconds(20);

        StopPowerUp(PowerUpType.Combi);
    }

    private IEnumerator AttackRotate()
    {
        isPlayerAttackingOpponent = true;
        updateAttackBlocksRotate = true;
        var attackData = new AttackData(true, 1, AttackType.Rotate);
        var dataString = AttackData.Serialize(attackData);
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "ActivateAttack", dataString } });

        yield return new WaitForSeconds(10);

        StopAttack(AttackType.Rotate);
    }

    private IEnumerator AttackCombi()
    {
        isPlayerAttackingOpponent = true;
        attackFactor *= 2;
        updateAttackBlocksCombi = true;
        var attackData = new AttackData(true, attackFactor, AttackType.Combi);
        var dataString = AttackData.Serialize(attackData);
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "ActivateAttack", dataString } });

        yield return new WaitForSeconds(20);

        StopAttack(AttackType.Score);
    }

    private void UpdateAllBlocks(UpdateBlocksData? dataPower, UpdateBlocksData? dataAttack)
    {
        BlockController[] blockControllers = FindObjectsOfType<BlockController>();
        // Loop through the found block controllers
        int length = blockControllers.Length;
        for (int i = 0; i < length; i++)
        {

            var controller = blockControllers[i];

            if (dataPower != null && ShouldBlockBeUpdated(dataPower.IsLeftSide, controller))
            {
                switch (dataPower.PowerUpType)
                {
                    case PowerUpType.Score:
                        if (dataPower.Size != null && dataPower.Factor != null)
                        {
                            controller.ChangeSize((int) dataPower.Size, (int) dataPower.Factor);
                        }
                        break;
                    case PowerUpType.NoDirections:
                        controller.RemoveArrow();
                        break;
                    case PowerUpType.MultiDirections:
                        controller.ShowAllDirections();
                        break;
                    case PowerUpType.Combi:
                        if (dataPower.Size != null && dataPower.Factor != null)
                        {
                            controller.RemoveArrow();
                            controller.ChangeSize((int)dataPower.Size, (int)dataPower.Factor);
                        }
                        
                        break;
                    default:
                        break;
                }
            }

            if (dataAttack != null && ShouldBlockBeUpdated(dataAttack.IsLeftSide, controller))
            {
                switch (dataAttack.AttackType)
                {
                    case AttackType.Score:
                        if (dataAttack.Size != null && dataAttack.Factor != null)
                        {
                            controller.ChangeSize((int)dataAttack.Size, (int)dataAttack.Factor);
                        }
                        break;
                    case AttackType.Rotate:
                        controller.RotateBlock();
                        break;
                    case AttackType.Combi:
                        if (dataAttack.Size != null && dataAttack.Factor != null)
                        {
                            controller.ChangeSize((int)dataAttack.Size, (int)dataAttack.Factor);
                            controller.RotateBlock();
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private bool ShouldBlockBeUpdated(bool changeLeftSide, BlockController controller)
    {
        if ((changeLeftSide && controller.isLeftPlayerCube) || (!changeLeftSide && !controller.isLeftPlayerCube))
        {
            return true;
        }

        return false;
    }

    private bool HitCorrectSide(Collision collision, int cutDirection)
    {
        if (cutDirection >= Enum.GetNames(typeof(BlockSides)).Length)
        {
            return true;
        }
        // Get the collision normal
        Vector3 collisionNormal = collision.contacts[0].normal;

        // Another way to determine the hit angle is to look at collisionNormal.X: close to 1 (highest value from x, y, z) it means it was hit on the right, close to -1 it means it was hit on the left.
        // If Y is the highest value and it was close to 1, it means it was hit on the top, close to -1 it was hit on the bottom
        // Calculate the dot products
        // float dotX = Vector3.Dot(collisionNormal, Vector3.right);
        // float dotY = Vector3.Dot(collisionNormal, Vector3.up);
        // float dotZ = Vector3.Dot(collisionNormal, Vector3.forward);

        if (Mathf.Abs(collisionNormal.x) > Mathf.Abs(collisionNormal.y) && Mathf.Abs(collisionNormal.x) > Mathf.Abs(collisionNormal.z))
        {
            if (collisionNormal.x > 0 && cutDirection == (int)BlockSides.Right)
            {
                // Right side of the block was hit
                // Debug.Log("SUCCESS Right was hit: " + cutDirection);
                return true;
            }
            else if (collisionNormal.x < 0 && cutDirection == (int)BlockSides.Left)
            {
                // Left side of the block was hit
                // Debug.Log("SUCCESS Left was hit: " + cutDirection);
                return true;
            } else if (cutDirection == (int)BlockSides.Up || cutDirection == (int)BlockSides.Down)
            {
                return true; // Make it less sensitive for the false negatives
            }
            else
            {
                Debug.Log("WRONG HIT: " + collisionNormal + " absolute X value should be highest, cut direction should be right? " + (cutDirection == (int)BlockSides.Right) + " cut direction should be left? " + (cutDirection == (int)BlockSides.Left) + " " + cutDirection);
                return false;
            }
        }
        else if (Mathf.Abs(collisionNormal.y) > Mathf.Abs(collisionNormal.x) && Mathf.Abs(collisionNormal.y) > Mathf.Abs(collisionNormal.z))
        {
            if (collisionNormal.y > 0 && cutDirection == (int)BlockSides.Up)
            {
                // Top side of the block was hit
                // Debug.Log("SUCCESS Top was hit: " + cutDirection);
                return true;
            }
            else if (collisionNormal.y < 0 && cutDirection == (int)BlockSides.Down)
            {
                // Bottom side of the block was hit
                // Debug.Log("SUCCESS Bottom was hit: " + cutDirection);
                return true;
            }
            else if (cutDirection == (int)BlockSides.Right || cutDirection == (int)BlockSides.Left)
            {
                return true; // Make it less sensitive for the false negatives
            }
            else
            {
                Debug.Log("WRONG HIT: " + collisionNormal + " absolute y value should be highest, cut direction should be up? " + (cutDirection == (int)BlockSides.Up) + " cut direction should be down? " + (cutDirection == (int)BlockSides.Down) + " " + cutDirection);
                return false;
            }
        }
        else
        {
            Debug.Log("Probably hit in the front: " + collisionNormal);
            return true; // If false, it would be too strict
        }
    }

    private void AddScore(BlockController controller)
    {
        // Add score points to the player's score
        if (scoreManager != null)
        {
            float forceMagnitude = GetAcceleration();
            var forceValue = ((int)forceMagnitude / 100) * 10;
            var calculatedScore = (((score + forceValue) * powerFactor) / factorOnBeingAttacked);
            scoreManager.AddScore( calculatedScore, isPlayerLeft);
            Debug.Log("Score: " + calculatedScore + " power: " + powerFactor + " attack: " + factorOnBeingAttacked + " force value: " + forceValue + " force magnitude: " + forceMagnitude);

            controller.ShowScore(calculatedScore);
        }
    }

    private void LoseScore(BlockController controller)
    {
        if (scoreManager != null)
        {
            float forceMagnitude = GetAcceleration();
            var forceValue = ((int)forceMagnitude / 100) * 10;
            var calculatedScore = -10 - forceValue;
            scoreManager.AddScore(calculatedScore, isPlayerLeft);

            controller.ShowScore(calculatedScore);
        }
    }

    public float GetAcceleration()
    {
        // Calculate the velocity change (∆v) between frames
        Vector3 deltaVelocity = velocity - previousVelocity;

        // Calculate the time interval (∆t) between frames
        float deltaTime = Time.fixedDeltaTime;

        // Calculate the force (F) based on Newton's second law
        Vector3 force = (rigidBody.mass * deltaVelocity) / deltaTime;

        return force.magnitude;
    }


    void Start()
    {
        lastOpponentBlockId = -1;
        lastOpponentPowerBlockId = -1;

        score = 10;
        powerFactor = 1;
        attackFactor = 1;
        factorOnBeingAttacked = 1;
        updatePowerBlocksScore = false;
        updateAttackBlocksScore = false;

        allowAllDirectionsHit = false;
        allowMultiDirectionsHit = false;
        updateAttackBlocksRotate = false;
        updateAttackBlocksCombi = false;
        updatePowerBlocksCombi = false;
        updatePowerBlocksMultiDirections = false;
        updatePowerBlocksNoDirections = false;

        isPlayerAttackingOpponent = false;
        hasPlayerPowerUpActivated = false;

        scoreManager = FindObjectOfType<ScoreManager>();
        isPlayerLeft = CheckPlayer.IsPlayerLeft();
        rigidBody = GetComponent<Rigidbody>();

        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "ActivatePowerUp", false } });
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "ActivateAttack", false } });
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "HitBlock", null } });

        if (targetTransform != null)
        {
            previousPositionFixedUpdate = targetTransform.position;
            previousPosition = targetTransform.position;
        }
    }

    private void FixedUpdate()
    {
        // calculate the displacement since the last frame
        Vector3 displacement = targetTransform.position - previousPositionFixedUpdate;

        previousVelocity = velocity;
        // calculate velocity based on displacement and time
        velocity = displacement / Time.fixedDeltaTime;

        // update the previous position
        previousPositionFixedUpdate = targetTransform.position;
    }

    void Update()
    {
        if (targetTransform != null)
        {
            // Calculate the speed of the target Transform
            speed = (targetTransform.position - previousPosition) / Time.deltaTime; // deltaTime = seconds between frames
            previousPosition = targetTransform.position;

            up = transform.TransformVector(upAxis);

            // Calculate a new vector perpendicular to the speed and upAxis
            perpendicularVector = Vector3.Cross(speed, up).normalized; // this is the direction where the two slices of the cube go after being sliced (so they don't just continue to move in the direction of the cube)
        }

        UpdateBlocksAccordingToData();

        if (PhotonNetwork.PlayerList.Length > 1)
        {
            CheckHitOpponentBlock();
            CheckOpponentUpdateBlocks();
        }
    }

    private void UpdateBlocksAccordingToData()
    {
        var isLeft = CheckPlayer.IsPlayerLeft();
        UpdateBlocksData dataPower = null;
        UpdateBlocksData dataAttack = null;

        if (updatePowerBlocksScore)
        {
            dataPower = new UpdateBlocksData()
            {
                PowerUpType = PowerUpType.Score,
                IsLeftSide = isLeft,
                Size = 1,
                Factor = powerFactor
            };
        }

        if (updateAttackBlocksScore)
        {
            dataAttack = new UpdateBlocksData()
            {
                AttackType = AttackType.Score,
                IsLeftSide = !isLeft,
                Size = -1,
                Factor = attackFactor
            };
        }

        if (updatePowerBlocksMultiDirections)
        {
            dataPower = new UpdateBlocksData()
            {
                PowerUpType = PowerUpType.MultiDirections,
                IsLeftSide = isLeft
            };
        }

        if (updatePowerBlocksNoDirections)
        {
            dataPower = new UpdateBlocksData()
            {
                PowerUpType = PowerUpType.NoDirections,
                IsLeftSide = isLeft
            };
        }

        if (updateAttackBlocksRotate)
        {
            dataAttack = new UpdateBlocksData()
            {
                AttackType = AttackType.Rotate,
                IsLeftSide = !isLeft
            };
        }

        if (updatePowerBlocksCombi)
        {
            dataPower = new UpdateBlocksData()
            {
                PowerUpType = PowerUpType.Combi,
                IsLeftSide = isLeft,
                Size = 1,
                Factor = powerFactor
            };
        }

        if (updateAttackBlocksCombi)
        {
            dataAttack = new UpdateBlocksData()
            {
                AttackType = AttackType.Combi,
                IsLeftSide = !isLeft,
                Size = -1,
                Factor = attackFactor
            };
        }

        if (dataPower != null || dataAttack != null)
        {
            UpdateAllBlocks(dataPower, dataAttack);
        }
    }

    private void CheckOpponentUpdateBlocks()
    {
        var isPlayerLeft = CheckPlayer.IsPlayerLeft();
        var opponent = PhotonNetwork.PlayerList[isPlayerLeft ? 1 : 0];
        ExitGames.Client.Photon.Hashtable props = opponent.CustomProperties;

        UpdateBlocksData dataPower = null;
        UpdateBlocksData dataAttack = null;

        if (props.ContainsKey("ActivatePowerUp") && props["ActivatePowerUp"] is string)
        {
            PowerUpData data = PowerUpData.Deserialize((string)props["ActivatePowerUp"]);

            dataPower = new UpdateBlocksData()
            {
                PowerUpType = data.PowerUpType,
                IsLeftSide = !isPlayerLeft,
                Size = (data.PowerUpType == PowerUpType.Score || data.PowerUpType == PowerUpType.Combi) ? 1 : null,
                Factor = (data.PowerUpType == PowerUpType.Score || data.PowerUpType == PowerUpType.Combi) ? data.Factor : null,
            };

            // A player cannot have a power up at the same time as being attacked, so an attack can be stopped when the opponent activates a power up
            if (isPlayerAttackingOpponent)
            {
                // So I am attacking the opponent but they send a power up so that means I have to stop my attack
                attackFactor = 2;
                StopAttack(AttackType);
            }

        }

        if (props.ContainsKey("ActivateAttack") && props["ActivateAttack"] is string)
        {
            AttackData data = AttackData.Deserialize((string)props["ActivateAttack"]);

            dataAttack = new UpdateBlocksData()
            {
                AttackType = data.AttackType,
                IsLeftSide = isPlayerLeft,
                Size = (data.AttackType == AttackType.Score || data.AttackType == AttackType.Combi) ? -1 : null,
                Factor = (data.AttackType == AttackType.Score || data.AttackType == AttackType.Combi) ? data.Factor : null
            };

            factorOnBeingAttacked = (data.AttackType == AttackType.Score || data.AttackType == AttackType.Combi) ? data.Factor : 1;

            // A player cannot have a power up when they are attacked, so the power up gets deactivated when an opponent attacks 
            if (hasPlayerPowerUpActivated)
            {
                powerFactor = 2;
                StopPowerUp(PowerUpType);
            }
        }
        else
        {
            factorOnBeingAttacked = 1;
        }

        if (dataPower != null || dataAttack != null)
        {
            UpdateAllBlocks(dataPower, dataAttack);
        }
    }

    private void CheckHitOpponentBlock()
    {
        var isPlayerLeft = CheckPlayer.IsPlayerLeft();
        var opponent = PhotonNetwork.PlayerList[isPlayerLeft ? 1 : 0];
        ExitGames.Client.Photon.Hashtable props = opponent.CustomProperties;
        if (props.ContainsKey("HitBlock") && props["HitBlock"] is string)
        {
            HitData hitData = DeserializeHitData((string) props["HitBlock"]);
            if (!hitData.IsPowerBlock)
            {
                if (hitData.Id != lastOpponentBlockId && crossSectionMaterial.name == hitData.SaberMaterial)
                {
                    lastOpponentBlockId = hitData.Id;
                    FindBlockAndDestroy(hitData, isPlayerLeft);
                }
                else if (hitData.Id != lastOpponentBlockId && crossSectionMaterial.name != hitData.SaberMaterial)
                {
                    lastOpponentBlockId = hitData.Id;
                }
            } else
            {
                if (hitData.Id != lastOpponentPowerBlockId && crossSectionMaterial.name == hitData.SaberMaterial)
                {
                    lastOpponentPowerBlockId = hitData.Id;
                    FindPowerBlockAndDestroy(hitData, isPlayerLeft);
                }
                else if (hitData.Id != lastOpponentPowerBlockId && crossSectionMaterial.name != hitData.SaberMaterial)
                {
                    lastOpponentPowerBlockId = hitData.Id;
                }
            }
        }
    }

    private void FindBlockAndDestroy(HitData hitData, bool isPlayerLeft)
    {
        // Find all objects with the BlockController component
        BlockController[] blockControllers = FindObjectsOfType<BlockController>();

        // Loop through the found block controllers
        foreach (BlockController blockController in blockControllers)
        {
            // Check if the ID matches the target ID
            if (blockController.Id == hitData.Id && blockController.isLeftPlayerCube == !isPlayerLeft)
            {
                // Found the block with the specified ID
                Collider collider = blockController.gameObject.GetComponentInChildren<Collider>();

                if (collider != null)
                {
                    bool success = Slice(collider, hitData.Position, hitData.Direction, hitData.IsPowerBlock, true);
                    if(!success)
                    {
                        blockController.gameObject.GetComponentInParent<BlockController>().enabled = false;
                        blockController.gameObject.SetActive(false);
                        Destroy(blockController.gameObject, 2);
                        Debug.Log("Alternative destroy block" + hitData.Id);
                    }
                }
                break;
            }
        }
    }

    private void FindPowerBlockAndDestroy(HitData hitData, bool isPlayerLeft)
    {
        // Find all objects with the BlockController component
        PowerBlockController[] powerBlockControllers = FindObjectsOfType<PowerBlockController>();

        // Loop through the found block controllers
        foreach (PowerBlockController powerBlockController in powerBlockControllers)
        {
            // Check if the ID matches the target ID
            if (powerBlockController.Id == hitData.Id && powerBlockController.isLeftPlayerCube == !isPlayerLeft)
            {
                Collider collider = powerBlockController.gameObject.GetComponentInChildren<Collider>();
                if (collider != null)
                {
                    bool success = Slice(collider, hitData.Position, hitData.Direction, hitData.IsPowerBlock, true);
                    if (!success)
                    {
                        powerBlockController.gameObject.GetComponentInParent<PowerBlockController>().enabled = false;
                        powerBlockController.gameObject.SetActive(false);
                        Destroy(powerBlockController.gameObject, 2);
                        Debug.Log("Alternative destroy power block" + hitData.Id);
                    }
                }
                break;
            }
        }
    }

    private string SerializeHitData(int id, Vector3 position, Vector3 direction, string material, bool isPowerBlock)
    {
        return $"{id}_{position.x}_{position.y}_{position.z}_{direction.x}_{direction.x}_{direction.x}_{material}_{isPowerBlock}";
    }

    private HitData DeserializeHitData(string dataString)
    {
        string[] data = dataString.Split('_');
        int id = int.Parse(data[0]);
        Vector3 position = new Vector3(float.Parse(data[1]), float.Parse(data[2]), float.Parse(data[3]));
        Vector3 direction = new Vector3(float.Parse(data[4]), float.Parse(data[5]), float.Parse(data[6]));
        string material = data[7];
        bool isPowerBlock = bool.Parse(data[8]);

        return new HitData(id, position, direction, material, isPowerBlock);
    }

    public bool Slice(Collider collider,Vector3 position, Vector3 direction, bool isPowerBlock, bool isSlicingOpponentBlock)
    {
        // Debug.Log("SLICING " + collider.gameObject.name + " position " + position + " direction " + direction + " block position: " + collider.gameObject.transform.position);

        MeshFilter meshFilter = collider.GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = collider.GetComponent<MeshRenderer>();

        if (meshFilter != null && meshRenderer != null)
        {
            Mesh originalMesh = meshFilter.mesh;

            if (originalMesh != null)
            {
                SlicedHull hull = null;
                // Convert plane's world position and normal to the object's local space
                if (isSlicingOpponentBlock)
                {
                    hull = collider.gameObject.Slice(collider.gameObject.transform.position, direction);
                } else
                {
                    hull = collider.gameObject.Slice(position, direction); // SlicedHull comes from the library EzySlice, .Slice is method also from EzySlice
                }
                
                if (hull != null)
                {
                    // Create front mesh GameObject
                    GameObject frontObject = hull.CreateUpperHull(collider.gameObject, crossSectionMaterial);
                    frontObject.transform.parent = collider.transform.parent;
                    frontObject.transform.position = collider.transform.position;
                    frontObject.transform.rotation = collider.transform.rotation;
                    frontObject.transform.localScale = collider.transform.localScale;
                    Rigidbody frb = frontObject.AddComponent<Rigidbody>();
                    //frb.AddExplosionForce(cutForce, frontObject.transform.position, 1);
                    frb.velocity = (3*direction+2*speed.normalized+up) *cutSpeedMultiplier;


                    // Create back mesh GameObject
                    GameObject backObject = hull.CreateLowerHull(collider.gameObject, crossSectionMaterial);
                    backObject.transform.parent = collider.transform.parent;
                    backObject.transform.position = collider.transform.position;
                    backObject.transform.rotation = collider.transform.rotation;
                    backObject.transform.localScale = collider.transform.localScale;
                    Rigidbody brb = backObject.AddComponent<Rigidbody>();
                    //brb.AddExplosionForce(cutForce, backObject.transform.position, 1);
                    brb.velocity = (-3*direction + 2*speed.normalized+up) * cutSpeedMultiplier;

                    // Disable the original GameObject and make it stop moving
                    if (isPowerBlock)
                    {
                        collider.gameObject.GetComponentInParent<PowerBlockController>().enabled = false;
                        collider.gameObject.SetActive(false);
                    } else
                    {
                        collider.gameObject.GetComponentInParent<BlockController>().enabled = false;
                        collider.gameObject.SetActive(false);
                    }
                    
                    toBeDestroyed = collider.gameObject.transform.parent.gameObject; // Destroy whole block, not just child with collider
                    Destroy(toBeDestroyed, 2);

                    slashParticles.transform.position = frontObject.transform.position;
                    Vector3 directionXZ = new Vector3(up.x, 0, up.z);
                    slashParticles.transform.rotation = Quaternion.LookRotation(directionXZ, perpendicularVector);
                    slashParticles.Play();
                    if (isSlicingOpponentBlock && isPowerBlock)
                    {
                        Debug.Log("SLICED ENDED: opponent power block");
                    } else if (isSlicingOpponentBlock && !isPowerBlock)
                    {
                        Debug.Log("SLICED ENDED: opponent block");
                    } else if (!isSlicingOpponentBlock && isPowerBlock)
                    {
                        Debug.Log("SLICED ENDED: local power block");
                    } else if (!isSlicingOpponentBlock && !isPowerBlock)
                    {
                        Debug.Log("SLICED ENDED: local block");
                    }
                    return true;

                } else
                {
                    Debug.Log("NO HULL because position and direction dont match the block" + hull);
                    return false;
                }
            } else
            {
                Debug.LogError("No original mesh: " + originalMesh);
                return false;
            }
        } else
        {
            Debug.LogError("No mesh filter or renderer: " + meshFilter + " " + meshRenderer);
            return false;
        }
    }
}

public class HitData
{
    public int Id { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Direction { get; set; }

    public string SaberMaterial { get; set; }
    public bool IsPowerBlock { get; set; }
    public HitData(int id, Vector3 position, Vector3 direction, string material, bool isPowerBlock)
    {
        Id = id;
        Position = position;
        Direction = direction;
        SaberMaterial = material;
        IsPowerBlock = isPowerBlock;
    }
}

public class PowerUpData
{
    public bool Active { get; set; }
    public int Factor { get; set; }

    public PowerUpType PowerUpType { get; set; }
    public PowerUpData(bool active, int factor, PowerUpType powerUpType)
    {
        Active = active;
        Factor = factor;
        PowerUpType = powerUpType;
    }

    public static string Serialize(PowerUpData data)
    {
        return $"{data.Active}_{data.Factor}_{(int)data.PowerUpType}";
    }

    public static PowerUpData Deserialize(string dataString)
    {
        string[] data = dataString.Split('_');
        bool active = bool.Parse(data[0]);
        int factor = int.Parse(data[1]);
        PowerUpType powerUpType = (PowerUpType) int.Parse(data[2]);

        return new PowerUpData(active, factor, powerUpType);
    }
}

public class AttackData
{
    public bool Active { get; set; }
    public int Factor { get; set; }
    public AttackType AttackType { get; set; }
    public AttackData(bool active, int factor, AttackType attackType)
    {
        Active = active;
        Factor = factor;
        AttackType = attackType;
    }
    public static string Serialize(AttackData data)
    {
        return $"{data.Active}_{data.Factor}_{(int)data.AttackType}";
    }

    public static AttackData Deserialize(string dataString)
    {
        string[] data = dataString.Split('_');
        bool active = bool.Parse(data[0]);
        int factor = int.Parse(data[1]);
        AttackType attackType = (AttackType)int.Parse(data[2]);

        return new AttackData(active, factor, attackType);
    }
}

public class UpdateBlocksData
{
    public PowerUpType? PowerUpType { get; set; }
    public AttackType? AttackType { get; set; }
    public int? Size { get; set; }
    public int? Factor { get; set; }
    public bool IsLeftSide { get; set; }
}

public enum PowerUpType {
    Score,
    NoDirections,
    MultiDirections,
    Combi
}

public enum AttackType
{
    Score,
    Rotate,
    Combi
}

