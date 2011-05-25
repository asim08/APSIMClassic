using System;
using System.Collections.Generic;
using System.Text;
using CSGeneral;

public class Nodule : BaseOrgan, BelowGround
{
    [Link]
    Plant Plant = null;

    #region Class Data Members
    private SowPlant2Type SowingInfo = null;
    public double RespiredWt = 0;
    public double PropFixationDemand = 0;
    public double FixedN = 0;
    private double PotentialDMAllocation = 0;
    #endregion
    
    public override double DMDemand
    {
        get
        {
            /// This DM demand is for DM to grow the structure of nodules.
            double StructureDemand = 0; 
            Arbitrator A = Plant.Children["Arbitrator"] as Arbitrator;
            Function PartitionFraction = Children["PartitionFraction"] as Function;
            StructureDemand = A.DMSupply * PartitionFraction.Value;
            return StructureDemand;
        }
    }   
    public override double DMPotentialAllocation
    {
        set
        {
            if (DMDemand == 0)
                if (value < 0.000000000001) { }//All OK
                else
                    throw new Exception("Invalid allocation of potential DM in " + Name);
            PotentialDMAllocation = value;
        }
    }    
    [Output]
    public override double DMAllocation
    {
        set
        {
           // allocating structural DM to nodule organ
            Live.StructuralWt += value; 
        }
    }
    [Output]
    public override double DMRespired
    {
        set
        //This is the DM that is consumed to fix N.  this is calculated by the arbitrator and passed to the nodule to report
        {
            RespiredWt = value;
        }
    }
    public override double NFixationSupply
    {
        get
        {
            Function MaximumSpecificFixation = Children["MaximumSpecificFixation"] as Function;
            Function FT = Children["FT"] as Function;
            Function FW = Children["FW"] as Function;
            Arbitrator A = Plant.Children["Arbitrator"] as Arbitrator;
            Function PartitionFraction = Children["PartitionFraction"] as Function;
            double MaximumPropFixation = 0.2; //Fixme.  Need to decide if this is to stay and if so put in IDE
            Double MaximumFixation = MaximumPropFixation * A.DMSupply * PartitionFraction.Value;
            return Math.Max(MaximumSpecificFixation.Value * FT.Value * FW.Value, MaximumFixation);
        }
    }
    [Output]
    public double NFixation
    { 
        get
        {
            return FixedN;
        }
    }
    public override double NFixed
    {
        set
        {
            FixedN = value;
        }
    }
    public override double NDemand
    {
        get
        {
            //Calculate N demand based on how much N is needed to grow nodule wt
            Function MaximumNConc = Children["MaximumNConc"] as Function;
            double NDeficit = Math.Max(0.0, MaximumNConc.Value * (Live.Wt + PotentialDMAllocation) - Live.N);
            return NDeficit;
        }
    }
    public override double NAllocation
    {
        set
        {
            Live.StructuralN += value;
        }
    }
    [Output]
    public double RespiredWtFixation
    {
        get
        {
            return RespiredWt;
        }
    }
    public override double MaxNconc
    {
        get
        {
            Function MaximumNConc = Children["MaximumNConc"] as Function;
            return MaximumNConc.Value;
        }
    }
    public override double MinNconc
    {
        get
        {
            Function MinimumNConc = Children["MinimumNConc"] as Function;
            return MinimumNConc.Value;
        }
    }
 }
