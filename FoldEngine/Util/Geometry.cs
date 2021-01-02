using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using FoldEngine;
using FoldEngine.Components;
using FoldEngine.Graphics;
using FoldEngine.Util;
using Microsoft.Xna.Framework;

namespace EntryProject.Util {
    [DebuggerDisplay("{" + nameof(DebugDisplayString) + ",nq}")]
    public struct Line {
        public Vector2 From;
        public Vector2 To;

        public Vector2 Center => (From + To) / 2;

        public float MagnitudeSqr => Vector2.DistanceSquared(From, To);
        public float Magnitude => Vector2.Distance(From, To);

        public Vector2 Normal => ((Complex) (To - From) * Complex.Imaginary).Normalized;

        public string DebugDisplayString => $"{From}, {To}";

        public Line(Vector2 from, Vector2 to) {
            From = from;
            To = to;
        }

        public bool Contains(Vector2 point, float precision = 0.000001f) {
            return Math.Abs(Vector2.DistanceSquared(From, point) + Vector2.DistanceSquared(point, To) - MagnitudeSqr) < precision;
        }

        public float DistanceFromPoint(Vector2 point, bool capped) {
            Vector2 flat = LayFlat(this, ref point, out Complex _);
            if(capped) {
                if(point.X <= 0) return point.Length();
                if(point.X >= flat.X) return Vector2.Distance(point, flat);
            }
            return point.Y;
        }

        public Vector2 SnapPointToLine(Vector2 point, bool capped) {
            Vector2 flat = LayFlat(this, ref point, out Complex undo);
            if(capped) {
                if(point.X <= 0) return this.From;
                if(point.X >= flat.X) return this.To;
            }
            return new Complex(point.X, 0) * undo + (Complex) this.From;
        }

        public Vector2? Intersect(Line other, bool thisCapped, bool otherCapped) {
            if(this.MagnitudeSqr == 0) {
                if(other.MagnitudeSqr == 0) {
                    return this.From == other.From ? this.From : (Vector2?) null;
                }

                return other.Contains(this.From) ? this.From : (Vector2?) null;
            } else if(other.MagnitudeSqr == 0) {
                return this.Contains(other.From) ? other.From : (Vector2?) null;
            }
            Vector2 flat = LayFlat(this, ref other, out Complex undo);

            if(otherCapped && Math.Sign(other.From.Y) == Math.Sign(other.To.Y)) return null;
            double xIntersect = (double) other.From.X
                                + (-(double) other.From.Y / ((double) other.To.Y - other.From.Y))
                                * ((double) other.To.X - other.From.X);
            if(thisCapped) {
                if(xIntersect < 0 || xIntersect > flat.X) return null;
            }
            return new Complex((float) xIntersect, 0) * undo + (Complex) this.From;
        }

        public static Vector2 LayFlat(Line line, ref Vector2 point, out Complex undo) {
            point -= line.From;
            line.To -= line.From;
            line.From = Vector2.Zero;

            undo = ((Complex) line.To).Normalized;

            point = (Vector2)((Complex) point / undo);
            line.To = (Vector2)((Complex) line.To / undo);

            return line.To;
        }

        public static Vector2 LayFlat(Line line, ref Line other, out Complex undo) {
            other -= line.From;
            line.To -= line.From;
            line.From = Vector2.Zero;

            undo = ((Complex) line.To).Normalized;

            other.From = (Vector2)((Complex) other.From / undo);
            other.To = (Vector2)((Complex) other.To / undo);
            line.To = (Vector2)((Complex) line.To / undo);

            if(other.From.X.Equals(float.NaN)) {
                Console.WriteLine("uh oh");
            }

            return line.To;
        }

        public static Line operator +(Line line, Vector2 offset) {
            return new Line(line.From + offset, line.To + offset);
        }

        public static Line operator -(Line line, Vector2 offset) {
            return new Line(line.From - offset, line.To - offset);
        } 
    }

