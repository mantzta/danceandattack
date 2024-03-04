using UnityEngine;
using System.Collections.Generic;

public class MeshSlicer : MonoBehaviour
{
    public static void SliceMesh(Mesh mesh, Vector3 planePoint, Vector3 planeNormal, out Mesh frontMesh, out Mesh backMesh)
    {
        // Initialize front and back mesh data
        List<Vector3> frontVertices = new List<Vector3>();
        List<Vector3> backVertices = new List<Vector3>();
        List<int> frontTriangles = new List<int>();
        List<int> backTriangles = new List<int>();

        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;

        // Loop through each triangle in the original mesh
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3[] triangleVertices = new Vector3[3]
            {
                vertices[triangles[i]],
                vertices[triangles[i + 1]],
                vertices[triangles[i + 2]]
            };

            List<Vector3> frontTriangleVertices = new List<Vector3>();
            List<Vector3> backTriangleVertices = new List<Vector3>();

            // For each vertex in the triangle, determine if it lies in front or behind the slicing plane
            for (int j = 0; j < 3; j++)
            {
                Vector3 vertex = triangleVertices[j];
                Vector3 nextVertex = triangleVertices[(j + 1) % 3];

                float dot = Vector3.Dot(planeNormal, vertex - planePoint);

                if (dot >= 0)
                {
                    frontTriangleVertices.Add(vertex);
                }
                else
                {
                    backTriangleVertices.Add(vertex);
                }

                // Check for intersection points between the plane and the triangle edges
                Vector3 edge = nextVertex - vertex;
                float edgeDot = Vector3.Dot(planeNormal, edge);

                if (Mathf.Abs(edgeDot) > Mathf.Epsilon)
                {
                    float t = (Vector3.Dot(planeNormal, planePoint - vertex)) / edgeDot;
                    if (t > 0 && t < 1)
                    {
                        Vector3 intersection = vertex + t * edge;
                        frontTriangleVertices.Add(intersection);
                        backTriangleVertices.Add(intersection);
                    }
                }
            }

            // Add the newly created triangles to their respective front or back mesh lists
            AddTriangles(frontVertices, frontTriangles, frontTriangleVertices);
            AddTriangles(backVertices, backTriangles, backTriangleVertices);
        }

        // Create the front and back meshes
        frontMesh = new Mesh();
        frontMesh.vertices = frontVertices.ToArray();
        frontMesh.triangles = frontTriangles.ToArray();
        frontMesh.RecalculateNormals();
        frontMesh.RecalculateBounds();

        backMesh = new Mesh();
        backMesh.vertices = backVertices.ToArray();
        backMesh.triangles = backTriangles.ToArray();
        backMesh.RecalculateNormals();
        backMesh.RecalculateBounds();
    }

    private static void AddTriangles(List<Vector3> vertices, List<int> triangles, List<Vector3> newTriangleVertices)
    {
        if (newTriangleVertices.Count >= 3)
        {
            for (int i = 0; i < newTriangleVertices.Count - 2; i++)
            {
                triangles.Add(vertices.Count);
                triangles.Add(vertices.Count + i + 1);
                triangles.Add(vertices.Count + i + 2);

                vertices.Add(newTriangleVertices[0]);
                vertices.Add(newTriangleVertices[i + 1]);
                vertices.Add(newTriangleVertices[i + 2]);
            }
        }
    }
}
