using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Wing
    {
        // A mesh object
        public class Shape
        {
            public enum Axis
            { 
                X = 0, 
                Y = 1, 
                Z = 2
            }

            // The various linked list of elements that make up the 3D mesh model.
            public LinkedList<WEdge> edges              = new LinkedList<WEdge>();
            public LinkedList<Vert> vertices            = new LinkedList<Vert>();
            public LinkedList<Face> faces               = new LinkedList<Face>();

            // Extra local transform.
            public Vector3 localPos     = Vector3.zero;
            public Vector3 localEuler   = Vector3.zero;
            public Vector3 localScale   = Vector3.one;

            public void Clear()
            {
                while(this.edges.Count != 0)
                { 
                    WEdge e = this.edges.First.Value;

                    e.conA.edge = null;
                    e.conA.face = null;
                    e.conA.vert = null;
                    e.conB.edge = null;
                    e.conB.face = null;
                    e.conB.vert = null;

                    e.selfNode = null;

                    this.edges.RemoveFirst();
                }

                while(this.vertices.Count != 0)
                { 
                    Vert v = this.vertices.First.Value;

                    v.edges.Clear();
                    v.selfNode = null;

                    this.vertices.RemoveFirst();
                }

                while(this.faces.Count != 0)
                { 
                    Face f = this.faces.First.Value;

                    f.edges.Clear();
                    f.selfNode = null;

                    f.edges.Clear();
                }
            }

            public Vert AddVertice(Vector3 pos)
            { 
                Vert v = new Vert();
                v.position = pos;
                v.selfNode = this.vertices.AddLast(v);
                return v;
            }

            Vert AddVertice(float x, float y, float z)
            { 
                return this.AddVertice(new Vector3(x, y, z));
            }

            public Face AddBlankFace()
            { 
                Face f = new Face();
                f.selfNode = this.faces.AddLast(f);
                return f;
            }

            public WEdge AddBlankEdge()
            { 
                WEdge e = new WEdge();
                e.selfNode = this.edges.AddLast(e);
                return e;
            }

            public bool IsEmpty()
            { 
                return 
                    this.edges.Count    == 0 && 
                    this.vertices.Count == 0 && 
                    this.faces.Count    == 0;
            }

            public void AddSimplePlane(Vector3 pos, Quaternion rot, float width, float depth)
            { 
                Vector2 rad = new Vector2(width/2.0f, depth/2.0f);

                Vector3 v0 = new Vector3(-rad.x, 0.0f,  rad.y);
                Vector3 v1 = new Vector3( rad.x, 0.0f,  rad.y);
                Vector3 v2 = new Vector3( rad.x, 0.0f, -rad.y);
                Vector3 v3 = new Vector3(-rad.x, 0.0f, -rad.y);

                ShapeConstructionUtil scu = new ShapeConstructionUtil(this);
                scu.CreateFace(v0, v1, v2, v3);
                scu.CreateFace(v3, v2, v1, v0);
            }

            public void AddSimpleCube(Vector3 pos, Quaternion rot, float width, float height, float depth)
            { 
                Vector3 rad = new Vector3(width/2.0f, height / 2.0f, depth / 2.0f);

                Vector3 v0 = new Vector3(-rad.x,  rad.y,  rad.z);
                Vector3 v1 = new Vector3( rad.x,  rad.y,  rad.z);
                Vector3 v2 = new Vector3( rad.x,  rad.y, -rad.z);
                Vector3 v3 = new Vector3(-rad.x,  rad.y, -rad.z);
                Vector3 v4 = new Vector3(-rad.x, -rad.y,  rad.z);
                Vector3 v5 = new Vector3( rad.x, -rad.y,  rad.z);
                Vector3 v6 = new Vector3( rad.x, -rad.y, -rad.z);
                Vector3 v7 = new Vector3(-rad.x, -rad.y, -rad.z);

                ShapeConstructionUtil scu = new ShapeConstructionUtil(this);
                // top
                scu.CreateFace(v0, v1, v2, v3);
                // bottom 
                scu.CreateFace(v7, v6, v5, v4);
                // front
                scu.CreateFace(v3, v2, v6, v7);
                // back
                scu.CreateFace(v1, v0, v4, v5);
                // left
                scu.CreateFace(v0, v3, v7, v4);
                // right
                scu.CreateFace(v2, v1, v5, v6);
            }

            public void AddCylinder(Vector3 pos, Quaternion rot, Axis axis, float topRadius, float botRadius, float height, int slices, int rings)
            {
                slices = Mathf.Max(slices, 3);
                rings = Mathf.Max(rings, 0);

                float halfHeight = height / 2.0f;
                Vector3 vpoleTop = new Vector3(0.0f, halfHeight, 0.0f);
                Vector3 vpoleBot = new Vector3(0.0f, -halfHeight, 0.0f);

                ShapeConstructionUtil scu = new ShapeConstructionUtil(this);

                // Instead of recalculating as needed, we're going to store all the verts we use.
                // 
                // While this may help with avoiding recalculations, this is more done as a saftey measure
                // for the possibility of floating point errors.
                List<Vector3> lstv3 = new List<Vector3>();
                lstv3.Add(vpoleTop);
                lstv3.Add(vpoleBot);

                int topSt = lstv3.Count;

                // Figure out all the vertices we would be producing
                int vrings = rings + 2;
                for(int s = 0; s < vrings; ++s)
                {
                    float hl = s / (vrings - 1.0f);
                    float rad = 
                        Mathf.Lerp( 
                            topRadius, 
                            botRadius, 
                            hl);

                    float ypos = Mathf.Lerp( halfHeight, -halfHeight, hl);


                    for(int i = 0; i < slices; ++i)
                    { 
                        float lambda = (float)i / slices;
                        float angle = lambda * 2.0f * Mathf.PI;

                        float fx = Mathf.Cos(angle);
                        float fy = Mathf.Sin(angle);

                        lstv3.Add(new Vector3(fx * topRadius, ypos, fy * topRadius));
                    }
                }

                // Stitch the top pole
                for(int i = 0; i < slices; ++i)
                {
                    const int topPoleIdx = 0;
                    int triIdx1 = topSt + i;
                    int triIdx2 = topSt + ((i + 1) % slices);
                    scu.CreateFace(
                        lstv3[topPoleIdx], 
                        lstv3[triIdx2], 
                        lstv3[triIdx1]);
                }

                // Stitch the bottom pole
                int botSt = lstv3.Count - slices;
                for(int i = 0; i < slices; ++i)
                { 
                    const int botPoleIdx = 1;
                    int triIdx1 = botSt + i;
                    int triIdx2 = botSt + ((i + 1) % slices);
                    scu.CreateFace(
                        lstv3[botPoleIdx],
                        lstv3[triIdx1],
                        lstv3[triIdx2]);
                }

                // Stitch the body
                for(int j = 0; j < rings + 1; ++j)
                { 
                    int s1 = topSt + ((j + 0) * slices);
                    int s2 = topSt + ((j + 1) * slices);
                
                    for(int i = 0; i < slices; ++i)
                    { 
                        int a = s1 + i;
                        int b = s1 + (i + 1) % slices;
                        int c = s2 + (i + 1) % slices;
                        int d = s2 + i;
                
                        scu.CreateFace(
                            lstv3[a],
                            lstv3[b],
                            lstv3[c],
                            lstv3[d]);
                    }
                }
                //int fringSt = botSt + lstv3.Count;
            }

            public void AddUVSphere(Vector3 pos, Quaternion rot, Axis axis, float heightRad, float beltRad, int slices, int rings)
            {
                slices = Mathf.Max(slices, 3);
                rings = Mathf.Max(rings, 0);

                Vector3 vpoleTop = new Vector3(0.0f, heightRad, 0.0f);
                Vector3 vpoleBot = new Vector3(0.0f, -heightRad, 0.0f);

                ShapeConstructionUtil scu = new ShapeConstructionUtil(this);

                // Instead of recalculating as needed, we're going to store all the verts we use.
                // 
                // While this may help with avoiding recalculations, this is more done as a saftey measure
                // for the possibility of floating point errors.
                List<Vector3> lstv3 = new List<Vector3>();
                lstv3.Add(vpoleTop);
                lstv3.Add(vpoleBot);

                int topSt = lstv3.Count;

                // Figure out all the vertices we would be producing
                int vrings = rings + 2;
                for (int s = 0; s < vrings; ++s)
                {
                    float hl = (float)(s + 1) / (float)(vrings + 1);

                    float rad = Mathf.Sin(hl * Mathf.PI) * beltRad;

                    float ypos = Mathf.Cos(hl * Mathf.PI) * heightRad;
                    //float ypos = Mathf.Lerp(heightRad, -heightRad, hl);

                    for (int i = 0; i < slices; ++i)
                    {
                        float lambda = (float)i / slices;
                        float angle = lambda * 2.0f * Mathf.PI;

                        float fx = Mathf.Cos(angle);
                        float fy = Mathf.Sin(angle);

                        lstv3.Add(new Vector3(fx * rad, ypos, fy * rad));
                    }
                }

                // Stitch the top pole
                for (int i = 0; i < slices; ++i)
                {
                    const int topPoleIdx = 0;
                    int triIdx1 = topSt + i;
                    int triIdx2 = topSt + ((i + 1) % slices);
                    scu.CreateFace(
                        lstv3[topPoleIdx],
                        lstv3[triIdx2],
                        lstv3[triIdx1]);
                }

                // Stitch the bottom pole
                int botSt = lstv3.Count - slices;
                for (int i = 0; i < slices; ++i)
                {
                    const int botPoleIdx = 1;
                    int triIdx1 = botSt + i;
                    int triIdx2 = botSt + ((i + 1) % slices);
                    scu.CreateFace(
                        lstv3[botPoleIdx],
                        lstv3[triIdx1],
                        lstv3[triIdx2]);
                }

                // Stitch the body
                for (int j = 0; j < rings + 1; ++j)
                {
                    int s1 = topSt + ((j + 0) * slices);
                    int s2 = topSt + ((j + 1) * slices);

                    for (int i = 0; i < slices; ++i)
                    {
                        int a = s1 + i;
                        int b = s1 + (i + 1) % slices;
                        int c = s2 + (i + 1) % slices;
                        int d = s2 + i;

                        scu.CreateFace(
                            lstv3[a],
                            lstv3[b],
                            lstv3[c],
                            lstv3[d]);
                    }
                }
            }

            public void Spherize(Vector3 pos, float radius)
            { 
                foreach(Vert v in this.vertices)
                { 
                    Vector3 tv = v.position - pos;
                    v.position = pos + tv.normalized * radius;
                }
            }

            void SimpleTriangulate()
            { 
                List<Face> origFaces = new List<Face>(this.faces);

                foreach(Face f in origFaces)
                { 
                    if(f.edges.Count <= 3)
                        break;

                    List<WEdge> origEdges = new List<WEdge>(f.edges);

                    // Created edges that will be used as part of the next triangle in the fan
                    WEdge sE = null;

                    // Processing the first triangle of the fan is a bit different
                    // than the rest.

                    // TODO:
                }
            }

            public void SimpleSubdivide()
            { 
                Dictionary<WEdge, Vert> midEdge = new Dictionary<WEdge, Vert>();
                Dictionary<Face, Vert> midFace = new Dictionary<Face, Vert>();

                // We need to record what we're starting out with, because as we subdivide,
                // it's going to change those containers
                List<WEdge> startingEdges = new List<WEdge>(this.edges);
                List<Face> startingFaces = new List<Face>(this.faces);

                // Edge centroids
                foreach(WEdge e in startingEdges)

                {
                    this.edges.Remove(e.selfNode);
                    if(e.conA.vert != null)
                        e.conA.vert.edges.Remove(e);

                    if(e.conB.vert != null)
                        e.conB.vert.edges.Remove(e);

                    // Centroid
                    Vert v = this.AddVertice( e.CalcMidpoint());
                    midEdge[e] = v;

                    WEdge e1 = this.AddBlankEdge();
                    WEdge e2 = this.AddBlankEdge();

                    // Subdivision
                    v.edges.Add(e1);
                    v.edges.Add(e2);

                    // Set them to the old faces (new faces don't exist yet)
                    // we do this, so later on we can identify them.
                    e1.conA.face = e.conA.face;
                    e2.conA.face = e.conA.face;
                    e1.conB.face = e.conB.face;
                    e2.conB.face = e.conB.face;

                    // Define the verts for the edges
                    e1.conA.vert = e.conA.vert;
                    e1.conB.vert = v;
                    // Define the verts for the edges
                    e2.conA.vert = v;
                    e2.conB.vert = e.conB.vert;
                }

                // Face centroids
                foreach(Face f in startingFaces)
                {
                    this.faces.Remove(f.selfNode);

                    // Create subdivided centroid vert.
                    Vert v = this.AddVertice( f.CalcMidpoint());
                    midFace[f] = v;

                    foreach(WEdge e in f.edges)
                    { 
                        WEdge enew = this.AddBlankEdge();

                        enew.conA.vert = v;
                        enew.conB.vert = midEdge[e];

                        v.edges.Add(enew);
                        midEdge[e].edges.Add(enew); 
                    }
                }

                foreach(Face f in startingFaces)
                { 
                    // The edges in a vertice aren't in a certain order, but
                    // there's a property we can leverage
                    Vert v = midFace[f];
                    for(int i = 0; i < v.edges.Count; ++i)
                    { 
                        int nextIdx = (i + 1) % v.edges.Count;
                        WEdge oldEdge = f.edges[i];
                        WEdge oldEdgeNext = f.edges[nextIdx];

                        // the subdividing vert for oldEdge
                        Vert subdivVert = midEdge[oldEdge];
                        Vert subdivVertNext = midEdge[oldEdgeNext];

                        // New edge from centroid vert to subdivVert
                        WEdge e0 = v.edges[i];
                        WEdge e1 = subdivVert.EdgeWithFaceAndNotVert(f, oldEdge.GetEdgeVert(f));
                        WEdge e2 = subdivVertNext.EdgeWithFaceAndVert(f, oldEdgeNext.GetEdgeVert(f));
                        WEdge e3 = v.edges[nextIdx];

                        Face fnew = this.AddBlankFace();

                        // Add edges
                        fnew.edges.Add(e0);
                        fnew.edges.Add(e1);
                        fnew.edges.Add(e2);
                        fnew.edges.Add(e3);
                        // Assemble the edge loop
                        e0.conA.edge = e1;
                        e0.conA.face = fnew;
                        //
                        e1.ReplaceFace(f, fnew);
                        e1.SetEdgeFromFace(e2, fnew);
                        //
                        e2.ReplaceFace(f, fnew);
                        e2.SetEdgeFromFace(e3, fnew);
                        //
                        e3.conB.edge = e0;
                        e3.conB.face = fnew;
                    }
                }
            }

            public bool CollapseSingleVert(Vert v)
            { 
                return false; // !TODO
            }

            public bool CollapseSingleEdge(WEdge e)
            { 
                return false; // !TODO
            }

            public Mesh GenerateMesh()
            { 
                // Convert unique verts to an array of Vector3 that can be 
                // int indexable.
                List<Vector3> mverts = new List<Vector3>();
                Dictionary<Vert, int> vlookup = new Dictionary<Vert, int>();
                foreach(Vert v in this.vertices)
                { 
                    vlookup.Add(v, mverts.Count);
                    mverts.Add(v.position);
                }

                // Convert the polygons into faces 
                List<int> tris = new List<int>();
                foreach(Face f in this.faces)
                { 
                    List<int> facevertidxs = new List<int>();

                    // An edge can have two faces comming off if it, so an edge
                    // has two faces assigned to it. Which edge and edge loop do we
                    // trace to create the face?
                    WEdge startingEdge = null;
                    for(WEdge wit = f.edges[0]; wit != startingEdge; /*adv done in loop*/)
                    {
                        WEdge next = null;
                        if(wit.conA.face == f)
                        {
                            facevertidxs.Add( vlookup[wit.conA.vert] );
                            next = wit.conA.edge;
                        }
                        else if(wit.conB.face == f)
                        {
                            facevertidxs.Add(vlookup[wit.conB.vert]);
                            next = wit.conB.edge;
                        }
                        else
                        { } // TODO: Throw error

                        if(startingEdge == null)
                            startingEdge = wit;

                        wit = next;
                    }

                    // This shouldn't happen unless we're in a degenerate state
                    if(facevertidxs.Count < 3)
                        continue;
                    else if(facevertidxs.Count == 3)
                    {
                        // Just append them in directly - no need for extra complicated code.
                        tris.AddRange(facevertidxs);
                    }
                    else
                    { 
                        // For else we need to tesselate the polygon into triangles. For now we're just going 
                        // to be simple and direct, do a triangle fan tesselation, using the first point as a pivot.
                        int pivot = facevertidxs[0];
                        for(
                            int i = 1;                  // The first point is already accounted for in pivot
                            i < facevertidxs.Count - 1; // We can only process points that aren't the last one.
                            ++i)
                        { 
                            tris.Add(pivot);
                            tris.Add(facevertidxs[i + 0]);
                            tris.Add(facevertidxs[i + 1]);
                        }
                    }
                }


                Mesh m = new Mesh();
                if(mverts.Count != 0 && tris.Count != 0)
                { 
                    m.subMeshCount = 1;
                    m.vertices = mverts.ToArray();
                    m.SetIndices( tris.ToArray(), MeshTopology.Triangles, 0);
                    //m.RecalculateNormals();
                    m.RecalculateBounds();
                }

                return m;
            }

            public Shape Clone()
            { 
                Shape sret = new Shape();

                // Create a map, where we can reference an old element and know what the
                // equivalent is in the new clone.
                Dictionary<Vert, Vert> oldToNewVert     = new Dictionary<Vert, Vert>();
                Dictionary<WEdge, WEdge> oldToNewEdge   = new Dictionary<WEdge, WEdge>();
                Dictionary<Face, Face> oldToNewFace     = new Dictionary<Face, Face>();

                // Start copying elements to fill the maps.
                foreach(Vert v in this.vertices)
                    oldToNewVert.Add(v, sret.AddVertice(v.position));

                foreach(WEdge e in this.edges)
                    oldToNewEdge.Add(e, sret.AddBlankEdge());

                foreach(Face f in this.faces)
                    oldToNewFace.Add(f, sret.AddBlankFace());

                // Now that we have everything that could be referenced when constructing 
                // the clone, start making the equivalent cloned connections.

                foreach(KeyValuePair<Vert,Vert> kvpv in oldToNewVert)
                { 
                    Vert oldV = kvpv.Key;
                    Vert newV = kvpv.Value;

                    foreach(WEdge e in oldV.edges)
                        newV.edges.Add( oldToNewEdge[e]);
                }

                foreach(KeyValuePair<WEdge,WEdge> kvpe in oldToNewEdge)
                { 
                    WEdge oldE = kvpe.Key;
                    WEdge newE = kvpe.Value;

                    // Transfer mapped conA stuff
                    if(oldE.conA.edge != null)
                        newE.conA.edge = oldToNewEdge[oldE.conA.edge];

                    if(oldE.conA.face != null)
                        newE.conA.face = oldToNewFace[oldE.conA.face];

                    if(oldE.conA.vert != null)
                        newE.conA.vert = oldToNewVert[oldE.conA.vert];

                    // Transfer mapped conB stuff
                    if(oldE.conB.edge != null)
                        newE.conB.edge = oldToNewEdge[oldE.conB.edge];

                    if(oldE.conB.face != null)
                        newE.conB.face = oldToNewFace[oldE.conB.face];

                    if(oldE.conB.vert != null)
                        newE.conB.vert = oldToNewVert[oldE.conB.vert];
                }

                foreach(KeyValuePair<Face,Face> kvpf in oldToNewFace)
                { 
                    Face oldF = kvpf.Key;
                    Face newF = kvpf.Value;

                    foreach( WEdge e in oldF.edges)
                        newF.edges.Add(oldToNewEdge[e]);
                }

                return sret;
            }
        }
    }
}