    public static class Polygon {
        [Pure]
        public static PolygonIntersectionVertex[][] ComputePolygonIntersection(
            MeshCollection meshes,
            string meshIdA,
            Transform transformA,
            string meshIdB,
            Transform transformB) {
            Vector2[] verticesA = new Vector2[meshes.GetVertexCountForMesh(meshIdA)];
            Vector2[] verticesB = new Vector2[meshes.GetVertexCountForMesh(meshIdA)];

            int i = 0;
            foreach(Vector2 vertex in meshes.GetVerticesForMesh(meshIdA)) {
                verticesA[i] = transformA.Apply(vertex);
                i++;
            }
            i = 0;
            foreach(Vector2 vertex in meshes.GetVerticesForMesh(meshIdB)) {
                verticesB[i] = transformB.Apply(vertex);
                i++;
            }
            return ComputePolygonIntersection(verticesA, verticesB);
        }
        
        private static readonly OrderedList<float, Intersection> Intersections = new OrderedList<float, Intersection>(v => v.OrderA);
        
        [Pure]
        public static PolygonIntersectionVertex[][] ComputePolygonIntersection(
            Vector2[] verticesA,
            Vector2[] verticesB) {
            //A is the black polygon
            //B is the red polygon
            //(reference images)
            
            Intersections.Clear();
            
            int ins = 0;
            int outs = 0;
            Vector2? minIntersection = null;
            Vector2? maxIntersection = null;

            for(int i = 0; i < verticesA.Length; i++) {
                Line lineA = new Line(verticesA[i], verticesA[(i+1) % verticesA.Length]);
                for(int j = 0; j < verticesB.Length; j++) {
                    Line lineB = new Line(verticesB[j], verticesB[(j+1) % verticesB.Length]);

                    Vector2? intersectionPoint = lineA.Intersect(lineB, true, true);

                    if(intersectionPoint.HasValue) {
                        var intersection = new Intersection {
                            Position = intersectionPoint.Value, VertexIndexA = i, VertexIndexB = j
                        };
                        Line lineACopy = lineA;
                        Line.LayFlat(lineB, ref lineACopy, out _);
                        int signDelta = (Math.Sign(lineACopy.To.Y) - Math.Sign(lineACopy.From.Y));
                        if(signDelta == 0) {
                            // Console.WriteLine("signDelta is zero, continuing");
                            continue;
                        }
                        intersection.Type = signDelta > 0 ? IntersectionType.In : IntersectionType.Out;
                        
                        Vector2 intersectionPointFlat = intersection.Position;
                        float lengthA = Line.LayFlat(lineA, ref intersectionPointFlat, out _).X;
                        intersection.OrderA = i + intersectionPointFlat.X / lengthA;
                        
                        intersectionPointFlat = intersection.Position;
                        float lengthB = Line.LayFlat(lineB, ref intersectionPointFlat, out _).X;
                        intersection.OrderB = j + intersectionPointFlat.X / lengthB;

                        if((int) intersection.OrderA != i || (int) intersection.OrderB != j) continue;
                        if(intersection.OrderA == i || intersection.OrderB == j) continue;
                        
                        minIntersection = Vector2.Min(minIntersection ?? intersectionPoint.Value, intersectionPoint.Value);
                        maxIntersection = Vector2.Max(maxIntersection ?? intersectionPoint.Value, intersectionPoint.Value);
                        
                        if(intersection.Type == IntersectionType.In) ins++;
                        else outs++;
                        
                        Intersections.Add(intersection);
                    }
                }
            }

            if(minIntersection.HasValue && (maxIntersection.Value - minIntersection.Value).Length() <= 0) {
                // Console.WriteLine("Single point, exiting");
                return null;
            }

            if(ins != outs) {
                Console.WriteLine("Mismatching in-intersection and out-intersection count, exiting");
                return null;
            }

            if(Intersections.Count < 2) return null;
            
            List<List<PolygonIntersectionVertex>> polygons = new List<List<PolygonIntersectionVertex>>();

            while(Intersections.Count >= 2) {
                List<PolygonIntersectionVertex> polygon = new List<PolygonIntersectionVertex>();
                // polygons.Add(polygon);
                
                Intersection firstIntersection = Intersections[0];

                Intersection current = firstIntersection;
                bool polygonComplete = false;
                while(!polygonComplete) {

                    if(current.Type == IntersectionType.Out) {
                        //traverse polygon A

                        int startIndex = current.VertexIndexA;
                        int i = startIndex;
                        bool doneFullLoop = false;
                        do {
                            Intersection nextIntersection = Intersections.Where(intersection =>
                                    intersection.VertexIndexA == i
                                    && intersection.Type != current.Type
                                    && (intersection != current
                                        && (intersection.VertexIndexA != current.VertexIndexA
                                            || intersection.OrderA >= current.OrderA))
                                )
                                .OrderBy(intersection => intersection.VertexIndexA)
                                .ThenBy(intersection => intersection.OrderA)
                                .FirstOrDefault();

                            if(nextIntersection == firstIntersection) {
                                //Completed the polygon
                                polygon.Add(new PolygonIntersectionVertex(nextIntersection));

                                Intersections.Remove(nextIntersection);

                                polygonComplete = true;

                                break;
                            } else if(nextIntersection != default) {
                                FoldUtil.Assert(nextIntersection.Type != current.Type,
                                    "Found two out-type intersections in a row!!!");
                                polygon.Add(new PolygonIntersectionVertex(nextIntersection));
                                //Wrap up and get ready to switch to traversing polygon B

                                Intersections.Remove(nextIntersection);

                                current = nextIntersection;
                                break;
                            } else {
                                //No intersection found ahead, instead add the next vertex and repeat (until an intersection is found)
                                i = (i + 1) % verticesA.Length;
                                polygon.Add(new PolygonIntersectionVertex(verticesA[i], i, -1));
                                if(i == startIndex) doneFullLoop = true;
                            }

                        } while(!doneFullLoop
                                // || i != ((startIndex + 1) % verticesA.Length)
                                );
                        
                        FoldUtil.Assert(!doneFullLoop, "Did a full loop and found no matching intersections (A)");
                    } else {
                        //traverse polygon B
                        
                        int startIndex = current.VertexIndexB;
                        int i = startIndex;
                        bool doneFullLoop = false;
                        do {
                            Intersection nextIntersection = Intersections.Where(intersection =>
                                    intersection.VertexIndexB == i
                                    && intersection.Type != current.Type
                                    && (intersection != current
                                        && (intersection.VertexIndexB != current.VertexIndexB
                                            || intersection.OrderB >= current.OrderB))
                                )
                                .OrderBy(intersection => intersection.VertexIndexB)
                                .ThenBy(intersection => intersection.OrderB)
                                .FirstOrDefault();

                            if(nextIntersection == firstIntersection) {
                                //Completed the polygon
                                polygon.Add(new PolygonIntersectionVertex(nextIntersection));
                                
                                Intersections.Remove(nextIntersection);

                                polygonComplete = true;

                                break;
                            } else if(nextIntersection != default) {
                                FoldUtil.Assert(nextIntersection.Type != current.Type,
                                    "Found two out-type intersections in a row!!!");
                                polygon.Add(new PolygonIntersectionVertex(nextIntersection));
                                //Wrap up and get ready to switch to traversing polygon A

                                Intersections.Remove(nextIntersection);
                                
                                current = nextIntersection;
                                break;
                            } else {
                                //No intersection found ahead, instead add the next vertex and repeat (until an intersection is found)
                                i = (i + 1) % verticesB.Length;
                                polygon.Add(new PolygonIntersectionVertex(verticesB[i], -1, i));
                                if(i == startIndex) doneFullLoop = true;
                            }

                        } while(!doneFullLoop
                                // || i != ((startIndex + 1) % verticesB.Length)
                                );

                        FoldUtil.Assert(!doneFullLoop, "Did a full loop and found no matching intersections (B)");
                    }
                }
                polygons.Add(polygon);
            }
            
            PolygonIntersectionVertex[][] asArrays = new PolygonIntersectionVertex[polygons.Count][];
            for(int i = 0; i < asArrays.Length; i++) {
                asArrays[i] = polygons[i].ToArray();
            }

            return asArrays;
        }

