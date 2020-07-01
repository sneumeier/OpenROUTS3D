using SumoImportPolygon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace SumoImportPolygon
{

    public class SceneAreaDimension
    {

        private double xmin = Double.MaxValue;
        private double ymin = Double.MaxValue;
        private double xmax = Double.MinValue;
        private double ymax = Double.MinValue;

        public SceneAreaDimension()
        {

        }

        public void CheckVector2D(Vector2 vector)
        {

            if (this.xmin >= vector.x)
            {
                this.xmin = vector.x;
            }

            if (this.ymin >= vector.y)
            {
                this.ymin = vector.y;
            }

            if (this.xmax <= vector.x)
            {
                this.xmax = vector.x;
            }

            if (this.ymax <= vector.y)
            {
                this.ymax = vector.y;
            }

        }

        public Vector2[] CreateQuad()
        {
            Vector2[] vertices = new Vector2[4];

            //
            vertices[0] = new Vector2((float)this.xmin, (float)this.ymin);
            vertices[1] = new Vector2((float)this.xmax, (float)this.ymin);
            vertices[2] = new Vector2((float)this.xmax, (float)this.ymax);
            vertices[3] = new Vector2((float)this.xmin, (float)this.ymax);

            // checkig values because of bounds for meshes
            for (int i = 0; i < vertices.Length; i++)
            {

                if (Double.IsInfinity(vertices[i].x) || Double.IsInfinity(vertices[i].x))
                {
                    vertices[i].x = 0.0f;
                }
                if (Double.IsInfinity(vertices[i].y) || Double.IsInfinity(vertices[i].y))
                {
                    vertices[i].y = 0.0f;
                }
                
            }



            return vertices;
        }

        public double Xmin
        {
            get
            {
                return xmin;
            }

            set
            {
                xmin = value;
            }
        }

        public double Ymin
        {
            get
            {
                return ymin;
            }

            set
            {
                ymin = value;
            }
        }
        public double Xmax
        {
            get
            {
                return xmax;
            }

            set
            {
                xmax = value;
            }
        }

        public double Ymax
        {
            get
            {
                return ymax;
            }

            set
            {
                ymax = value;
            }
        }

    }

}
