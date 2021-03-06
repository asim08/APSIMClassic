using System;
using System.Collections.Generic;
using System.Text;

[Description("This Function calculates a mean daily temperature from Max and Min weighted toward Max according to the specified MaximumTemperatureWeighting factor.  This is then passed into the XY matrix as the x property and the function returns the y value")]
public class WeightedTemperatureFunction : Function
{
    #region Class Data Members
    [Link]
    private XYPairs XYPairs = null;   // Temperature effect on Growth Interpolation Set

    [Param]
    private double MaximumTemperatureWeighting = 0.0;

    //[Input]
    //public NewMetType MetData;

    #endregion

    [Output]
    [Units("0-1")]
    public override double Value
    {
        get
        {
            double Tav = MaximumTemperatureWeighting * MetData.MaxT + (1 - MaximumTemperatureWeighting) * MetData.MinT;
            return XYPairs.ValueIndexed(Tav);
        }
    }

}
   
