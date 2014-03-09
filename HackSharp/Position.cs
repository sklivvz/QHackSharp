using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace HackSharp
{
    public struct Position : IEquatable<Position>, IEqualityComparer<Position>
    {
        public readonly int X;
        public readonly int Y;

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Position Empty = new Position(-1,-1);

        public bool Equals(Position other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != GetType()) return false;
            var o = (Position)obj;
            return X.Equals(o.X) && Y.Equals(o.Y);
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() >> 3 & 27 + Y.GetHashCode();
        }

        public bool Equals(Position x, Position y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(Position obj)
        {
            return obj.GetHashCode();
        }

        public Position North()
        {
            return new Position(X, Y - 1);
        }

        public Position South()
        {
            return new Position(X, Y + 1);
        }
        public Position East()
        {
            return new Position(X + 1, Y);
        }

        public Position West()
        {
            return new Position(X - 1, Y);
        }
    }
}
