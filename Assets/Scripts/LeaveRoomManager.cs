using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LeaveRoomManager : MonoBehaviour
{
    [SerializeField]
    GameObject GoBack_Button;

    // Start is called before the first frame update
    void Start()
    {
        GoBack_Button.GetComponent<Button>().onClick.AddListener(VirtualWorldManager.Instance.LeaveRoomAndLoadMultiStartScene);
    }
}