        [Pure]
        public static float ComputeLargestCrossSection(PolygonIntersectionVertex[] polygon, Vector2 axisNormal) {
            Complex axisNormalComplex = axisNormal;

            float largestCrossSection = 0;
            
            Vector2[] vertices = new Vector2[polygon.Length];
            for(int i = 0; i < vertices.Length; i++) {
                vertices[i] = (Complex) polygon[i].Position / axisNormalComplex;
            }

            for(int i = 0; i < vertices.Length; i++) {
                Vector2 vertex = vertices[i];
                Vector2 prevVertex = vertices[i - 1 >= 0 ? i - 1 : vertices.Length - 1];
                Vector2 nextVertex = vertices[(i+1) % vertices.Length];
                
                Vector2 vertexNormal = ((new Line(prevVertex, vertex).Normal + new Line(vertex, nextVertex).Normal) / 2).Normalized();
                
                float minX = vertex.X;
                float maxX = minX;
                for(int j = 0; j < vertices.Length; j++) {
                    if(j == i - 1 || (i == 0 && j == vertices.Length - 1) || j == i) {
                        //Don't check adjacent faces
                        continue;
                    }
                    
                    Line line = new Line(vertices[j], vertices[(j+1) % vertices.Length]);
                    Vector2? intersection = line.Intersect(new Line(vertex, vertex + Vector2.UnitX), true, false);
                    //TODO verify that the line this is checking against is facing away from this vertex (for concave shapes)

                    if(intersection.HasValue) {
                        if(Vector2.Dot(intersection.Value, vertexNormal) < 0) {
                            minX = Math.Min(minX, intersection.Value.X);
                            maxX = Math.Max(maxX, intersection.Value.X);
                        }
                    }
                }

                largestCrossSection = Math.Max(largestCrossSection, maxX - minX);
            }

            return largestCrossSection;
        }

