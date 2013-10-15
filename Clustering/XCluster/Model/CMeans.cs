﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using System.Drawing.Imaging;

namespace XCluster.Model
{
    class CMeans
    {
        

        /// <summary>
        /// Array containing all points used by the algorithm
        /// </summary>
        private List<ClusterPoint> Points;

        /// <summary>
        /// Array containing all clusters handled by the algorithm
        /// </summary>
        private List<ClusterCentroid> Clusters;

        /// <summary>
        /// Array containing all clusters membership value of all points to each cluster
        /// Fuzzy  rules state that the sum of the membership of a point to all clusters must be 1
        /// </summary>
        public double[,] U;

        public double[][] Data;
        private bool isConverged = false;
        //private double[][] processedImage;
        //public double[][] getProcessedImage { get { return processedImage; } }
        public bool Converged { get { return isConverged; } }

        /// <summary>
        /// Gets or sets the current fuzzyness factor
        /// </summary>
        private double Fuzzyness;

        /// <summary>
        /// Algorithm precision
        /// </summary>
        private double Eps = Math.Pow(10, -5);

        /// <summary>
        /// Gets or sets objective function
        /// </summary>
        public double J { get; set; }

   
        /// <summary>
        /// Initialize the algorithm with points and initial clusters
        /// </summary>
        /// <param name="points">The list of Points objects</param>
        /// <param name="clusters">The list of Clusters objects</param>
        /// <param name="fuzzy">The fuzzyness factor to be used, constant</param>
        /// <param name="myImage">A working image, so that the GUI working image can be updated</param>
        /// <param name="numCluster">The number of clusters requested by the user from the GUI</param>
        public CMeans(List<ClusterPoint> points, List<ClusterCentroid> clusters, float fuzzy, double[][] data)
        {
            if (points == null)
            {
                throw new ArgumentNullException("points");
            }

            if (clusters == null)
            {
                throw new ArgumentNullException("clusters");
            }


           // processedImage = myImage;
 

            this.Points = points;
            this.Clusters = clusters;
            //this.myImageHeight = myImage.Length;
            //this.myImageWidth = myImage.Rank;
            this.Data = data;

            U = new double[this.Points.Count, this.Clusters.Count];
            this.Fuzzyness = fuzzy;

            double diff;

            // Iterate through all points to create initial U matrix
            for (var i = 0; i < this.Points.Count; i++)
            {
                ClusterPoint p = this.Points[i];
                var sum = 0.0;

                for (var j = 0; j < this.Clusters.Count; j++)
                {
                    ClusterCentroid c = this.Clusters[j];
                    diff = Math.Sqrt(Math.Pow(CalculateEuclideanDistance(p, c), 2.0));
                    U[i, j] = (diff == 0) ? Eps : diff;
                    sum += U[i, j];
                }
             }

            this.RecalculateClusterMembershipValues();
        }

        /// <summary>
        /// Private constructor
        /// </summary>
        private CMeans()
        {
        }

