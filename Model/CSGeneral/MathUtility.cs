using System;
using System.Collections;
using System.Collections.Generic;

namespace CSGeneral
   {
   /// <summary>
   /// Various math utilities.
   /// </summary>
   public class MathUtility
      {
      //------------------------------------------------
      // Returns true if specified value is 'missing'
      // -----------------------------------------------
      public static double MissingValue
         {
         get { return 999999; }
         }

      //-------------------------------------------------------------------------
      //
      //-------------------------------------------------------------------------
      public static bool FloatsAreEqual(double value1, double value2)
         {
         return FloatsAreEqual(value1, value2, 0.00001);
         }
      //-------------------------------------------------------------------------
      //
      //-------------------------------------------------------------------------
      public static bool FloatsAreEqual(double value1, double value2, double tolerance)
         {
         return (Math.Abs(value1 - value2) < tolerance);
         }
      //-------------------------------------------------------------------------
      //
      //-------------------------------------------------------------------------
      public static double[] Multiply(double[] value1, double[] value2)
         {
         double[] results = new double[value1.Length];
         if (value1.Length == value2.Length)
            {
            results = new double[value1.Length];
            for (int iIndex = 0; iIndex < value1.Length; iIndex++)
               {
               if (value1[iIndex] == MissingValue || value2[iIndex] == MissingValue)
                  results[iIndex] = MissingValue;
               else
                  results[iIndex] = (value1[iIndex] * value2[iIndex]);
               }
            }
         return results;
         }
      //-------------------------------------------------------------------------
      //
      //-------------------------------------------------------------------------
      public static double[] Multiply_Value(double[] value1, double value2)
         {
         double[] results = null;
         results = new double[value1.Length];
         for (int iIndex = 0; iIndex < value1.Length; iIndex++)
            {
            if (value1[iIndex] == MissingValue)
               results[iIndex] = MissingValue;
            else
               results[iIndex] = (value1[iIndex] * value2);
            }
         return results;
         }
      //-------------------------------------------------------------------------
      //
      //-------------------------------------------------------------------------
      public static double[] Divide(double[] value1, double[] value2)
         {
         double[] results = null;
         if (value1.Length == value2.Length)
            {
            results = new double[value1.Length];
            for (int iIndex = 0; iIndex < value1.Length; iIndex++)
               {
               if (value1[iIndex] == MissingValue || value2[iIndex] == MissingValue)
                  results[iIndex] = MissingValue;
               else if (value2[iIndex] != 0)
                  {
                  results[iIndex] = (value1[iIndex] / value2[iIndex]);
                  }
               else
                  {
                  results[iIndex] = value1[iIndex];
                  }
               }
            }
         return results;
         }
      //-------------------------------------------------------------------------
      //
      //-------------------------------------------------------------------------
      public static double[] Divide_Value(double[] value1, double value2)
         {
         double[] results = new double[value1.Length];
         //Avoid divide by zero problems
         if (value2 != 0)
            {
            for (int iIndex = 0; iIndex < value1.Length; iIndex++)
               {
               if (value1[iIndex] == MissingValue)
                  results[iIndex] = MissingValue;
               else
                  results[iIndex] = (value1[iIndex] / value2);
               }
            }
         else
            {
            results = value1;
            }
         return results;
         }


      //-------------------------------------------------------------------------
      //
      //-------------------------------------------------------------------------
      public static double[] Add_Value(double[] value1, double value2)
         {
         double[] results = new double[value1.Length];
         for (int iIndex = 0; iIndex < value1.Length; iIndex++)
            {
            if (value1[iIndex] == MissingValue)
               results[iIndex] = MissingValue;
            else
               results[iIndex] = (value1[iIndex] + value2);
            }
         return results;
         }

      //-------------------------------------------------------------------------
      //
      //-------------------------------------------------------------------------
      public static double[] Subtract_Value(double[] value1, double value2)
         {
         double[] results = new double[value1.Length];
         for (int iIndex = 0; iIndex < value1.Length; iIndex++)
            {
            if (value1[iIndex] == MissingValue)
               results[iIndex] = MissingValue;
            else
               results[iIndex] = (value1[iIndex] - value2);
            }
         return results;
         }
      //-------------------------------------------------------------------------
      // Sum an array of numbers 
      //-------------------------------------------------------------------------
      public static double Sum(IEnumerable Values)
         {
         return Sum(Values, 0, 0, 0.0);
         }

      //-------------------------------------------------------------------------
      // Sum an array of numbers starting at startIndex up to (but not including) endIndex
      // beginning with an initial value
      //-------------------------------------------------------------------------
      public static double Sum(IEnumerable Values, int iStartIndex, int iEndIndex,
                              double dInitialValue)
         {
         double result = dInitialValue;
         if (iStartIndex < 0)
            {
            throw new Exception("MathUtility.Sum: End index or start index is out of range");
            }
         int iIndex = 0;
         foreach (double Value in Values)
            {
            if ((iStartIndex == 0 && iEndIndex == 0) ||
               (iIndex >= iStartIndex && iIndex < iEndIndex) && Value != MissingValue)
               result += Value;
            iIndex++;
            }

         return result;
         }
      //-------------------------------------------------------------------------
      //Linearly interpolates a value y for a given value x and a given
      //set of xy co-ordinates.
      //When x lies outside the x range_of, y is set to the boundary condition.
      //Returns true for Did_interpolate if interpolation was necessary.
      //-------------------------------------------------------------------------
      public static double LinearInterpReal(double dX, double[] dXCoordinate, double[] dYCoordinate, ref bool bDidInterpolate)
         {
         bDidInterpolate = false;
         if (dXCoordinate == null || dYCoordinate == null)
            return 0;
         //find where x lies in the x coordinate
         if (dXCoordinate.Length == 0 || dYCoordinate.Length == 0 || dXCoordinate.Length != dYCoordinate.Length)
            {
            throw new Exception("MathUtility.LinearInterpReal: Lengths of passed in arrays are incorrect");
            }

         for (int iIndex = 0; iIndex < dXCoordinate.Length; iIndex++)
            {
            if (dX <= dXCoordinate[iIndex])
               {
               //Chcek to see if dX is exactly equal to dXCoordinate[iIndex]
               //If so then don't calcuate dY.  This was added to remove roundoff error.
               if (dX == dXCoordinate[iIndex])
                  {
                  bDidInterpolate = false;
                  return dYCoordinate[iIndex];
                  }
               //Found position
               else if (iIndex == 0)
                  {
                  bDidInterpolate = true;
                  return dYCoordinate[iIndex];
                  }
               else
                  {
                  //interpolate - y = mx+c
                  if ((dXCoordinate[iIndex] - dXCoordinate[iIndex - 1]) == 0)
                     {
                     bDidInterpolate = true;
                     return dYCoordinate[iIndex - 1];
                     }
                  else
                     {
                     bDidInterpolate = true;
                     return ((dYCoordinate[iIndex] - dYCoordinate[iIndex - 1]) / (dXCoordinate[iIndex] - dXCoordinate[iIndex - 1]) * (dX - dXCoordinate[iIndex - 1]) + dYCoordinate[iIndex - 1]);
                     }
                  }
               }
            else if (iIndex == (dXCoordinate.Length - 1))
               {
               bDidInterpolate = true;
               return dYCoordinate[iIndex];
               }
            }// END OF FOR LOOP
         return 0.0;
         }

      static public double Constrain(double dLowerLimit, double dUpperLimit, double dValue)
         {
         double dConstrainedValue = 0.0;
         dConstrainedValue = Math.Min(dUpperLimit, Math.Max(dLowerLimit, dValue));
         return dConstrainedValue;
         }

      static public double Round(double Value, int NumDecPlaces)
         // rounds properly rather than the math.round function.
         // e.g. 3.4 becomes 3.0
         //      3.5 becomes 4.0
         {
         double Multiplier = Math.Pow(10.0, NumDecPlaces);  // gives 1 or 10 or 100 for decplaces=0, 1, or 2 etc
         Value = Math.Truncate(Value * Multiplier + 0.5);
         return Value / Multiplier;
         }

      static public double[] Round(double[] Values, int NumDecPlaces)
         // rounds properly rather than the math.round function.
         // e.g. 3.4 becomes 3.0
         //      3.5 becomes 4.0
         {
         double ExtraBit = 1.0 / Math.Pow(10.0, NumDecPlaces);  // gives 0.1 or 0.01 or 0.001 etc
         for (int i = 0; i != Values.Length; i++)
            Values[i] = Round(Values[i], NumDecPlaces);
         return Values;
         }


      // ---------------------------------------------
      // Reverse the contents of the specified array.
      // ---------------------------------------------
      static public double[] Reverse(double[] Values)
         {
         double[] ReturnValues = new double[Values.Length];

         int Index = 0;
         for (int Layer = Values.Length - 1; Layer >= 0; Layer--)
            {
            ReturnValues[Index] = Values[Layer];
            Index++;
            }
         return ReturnValues;
         }

      static public bool ValuesInArray(double[] Values)
         {
         foreach (double Value in Values)
            {
            if (Value != MathUtility.MissingValue)
               return true;
            }
         return false;
         }

      // --------------------------------------------------
      // Convert an array of strings to an array of doubles
      // --------------------------------------------------
      static public double[] StringsToDoubles(string[] Values)
         {
         double[] ReturnValues = new double[Values.Length];

         for (int Index = 0; Index != Values.Length; Index++)
            ReturnValues[Index] = Convert.ToDouble(Values[Index]);
         return ReturnValues;
         }

      static public double[] ProbabilityDistribution(int NumPoints, bool Exceed)
         {
         double[] Probability = new double[NumPoints];

         for (int x = 1; x <= NumPoints; x++)
            Probability[x - 1] = (x - 0.5) / NumPoints * 100;

         if (Exceed)
            Array.Reverse(Probability);
         return Probability;
         }


      public class RegrStats
         {
         public int n;
         public double m;
         public double c;
         public double SEslope;
         public double SEcoeff;
         public double R2;
         public double ADJR2;
         public double R2YX;
         public double VarRatio;
         public double RMSD;
         };

      static public RegrStats CalcRegressionStats(List<double> X, List<double> Y)
         {
         // ------------------------------------------------------------------
         //    Calculate regression stats.   
         // ------------------------------------------------------------------
         RegrStats stats = new RegrStats();
         double SumX = 0;
         double SumY = 0;
         double SumXY = 0;
         double SumX2 = 0;
         double SumY2 = 0;
         double SumXYdiff2 = 0;
         double CSSX, CSSXY;
         double Xbar, Ybar;
         double TSS, TSSM;
         double REGSS, REGSSM;
         double RESIDSS, RESIDSSM;
         double S2;

         stats.n = 0;
         stats.m = 0.0;
         stats.c = 0.0;
         stats.SEslope = 0.0;
         stats.SEcoeff = 0.0;
         stats.R2 = 0.0;
         stats.ADJR2 = 0.0;
         stats.R2YX = 0.0;
         stats.VarRatio = 0.0;
         stats.RMSD = 0.0;
         int Num_points = X.Count;

         if (Num_points > 1)
            {

            for (int Point = 0;
                 Point < Num_points;
                 Point++)
               {
               SumX = SumX + X[Point];
               SumX2 = SumX2 + X[Point] * X[Point];       // SS for X
               SumY = SumY + Y[Point];
               SumY2 = SumY2 + Y[Point] * Y[Point];       // SS for y
               SumXY = SumXY + X[Point] * Y[Point];       // SS for products
               }
            Xbar = SumX / Num_points;
            Ybar = SumY / Num_points;

            CSSXY = SumXY - SumX * SumY / Num_points;     // Corrected SS for products
            CSSX = SumX2 - SumX * SumX / Num_points;      // Corrected SS for X
            stats.n = Num_points;
            stats.m = CSSXY / CSSX;                             // Calculate slope
            stats.c = Ybar - stats.m * Xbar;                          // Calculate intercept

            TSS = SumY2 - SumY * SumY / Num_points;       // Corrected SS for Y = Sum((y-ybar)^2)
            TSSM = TSS / (Num_points - 1);                // Total mean SS
            REGSS = stats.m * CSSXY;                            // SS due to regression = Sum((yest-ybar)^2)
            REGSSM = REGSS;                               // Regression mean SS
            RESIDSS = TSS - REGSS;                        // SS about the regression = Sum((y-yest)^2)

            if (Num_points > 2)                           // MUST HAVE MORE THAN TWO POINTS FOR REG
               RESIDSSM = RESIDSS / (Num_points - 2);     // Residual mean SS, variance of residual
            else
               RESIDSSM = 0.0;

            stats.RMSD = Math.Sqrt(RESIDSSM);                        // Root mean square deviation
            stats.VarRatio = REGSSM / RESIDSSM;                  // Variance ratio - for F test (1,n-2)
            stats.R2 = 1.0 - (RESIDSS / TSS);                   // Unadjusted R2 calculated from SS
            stats.ADJR2 = 1.0 - (RESIDSSM / TSSM);              // Adjusted R2 calculated from mean SS
            if (stats.ADJR2 < 0.0)
               stats.ADJR2 = 0.0;
            S2 = RESIDSSM;                                // Resid. MSS is estimate of variance
            // about the regression
            stats.SEslope = Math.Sqrt(S2) / Math.Sqrt(CSSX);              // Standard errors estimated from S2 & CSSX
            stats.SEcoeff = Math.Sqrt(S2) * Math.Sqrt(SumX2 / (Num_points * CSSX));

            // Statistical parameters of Butler, Mayer and Silburn

            stats.R2YX = 1.0 - (SumXYdiff2 / TSS);              // If you are on the 1:1 line then R2YX=1

            // If R2YX is -ve then the 1:1 line is a worse fit than the line y=ybar

            //      MeanAbsError = SumXYdiff / Num_points;
            //      MeanAbsPerError = SumXYDiffPer / Num_points;  // very dangerous when y is low
            // could use MeanAbsError over mean
            }
         return stats;
         }


      }
   }
