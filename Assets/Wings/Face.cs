using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PxPre
{
    namespace Wing
    {
        // N-gon
        public class Face 
        {
            // Debug identification
            static int dbgidctr = 0;
            int dbgid;

            // The edges that make the face. These
            // have a clockwise winding.
            public List<WEdge> edges = new List<WEdge>();

            // The position of the face in the shape's linked list.
            public LinkedListNode<Face> selfNode = null;

            public Face()
            { 
                this.dbgid = dbgidctr;
                ++dbgidctr;
            }

            public Vector3 CalcMidpoint()
            { 
                Vector3 accum = Vector3.zero;
                int ct = 0;

                if(edges.Count >= 3) // Sanity check
                { 
                    WEdge eStart = this.edges[0];
                    accum += eStart.GetEdgeVert(this).position;
                    ++ct;

                    for(
                        WEdge eIt = eStart.GetConnectingEdge(this);
                        eIt != eStart;
                        eIt = eIt.GetConnectingEdge(this))
                    { 
                        accum += eIt.GetEdgeVert(this).position;
                        ++ct;
                    }
                }

                if(ct == 0)
                    return Vector3.zero;

                return accum / (float)ct;
            }
        }
    }
}