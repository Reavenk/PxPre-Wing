using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using PxPre.Wing;

public class SimpleTest : MonoBehaviour
{
    

    void Start()
    {
        Shape shape = new Shape();
        //shape.AddUVSphere( Vector3.zero, Quaternion.identity, Shape.Axis.X, 5.0f, 5.0f, 12, 12);
        //shape.AddSimplePlane(Vector3.zero, Quaternion.identity, 10.0f, 10.0f);
        //
        shape.AddSimpleCube(Vector3.zero, Quaternion.identity, 3.0f, 3.0f, 3.0f);
        shape.SimpleSubdivide();
        shape.SimpleSubdivide();
        shape.SimpleSubdivide();
        shape.Spherize(Vector3.zero, 3.0f);

        //shape.AddCylinder(Vector3.zero, Quaternion.identity, Shape.Axis.X, 10.0f, 5.0f, 10.0f, 12, 3);

        Mesh m = shape.GenerateMesh();

        GameObject goSimple = new GameObject("test");
        //
        MeshRenderer mr = goSimple.AddComponent<MeshRenderer>();
        mr.sharedMaterial = new Material(Shader.Find("Standard"));

        MeshFilter mf = goSimple.AddComponent<MeshFilter>();

        mf.sharedMesh = m;
    }

    void Update()
    {
        
    }
}
