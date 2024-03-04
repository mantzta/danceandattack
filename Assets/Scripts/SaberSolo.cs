using EzySlice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaberSolo : MonoBehaviour
{
    public string sliceTag;
    public Transform targetTransform;
    public Vector3 upAxis = Vector3.up;
    public float cutSpeedMultiplier = 0.1f;
    public float cutForce = 1000;

    public ParticleSystem slashParticles;

    public Material crossSectionMaterial;

    private Vector3 previousPosition;
    private Vector3 speed;
    private Vector3 perpendicularVector;
    private Vector3 up;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == sliceTag)
            Slice(collision.collider, transform.position, perpendicularVector);
    }


    void Start()
    {
        if (targetTransform != null)
        {
            previousPosition = targetTransform.position;
        }
    }

    void Update()
    {
        if (targetTransform != null)
        {
            // Calculate the speed of the target Transform
            speed = (targetTransform.position - previousPosition) / Time.deltaTime;
            previousPosition = targetTransform.position;

            up = transform.TransformVector(upAxis);

            // Calculate a new vector perpendicular to the speed and upAxis
            perpendicularVector = Vector3.Cross(speed, up).normalized;
        }
    }

    public void Slice(Collider collider, Vector3 position, Vector3 direction)
    {
        Debug.Log("SLICING " + collider.gameObject.name);

        MeshFilter meshFilter = collider.GetComponent<MeshFilter>();
        MeshRenderer meshRenderer = collider.GetComponent<MeshRenderer>();

        if (meshFilter != null && meshRenderer != null)
        {
            Mesh originalMesh = meshFilter.mesh;

            if (originalMesh != null)
            {
                // Convert plane's world position and normal to the object's local space

                SlicedHull hull = collider.gameObject.Slice(position, direction);

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
                    frb.velocity = (3 * direction + 2 * speed.normalized + up) * cutSpeedMultiplier;


                    // Create back mesh GameObject
                    GameObject backObject = hull.CreateLowerHull(collider.gameObject, crossSectionMaterial);
                    backObject.transform.parent = collider.transform.parent;
                    backObject.transform.position = collider.transform.position;
                    backObject.transform.rotation = collider.transform.rotation;
                    backObject.transform.localScale = collider.transform.localScale;
                    Rigidbody brb = backObject.AddComponent<Rigidbody>();
                    //brb.AddExplosionForce(cutForce, backObject.transform.position, 1);
                    brb.velocity = (-3 * direction + 2 * speed.normalized + up) * cutSpeedMultiplier;

                    // Disable the original GameObject and make it stop moving
                    collider.gameObject.GetComponentInParent<BlockControllerSolo>().enabled = false;
                    collider.gameObject.SetActive(false);
                    Destroy(collider.gameObject, 2);

                    slashParticles.transform.position = frontObject.transform.position;
                    Vector3 directionXZ = new Vector3(up.x, 0, up.z);
                    slashParticles.transform.rotation = Quaternion.LookRotation(directionXZ, perpendicularVector);
                    slashParticles.Play();


                }
            }
        }
    }
}

