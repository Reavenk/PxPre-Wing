using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Wing
    {
        public class Vert
        {
            // Debug identification
            static int dbgidctr = 0;
            int dbgid;

            // 3D position in the shape
            public Vector3 position;
            // The edges that the verts contribute to.
            public List<WEdge> edges = new List<WEdge>();

            // The position of the vert in the shape's linked list
            public LinkedListNode<Vert> selfNode = null;

            public Vert()
            { 
                this.dbgid = dbgidctr;
                ++dbgidctr;
            }

            public WEdge EdgeOnFace(Face f)
            { 
                foreach(WEdge e in this.edges)
                { 
                    if(e.conA.face == f || e.conB.face == f)
                        return e;
                }

                return null;
            }

            public WEdge EdgeWithFaceAndVert(Face f, Vert v)
            { 
                foreach(WEdge e in this.edges)
                { 
                    if(e.conA.face == f && e.conA.vert == v)
                        return e;

                    if(e.conB.face == f && e.conB.vert == v)
                        return e;
                }

                return null;
            }

            public WEdge EdgeWithFaceAndNotVert(Face f, Vert v)
            {
                foreach(WEdge e in this.edges)
                { 
                    if(e.conA.face == f && e.conA.vert != v)
                        return e;

                    if(e.conB.face == f && e.conB.vert != v)
                        return e;

                }
                return null;
            }
        }
    }
}