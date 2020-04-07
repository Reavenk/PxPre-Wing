using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Wing
    {
        public class ShapeConstructionUtil
        {
            public struct TwoVert
            { 
                public Vert a;
                public Vert b;

                public TwoVert(Vert a, Vert b)
                {
                    this.a = a;
                    this.b = b;
                }

                public override int GetHashCode()
                {
                    return a.GetHashCode() ^ b.GetHashCode();
                }

                public override bool Equals(object obj)
                {
                    TwoVert tv = (TwoVert)obj;

                    if(this.a != tv.a && this.a != tv.b)
                        return false;

                    if(this.b != tv.a && this.b != tv.b)
                        return false;

                    return true;
                }
            }

            Dictionary<Vector3, Vert> knownVerts = 
                new Dictionary<Vector3, Vert>();

            Dictionary<TwoVert, WEdge> knownEdgeFromVerts = 
                new Dictionary<TwoVert, WEdge>();

            Shape shape;

            public ShapeConstructionUtil(Shape shape)
            { 
                this.shape = shape;
            }

            public Face CreateFace(params Vector3 [] rv3s)
            { 
                List<Vert> verts = new List<Vert>();
                foreach(Vector3 v in rv3s)
                    verts.Add( this.GetVert(v));

                Face f = this.shape.AddBlankFace();

                for (int i = 0; i < verts.Count; ++i)
                {
                    WEdge we;
                    Vert vCur = verts[i];
                    Vert vNxt = verts[(i + 1) % verts.Count];

                    if(this.knownEdgeFromVerts.TryGetValue( new TwoVert(vCur, vNxt), out we) == true)
                    { 
                        if(we.conA.face == null || we.conB.face != null)
                        { } // Throw error

                        we.conB.face = f;
                    }
                    else
                    {
                        we = this.shape.AddBlankEdge();
                        this.knownEdgeFromVerts.Add(new TwoVert(vCur, vNxt), we);

                        vCur.edges.Add(we);
                        vNxt.edges.Add(we);

                        we.conA.vert = vCur;
                        we.conB.vert = vNxt;

                        we.conA.face = f;
                    }
                    f.edges.Add(we);
                }

                // Define the faceloop
                for(int i = 0; i < f.edges.Count; ++i)
                { 
                    // At this point, the size of edges and verts should be the same size,
                    // with 
                    WEdge we = f.edges[i];
                    Vert v = verts[i];

                    WEdge weNext = f.edges[ (i + 1) % f.edges.Count];

                    if(we.conA.face == f)
                        we.conA.edge = weNext;
                    else if(we.conB.face == f)
                        we.conB.edge = weNext;
                    else
                    { 
                        // Throw error
                    }

                }
                return f;
            }

            public Vert GetVert(Vector3 v)
            { 
                Vert ret;
                if(this.knownVerts.TryGetValue(v, out ret) == true)
                    return ret;

                ret = shape.AddVertice(v);
                this.knownVerts.Add(v, ret);
                return ret;
            }
        }
    }
}
