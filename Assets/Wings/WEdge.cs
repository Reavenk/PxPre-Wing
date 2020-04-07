using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Wing
    {
        // Vertex to edge connection.
        public struct VertEdgeCon
        { 
            // One of the 2 vertices  that make up the edge.
            public Vert vert;

            // The edge connected to this edge, from vert.
            public WEdge edge;

            // The face that this edge is connected to.
            // If this edge, and the member this.edge form
            // a face, that will be this variable. if this were
            // the top point of a vertical edge, this would be
            // the left face.
            public Face face;

            public VertEdgeCon(Vert vert, WEdge edge, Face face)
            { 
                this.vert = vert;
                this.edge = edge;
                this.face = face;
            }
        }

        public class WEdge
        {
            // Debug identification
            static int dbgidctr = 0;
            int dbgid;

            // Half edges
            public VertEdgeCon conA;
            public VertEdgeCon conB;

            // The position of teh edge in the shape's linked list.
            public LinkedListNode<WEdge> selfNode = null;

            public WEdge()
            { 
                this.dbgid = dbgidctr;
                ++dbgidctr;
            }

            public int GetWingIndex(Face f)
            { 
                if(this.conA.face == f)
                    return 0;

                if(this.conB.face == f)
                    return 1;

                return -1;
            }

            public VertEdgeCon GetWingFromIndex(int idx)
            { 
                if(idx == 0)
                    return this.conA;

                if(idx == 1)
                    return this.conB;

                return new VertEdgeCon();
            }

            public Vector3 CalcMidpoint()
            { 
                return 
                    (this.conA.vert.position + this.conB.vert.position)/2.0f;
            }

            public VertEdgeCon GetConInfo(Face f)
            { 
                if( this.conA.face == f)
                    return this.conA;
                if( this.conB.face == f)
                    return this.conB;

                return new VertEdgeCon();
            }

            public Vert GetEdgeVert(Face f)
            { 
                if(this.conA.face == f)
                    return this.conA.vert;

                if(this.conB.face == f)
                    return this.conB.vert;

                return null;
            }

            public WEdge GetConnectingEdge(Face f)
            {
                if (this.conA.face == f)
                    return this.conA.edge;

                else if( this.conB.face == f)
                    return this.conB.edge;

                return null;
            }

            public Face ReturnOtherFace(Face f)
            { 
                if(this.conA.face == f)
                    return this.conB.face;

                if(this.conB.face == f)
                    return this.conA.face;

                return null;
            }

            public bool ReplaceFace(Face fmatch, Face fnew)
            { 
                if(this.conA.face == fmatch)
                {
                    this.conA.face = fnew;
                    return true;
                }

                if(this.conB.face == fmatch)
                { 
                    this.conB.face = fnew;
                    return true;
                }

                return false;
            }

            public Vert OtherVert(Vert v)
            { 
                if(this.conA.vert == v)
                    return this.conB.vert;

                if(this.conB.vert == v)
                    return this.conA.vert;

                return null;
            }

            public bool SetFaceFromVert(Vert v, Face f)
            { 
                if(this.conA.vert == v)
                {
                    this.conA.face = f;
                    return true;
                }

                if(this.conB.vert == v)
                { 
                    this.conB.face = f;
                    return true;
                }

                return false;
            }

            public bool SetEdgeFromFace(WEdge e, Face f)
            { 
                if(this.conA.face == f)
                { 
                    this.conA.edge = e;
                    return true;
                }

                if(this.conB.face == f)
                { 
                    this.conB.edge = e;
                    return true;
                }

                return false;
            }
        }
    }
}