        [Pure]
        public static Vector2 ComputeHighestPoint(PolygonIntersectionVertex[] polygon, Vector2 axisNormal) {
            Complex axisNormalComplex = axisNormal;
            
            Vector2[] vertices = new Vector2[polygon.Length];
            for(int i = 0; i < vertices.Length; i++) {
                vertices[i] = (Complex) polygon[i].Position / axisNormalComplex;
            }

            float maxX = 0;
            float minY = 0;
            float maxY = 0;
            
            for(int i = 0; i < vertices.Length; i++) {
                (float x, float y) = vertices[i];

                if(i == 0) {
                    maxX = x;
                    minY = y;
                    maxY = y;
                } else {
                    if(x >= maxX) {
                        // minY = maxY = y;
                        if(Math.Abs(x - maxX) < 0.0000001) {
                            minY = Math.Min(minY, y);
                            maxY = Math.Max(maxY, y);
                        } else {
                            minY = y;
                            maxY = minY;
                        }
                        maxX = x;
                    }
                }
            }
            
            return ((Complex)new Vector2(maxX, (minY + maxY) / 2)) * axisNormalComplex;
        }

        internal struct Intersection {
            public Vector2 Position;
            public IntersectionType Type;
            public int VertexIndexA;
            public int VertexIndexB;

            public float OrderA;
            public float OrderB;

            public bool Equals(Intersection other) {
                return VertexIndexA == other.VertexIndexA
                       && VertexIndexB == other.VertexIndexB
                       && OrderA.Equals(other.OrderA)
                       && OrderB.Equals(other.OrderB);
            }

            public override bool Equals(object obj) {
                return obj is Intersection other && Equals(other);
            }

            public override int GetHashCode() {
                unchecked {
                    int hashCode = VertexIndexA;
                    hashCode = (hashCode * 397) ^ VertexIndexB;
                    hashCode = (hashCode * 397) ^ OrderA.GetHashCode();
                    hashCode = (hashCode * 397) ^ OrderB.GetHashCode();
                    return hashCode;
                }
            }

            public static bool operator ==(Intersection a, Intersection b) {
                return a.Equals(b);
            } 

            public static bool operator !=(Intersection a, Intersection b) {
                return !a.Equals(b);
            } 
        }

        internal enum IntersectionType {
            In, Out
        }
        
        public struct PolygonIntersectionVertex {
            public Vector2 Position;
            public int VertexIndexA;
            public int VertexIndexB;

            public bool IsIntersection => IsFromA && IsFromB;
            public bool IsFromA => VertexIndexA != -1;
            public bool IsFromB => VertexIndexB != -1;

            internal PolygonIntersectionVertex(Intersection intersection) {
                this.Position = intersection.Position;
                this.VertexIndexA = intersection.VertexIndexA;
                this.VertexIndexB = intersection.VertexIndexB;
            }

            public PolygonIntersectionVertex(Vector2 position, int vertexIndexA = -1, int vertexIndexB = -1) {
                Position = position;
                VertexIndexA = vertexIndexA;
                VertexIndexB = vertexIndexB;
            }
        }
    }
}