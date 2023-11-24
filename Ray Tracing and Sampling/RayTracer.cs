using System;

namespace rt
{
    class RayTracer
    {
        private Geometry[] geometries;
        private Light[] lights;

        public RayTracer(Geometry[] geometries, Light[] lights)
        {
            this.geometries = geometries;
            this.lights = lights;
        }

        private double ImageToViewPlane(int n, int imgSize, double viewPlaneSize)
        {
            return -n * viewPlaneSize / imgSize + viewPlaneSize / 2;
        }

        private Intersection FindFirstIntersection(Line ray, double minDist, double maxDist)
        {
            var intersection = Intersection.NONE;

            foreach (var geometry in geometries)
            {
                var intr = geometry.GetIntersection(ray, minDist, maxDist);

                if (!intr.Valid || !intr.Visible) continue;

                if (!intersection.Valid || !intersection.Visible)
                {
                    intersection = intr;
                }
                else if (intr.T < intersection.T)
                {
                    intersection = intr;
                }
            }

            return intersection;
        }

        private Intersection LightFindFirstIntersection(Line ray, double minDist, double maxDist)
        {
            var intersection = Intersection.NONE;

            foreach (var geometry in geometries)
            {
                if (geometry is RawCtMask) continue;

                var intr = geometry.GetIntersection(ray, minDist, maxDist);

                if (!intr.Valid || !intr.Visible) continue;

                if (!intersection.Valid || !intersection.Visible)
                {
                    intersection = intr;
                }
                else if (intr.T < intersection.T)
                {
                    intersection = intr;
                }
            }

            return intersection;
        }

        private bool IsLit(Vector point, Light light)
        {

            Line line = new Line(light.Position, point);
            var inter = LightFindFirstIntersection(line, 0, (point - light.Position).Length() + 10);
            if (!inter.Visible || !inter.Valid) return false;

            Vector firstIntersection = inter.Position;

            return Math.Abs(firstIntersection.X - point.X) < 0.001 &&
                Math.Abs(firstIntersection.Y - point.Y) < 0.001 &&
                Math.Abs(firstIntersection.Z - point.Z) < 0.001;
        }

        public void Render(Camera camera, int width, int height, string filename)
        {
            var background = new Color(0.2, 0.2, 0.2, 1.0);

            var image = new Image(width, height);
            var viewPararel = camera.Direction ^ camera.Up;

            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                {
                    Vector dist = camera.Position + camera.Direction * camera.ViewPlaneDistance;
                    Vector right = viewPararel * ImageToViewPlane(i, width, camera.ViewPlaneWidth);
                    Vector up = camera.Up * ImageToViewPlane(j, height, camera.ViewPlaneHeight);

                    Vector pixelPoint = dist + right + up;

                    Line line = new Line(camera.Position, pixelPoint);

                    Intersection intersection = FindFirstIntersection(line, camera.FrontPlaneDistance, camera.BackPlaneDistance);

                    if (intersection.Valid && intersection.Visible)
                    {
                        // LAB 1
                        /*Color color = intersection.Geometry.Color;*/

                        // LAB 3
                        Color pixelColor = new Color(0, 0, 0, 1);
                        Material material = intersection.Material;

                        foreach (var light in lights)
                        {
                            Color color = new Color(0, 0, 0, 1);
                            Color ambient = material.Ambient * light.Ambient;
                            color += ambient;

                            Vector t = (light.Position - intersection.Position).Normalize();
                            if (IsLit(intersection.Position, light))
                            {
                                if (intersection.Normal * t > 0)
                                {
                                    Color diffuse = material.Diffuse * light.Diffuse * (intersection.Normal * t);
                                    color += diffuse;
                                }

                                Vector e = (camera.Position - intersection.Position).Normalize();
                                Vector r = intersection.Normal * (intersection.Normal * t) * 2 - t;

                                if (e * r > 0)
                                {
                                    Color specular = material.Specular * light.Specular * Math.Pow(e * r, material.Shininess);
                                    color += specular;
                                }


                                color *= light.Intensity;
                            }

                            pixelColor += color;
                        }



                        image.SetPixel(i, j, pixelColor);

                    }
                    else
                    {
                        image.SetPixel(i, j, background);
                    }


                }
            }

            image.Store(filename);
        }
    }
}