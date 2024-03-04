using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerBlockController : MonoBehaviour
{
    public Vector3 moveDirection = Vector3.back;
    public bool isLeftPlayerCube;
    public float moveSpeed;
    public int Id;
    public void Initialize(Vector3 moveDirection, bool isLeftPlayer, int id)
    {
        this.moveDirection = moveDirection;
        this.isLeftPlayerCube = isLeftPlayer;
        this.Id = id;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        if (transform.position.magnitude > 25)
        {
            Destroy(gameObject);
        }
    }
}
