using UnityEngine;
using EzySlice;

public class MeshSlicerTest : MonoBehaviour
{
    public float maxDistance = 1.0f;
    public KeyCode sliceKey = KeyCode.Space;

    void Update()
    {
        if (Input.GetKeyDown(sliceKey))
        {
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, maxDistance);

            foreach (Collider collider in nearbyColliders)
            {
                MeshFilter meshFilter = collider.GetComponent<MeshFilter>();
                MeshRenderer meshRenderer = collider.GetComponent<MeshRenderer>();

                if (meshFilter != null && meshRenderer != null)
                {
                    Mesh originalMesh = meshFilter.mesh;

                    if (originalMesh != null)
                    {
                        // Convert plane's world position and normal to the object's local space
                        //Vector3 planeLocalPos = meshFilter.transform.InverseTransformPoint(transform.position);
                        //Vector3 planeLocalNormal = meshFilter.transform.InverseTransformDirection(transform.up);


                        GameObject[] slices = collider.gameObject.SliceInstantiate(transform.position,transform.up, meshRenderer.material);

                        if (slices.Length == 2)
                        {
                            // Create front mesh GameObject
                            GameObject frontObject = slices[0];
                            frontObject.transform.position = collider.transform.position;
                            frontObject.transform.rotation = collider.transform.rotation;
                            frontObject.transform.localScale = collider.transform.localScale;

                            // Create back mesh GameObject
                            GameObject backObject = slices[1];
                            backObject.transform.position = collider.transform.position;
                            backObject.transform.rotation = collider.transform.rotation;
                            backObject.transform.localScale = collider.transform.localScale;

                            // Disable the original GameObject
                            collider.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }
}


