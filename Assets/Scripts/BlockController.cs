using UnityEngine;
using Photon.Pun;
using TMPro;

public class BlockController : MonoBehaviourPun//, IPunObservable
{
    public GameObject ScorePrefab;
    public GameObject Cubes;
    public GameObject WholeCube;
    public Vector3 moveDirection = Vector3.back;
    public int cutDirection = -1;
    public bool isLeftPlayerCube;
    public float moveSpeed;

    public int Id;
    public Vector3 normalScale;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI scoreTextBack;

    private Vector3[] cutDirectionRotations = new Vector3[]
    {
        new Vector3(0, 0, 0),      // down
        new Vector3(0, 0, 180),    // Up
        new Vector3(0, 0, 90),     // Right
        new Vector3(0, 0, -90),    // Left
        //new Vector3(0, 0, -45),    // Up-Left
        //new Vector3(0, 0, -135),   // Up-Right
        //new Vector3(0, 0, 135),    // Down-Left
        //new Vector3(0, 0, 45)      // Down-Right
    };

    private ScoreManager scoreManager;

    private bool isRotating = false;
    private bool arrowWasRemoved = false;

    private void OnDisable()
    {
        GetComponentInChildren<Light>().enabled = false;
    }

    public void Initialize(BeatSaberBlockSpawner.BeatSaberBlockData blockData, Vector3 moveDirection, bool isLeftPlayer, int id)
    {
        this.Id = id;
        this.moveDirection = moveDirection;
        this.isLeftPlayerCube = isLeftPlayer;
        this.normalScale = gameObject.transform.localScale;
        scoreManager = FindObjectOfType<ScoreManager>();

        cutDirection = blockData._cutDirection;

        if (cutDirection >= 0 && cutDirection < cutDirectionRotations.Length)
        {
            Vector3 cubeRotation = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, cutDirectionRotations[cutDirection].z);
            WholeCube.transform.eulerAngles = cubeRotation;
            var z = cutDirectionRotations[cutDirection].z;
            if (cutDirectionRotations[cutDirection].z == 90)
            {
                z = 90 + 90 + 180;
            } else if (cutDirectionRotations[cutDirection].z == -90)
            {
                z = -90 - 90 -180;
            }
            else if (cutDirectionRotations[cutDirection].z == 180)
            {
                z = 180 + 180;
            }
            Vector3 textRotation = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, z);
            Vector3 textRotationBack = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + 180, z + 180);
                
            scoreText.rectTransform.eulerAngles = textRotation;
            scoreTextBack.rectTransform.eulerAngles = textRotationBack;
        }
        else
        {
            // Hide the arrow for "Any" direction (cutDirection == 8)
            Cubes.SetActive(true);
            Vector3 textRotationBack = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y + 180, transform.eulerAngles.z + 180);

            scoreTextBack.rectTransform.eulerAngles = textRotationBack;
        }
    }

    public void ShowScore(int score)
    {
        if (ScorePrefab != null)
        {
            var scoreObject = Instantiate(ScorePrefab, transform.position, Quaternion.identity, transform);
            scoreObject.GetComponentInChildren<TextMeshPro>().text = score.ToString();
            if (score < 0)
            {
                scoreObject.GetComponentInChildren<TextMeshPro>().color = Color.red;
            }
            
        }
    }

    public void ChangeSize(int size, int factor)
    {
        switch (size) {
            case -1:
                gameObject.transform.localScale = new Vector3(0.25f, 0.25f, 0.25f);
                scoreText.text = "1/" + factor;
                scoreTextBack.text = "1/" + factor;
                break;
            case 1:
                scoreText.text = factor + "x";
                scoreTextBack.text = factor + "x";
                break;
            default:
                break;
        }
    }

    public void RemoveArrow()
    {
        if (!arrowWasRemoved)
        {
            Vector3 cubeRotation = new Vector3(180, transform.eulerAngles.y, transform.eulerAngles.z);
            transform.eulerAngles = cubeRotation;
            arrowWasRemoved = true;
        }
        
    }

    public void ShowAllDirections()
    {
        Cubes.SetActive(true);
    }

    public void RotateBlock()
    {
        isRotating = true;
    }

    private void AnimateRotation()
    {
        Vector3 RotateAmount = new Vector3(100, 100, 100);
        transform.Rotate(RotateAmount * Time.deltaTime);
    }

    private void Update()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        if (transform.position.magnitude > 20)
        {
            Destroy(gameObject);
            if (CheckPlayer.IsPlayerLeft() == isLeftPlayerCube)
            {
                scoreManager.AddScore(-10, isLeftPlayerCube);
            }
            
        }

        if (isRotating)
        {
            AnimateRotation();
        }
    }

    //public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    //{
    //    if (stream.IsWriting)
    //    {
    //        // If this is the owner, send cube position and rotation over the network
    //        stream.SendNext(transform.position);
    //        stream.SendNext(transform.rotation);
    //    }
    //    else
    //    {
    //        // If this is not the owner, receive cube position and rotation and update them
    //        transform.position = (Vector3)stream.ReceiveNext();
    //        transform.rotation = (Quaternion)stream.ReceiveNext();
    //    }
    //}
}
