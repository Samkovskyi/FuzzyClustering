﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace XCluster.Model
{
    public class ClusterPoint
    {
        /// <summary>
        /// Gets or sets X-coord of the point
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Gets or sets Y-coord of the point
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// Gets or sets the RGB color of the point
        /// </summary>
        public double[] PixelColor { get; set; }

        /// <summary>
        /// Gets or sets the original RGB color of the point
        /// </summary>
        public double[] OriginalPixelColor { get; set; }


        /// <summary>
        /// Gets or sets cluster index, the strongest membership value to a cluster, used for defuzzification
        /// </summary>
        public double ClusterIndex { get; set; }

        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="x">X-coord</param>
        /// <param name="y">Y-coord</param>
        /// <param name="z">PixelColor</param>
        /// <param name="z">OriginalPixelColor</param>
        /// <param name="z">ClusterIndex</param>
        public ClusterPoint(double x, double y, double[] col)
        {
            this.X = x;
            this.Y = y;
            this.PixelColor = col;
            this.OriginalPixelColor = col;
            this.ClusterIndex = -1;
        }
        
        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="x">X-coord</param>
        /// <param name="y">Y-coord</param>
        public ClusterPoint(double x, double y, double[] z, object tag)
        {
            this.X = x;
            this.Y = y;
            this.PixelColor = z;
            this.ClusterIndex = -1;
        }
    }
}