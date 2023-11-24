using System;


namespace rt
{
    public class Ellipsoid : Geometry
    {
        private Vector Center { get; }
        private Vector SemiAxesLength { get; }
        private double Radius { get; }
        
        
        public Ellipsoid(Vector center, Vector semiAxesLength, double radius, Material material, Color color) : base(material, color)
        {
            Center = center;
            SemiAxesLength = semiAxesLength;
            Radius = radius;
        }

        public Ellipsoid(Vector center, Vector semiAxesLength, double radius, Color color) : base(color)
        {
            Center = center;
            SemiAxesLength = semiAxesLength;
            Radius = radius;
        }

        public override Intersection GetIntersection(Line line, double minDist, double maxDist)
        {
            Vector A = SemiAxesLength;
            double R = Radius;
            Vector Dx = line.Dx;
            Vector x0 = line.X0;
            Vector C = Center;

            double a = divideVector(Dx, A).Length2();
            double b = 2 * (divideVector(Dx, A) * divideVector(x0 - C, A));
            double c = divideVector(x0 - C, A).Length2() - R * R;

            double delta = b * b - 4 * a * c;

            if (delta < 0)
            {
                return Intersection.NONE;
            }

            if (delta == 0)
            {
                double t = -b / (2 * a);
                Vector pos = line.CoordinateToPosition(t);
                Vector norm = new Vector(2 * (pos.X - C.X) / (A.X * A.X), 2 * (pos.Y - C.Y) / (A.Y * A.Y), 2 * (pos.Z - C.Z) / (A.Z * A.Z))
                    .Normalize();

                bool visible = minDist <= t && t <= maxDist;

                return new Intersection(true, visible, this, line, t, norm, this.Material, this.Color);
            }

            double t1 = (-b - Math.Sqrt(delta)) / (2 * a);
            double t2 = (-b + Math.Sqrt(delta)) / (2 * a);

            bool t1In = minDist <= t1 && t1 <= maxDist;
            bool t2In = minDist <= t2 && t2 <= maxDist;

            if (!t1In && !t2In)
            {
                return new Intersection();
            }

            if (t2In && !t1In)
            {
                Vector pos = line.CoordinateToPosition(t2);
                Vector norm = new Vector(2 * (pos.X - C.X) / (A.X * A.X), 2 * (pos.Y - C.Y) / (A.Y * A.Y), 2 * (pos.Z - C.Z) / (A.Z * A.Z))
                    .Normalize();

                return new Intersection(true, true, this, line, t2, norm, this.Material, this.Color);
            }

            Vector pos_d = line.CoordinateToPosition(t1);
            Vector norm_d = new Vector(2 * (pos_d.X - C.X) / (A.X * A.X), 2 * (pos_d.Y - C.Y) / (A.Y * A.Y), 2 * (pos_d.Z - C.Z) / (A.Z * A.Z))
                .Normalize();

            return new Intersection(true, true, this, line, t1, norm_d, this.Material, this.Color);
        }

        private Vector divideVector(Vector v1, Vector v2)
        {
            return new Vector(v1.X / v2.X, v1.Y / v2.Y, v1.Z / v2.Z);
        }


    }
}

