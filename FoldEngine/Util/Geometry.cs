using System.Diagnostics;
using System.Security.Cryptography;
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
            Vector2 flat = LayFlat(this, ref other, out Complex undo);

            if(otherCapped && (other.From.Y > 0) == (other.To.Y > 0)) return null;
            double xIntersect = (double) other.From.X
                                + (-(double) other.From.Y / ((double) other.To.Y - other.From.Y))
                                * ((double) other.To.X - other.From.X);
            if(thisCapped) {
                if(xIntersect < 0 || xIntersect >= flat.X) return null;
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

            return line.To;
        }

        public static Line operator +(Line line, Vector2 offset) {
            return new Line(line.From + offset, line.To + offset);
        }

        public static Line operator -(Line line, Vector2 offset) {
            return new Line(line.From - offset, line.To - offset);
        } 
    }
}