        /// <summary>
        /// Recalculates the cluster membership values to ensure that 
        /// the sum of all membership values of a point to all clusters is 1
        /// </summary>
        private void RecalculateClusterMembershipValues() 
        {

            for (var i = 0; i < this.Points.Count; i++)
           {
               var max = 0.0;
               var min = 0.0;
               var sum = 0.0;
               var newmax = 0.0;
               var p = this.Points[i];
               //Normalize the entries
               for (var j = 0; j < this.Clusters.Count; j++)
               {
                   max = U[i, j] > max ? U[i, j] : max;
                   min = U[i, j] < min ? U[i, j] : min;
               }
               //Sets the values to the normalized values between 0 and 1
               for (var j = 0; j < this.Clusters.Count; j++)
               {
                   U[i, j] = (U[i, j] - min) / (max - min);
                   sum += U[i, j];
               }
               //Makes it so that the sum of all values is 1 
               for (var j = 0; j < this.Clusters.Count; j++)
               {
                   U[i, j] = U[i, j] / sum;
                   if (double.IsNaN(U[i, j]))
                   {
                       ///Console.WriteLine("NAN value: point({0}) cluster({1}) sum {2} newmax {3}", i, j, sum, newmax);
                       U[i, j] = 0.0;
                   }
                   //Console.WriteLine("ClusterIndex: point({0}) cluster({1}) min {2} max {3} value {4} p.ClusterIndex {5}", i, j, min, max, U[i, j], p.ClusterIndex);
                   newmax = U[i, j] > newmax ? U[i, j] : newmax;
               }
               // ClusterIndex is used to store the strongest membership value to a cluster, used for defuzzification
                p.ClusterIndex = newmax;
             }
        }

 
        /// <summary>
        /// Perform one step of the algorithm
        /// </summary>
        public void Step()
        {
            for (var c = 0; c < Clusters.Count; c++)
            {
                for (var h = 0; h < Points.Count; h++)
                {
 
                    double top;
                    top = CalculateEuclideanDistance(Points[h], Clusters[c]);
                    if (top < 1.0) top = Eps;

                    // sumTerms is the sum of distances from this data point to all clusters.
                    double sumTerms = 0.0;

                    for (var ck = 0; ck < Clusters.Count; ck++)
                    {
                        sumTerms += top / CalculateEuclideanDistance(Points[h], Clusters[ck]);
 
                    }
                    // Then the membership value can be calculated as...
                    U[h, c] = (double)(1.0 / Math.Pow(sumTerms, (2 / (this.Fuzzyness - 1)))); 
                }
            }


            this.RecalculateClusterMembershipValues();
        }

        /// <summary>
        /// Calculates Euclidean Distance distance between a point and a cluster centroid
        /// </summary>
        /// <param name="p">Point</param>
        /// <param name="c">Centroid</param>
        /// <returns>Calculated distance</returns>
        private double CalculateEuclideanDistance(ClusterPoint p, ClusterCentroid c)
        {
            var sum = 0.0;
            for (int i = 0; i < p.PixelColor.Length; i++)
            {
                sum += (p.PixelColor[i] - c.PixelColor[i])*(p.PixelColor[i] - c.PixelColor[i]);
            }
            return Math.Sqrt(sum);
        }

 
        /// <summary>
        /// Calculate the objective function
        /// </summary>
        /// <returns>The objective function as double value</returns>
        public double CalculateObjectiveFunction()
        {
            double Jk = 0.0;

            for (var i = 0; i < this.Points.Count;i++)
            {
                for (var j = 0; j < this.Clusters.Count; j++)
                {
                    Jk += Math.Pow(U[i, j], this.Fuzzyness) * Math.Pow(this.CalculateEuclideanDistance(Points[i], Clusters[j]), 2);
                }
            }
            return Jk;
        }

        /// <summary>
        /// Calculates the centroids of the clusters 
        /// </summary>
        public void CalculateClusterCentroids()
        {
            //Console.WriteLine("Cluster Centroid calculation:");
            for (var j = 0; j < this.Clusters.Count; j++)
            {
                ClusterCentroid c = this.Clusters[j];
                double l = 0.0;
                c.PixelCount = 1;
                for (int i = 0; i < c.PropertiesSum.Length; i++)
                {
                    c.PropertiesSum[i] = 0;
                }
                c.MembershipSum = 0;

                for (var i = 0; i < this.Points.Count; i++)
                {
                
                    ClusterPoint p = this.Points[i];
                    l = Math.Pow(U[i, j], this.Fuzzyness);
                    for (var k = 0;  k < c.PropertiesSum.Length; k++)
                    {
                        c.PropertiesSum[k] += l * p.PixelColor[k];
                    }
                    c.MembershipSum += l;
                    if (U[i, j] == p.ClusterIndex)
                    {
                        c.PixelCount += 1;
                    }
                }

                for (int i = 0; i < c.PixelColor.Length; i++)
                {
                    c.PixelColor[i] = c.PropertiesSum[i]/c.MembershipSum;
                }
             }
        }

        /// <summary>
        /// Perform a complete run of the algorithm until the desired accuracy is achieved.
        /// For demonstration issues, the maximum Iteration counter is set to 20.
        /// </summary>
        /// <param name="accuracy">Algorithm accuracy</param>
        /// <returns>The number of steps the algorithm needed to complete</returns>

    }
}