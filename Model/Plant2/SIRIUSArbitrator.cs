using System;
using System.Collections.Generic;
using System.Text;
using CSGeneral;


public class SIRIUSArbitrator : Arbitrator
{
 #region Setup Class Members
    // IDE set paramaters
    [Link(IsOptional.Yes)]
    Function FixationMetabolicCost = null;
    [Link(IsOptional.Yes)]
    Function RetransWtNRatio = null;
    double _RetransWtNRatio = 0.0;
    public override void Initialised()
    {
        base.Initialised();
        _RetransWtNRatio = 0;
        if (RetransWtNRatio != null) //Default of zero means no biomass will be moved with reallocated N
            _RetransWtNRatio = RetransWtNRatio.Value; 
    }
    private void Or(bool p)
    {
        throw new NotImplementedException();
    }

    //  Class arrays
    double[] DMSupplyPlant = null;
    double[] DMDemand = null;
    double[] DMAllocation = null;
    double[] DMRetranslocation = null;
    
    double[] NDemandPlant = null;
    double[] RelativeNDemand = null;
    double[] NReallocationSupply = null;
    double[] NUptakeSupply = null;
    double[] NFixationSupply = null;
    double[] NRetranslocationSupply = null;
    double[] NReallocation = null;
    double[] NUptake = null;
    double[] NFixation = null;
    double[] NRetranslocation = null;
    double[] FixationWtLoss = null;
    double[] NLimitedGrowth = null;
    double[] NAllocated = null;
        

    // Public Arbitrator variables
    private double TotalDMSupply = 0;
    [Output]
    public override double DMSupply
    {
        get
        {
            return TotalDMSupply;
        }
    }
    private double TotalNDemand = 0;
    [Output]
    public override double NDemand
    {
        get
        {
            return TotalNDemand;
        }
    }
 #endregion

    public override void DoDM(List<Organ> Organs)
    {
//Fixme  The only biomass retranslocation is associated with N retranslocation.  Need something to move sugars to reproductive organs
 #region Setup Biomass calculations
        //create organ specific variables
        DMSupplyPlant = new double[Organs.Count];
        DMDemand = new double[Organs.Count];
        DMAllocation = new double[Organs.Count];
        DMRetranslocation = new double[Organs.Count];
        
        // GET INITIAL STATE VARIABLES FOR MASS BALANCE CHECKS
        double StartingMass = 0;
        for (int i = 0; i < Organs.Count; i++)
            StartingMass += Organs[i].Live.Wt + Organs[i].Dead.Wt;

        // GET SUPPLIES AND CALCULATE TOTAL
        for (int i = 0; i < Organs.Count; i++)
            DMSupplyPlant[i] = Organs[i].DMSupply;
        TotalDMSupply = MathUtility.Sum(DMSupplyPlant);

        // set start values once TotalDMSupply is determined but before any arbitration has been done
        foreach (Organ o in Organs)
        {
            o.DoStartSet();
        }

        // SET OTHER ORGAN VARIABLES AND CALCULATE TOTALS
        for (int i = 0; i < Organs.Count; i++)
        {
            DMDemand[i] = Organs[i].DMDemand;
            DMAllocation[i] = 0;
            DMRetranslocation[i] = 0;
        }
        double TotalDMDemand = MathUtility.Sum(DMDemand);
 #endregion

 #region Allocate Assimilated biomass
        //  Allocate to meet Organs demands
        double TotalWtAllocated = 0;
        double TotalWtNotAllocatedSinkLimitation = 0;
        for (int i = 0; i < Organs.Count; i++)
        {
            double proportion = 0.0;
            if (DMDemand[i] > 0.0)
            {
                proportion = DMDemand[i] / TotalDMDemand;
                DMAllocation[i] = Math.Min(TotalDMSupply * proportion, DMDemand[i]);
                TotalWtAllocated += DMAllocation[i];
            }
        }
        
        // Anything not required by organs demand is Not allocated.  This represents down regulation of photosynthesis if there is limited sink size.
        TotalWtNotAllocatedSinkLimitation = Math.Max(0.0, TotalDMSupply - TotalWtAllocated);
        
        // Send potential DM allocation to organs to set this variable for calculating N demand
        for (int i = 0; i < Organs.Count; i++)
        {
            Organs[i].DMPotentialAllocation = DMAllocation[i];
        }
        // Then check it all adds up
        double DMBalanceError = Math.Abs((TotalWtAllocated + TotalWtNotAllocatedSinkLimitation) - TotalDMSupply);
        if (DMBalanceError > 0.00001 & TotalDMDemand > 0)
        throw new Exception("Mass Balance Error in Photosynthesis DM Allocation");
        
        // ==============================================================================
        // Retranslocate DM from senessing tops .
        // ==============================================================================

        //Currently retranslocate DM in proportion to N retranslocation but don't do DM explicitly
 #endregion

 #region Set up Nitorgen calculations
        // Create organ specific variables       
        NDemandPlant = new double[Organs.Count];
        RelativeNDemand = new double[Organs.Count];
        NReallocationSupply = new double[Organs.Count];
        NUptakeSupply = new double[Organs.Count];
        NFixationSupply = new double[Organs.Count];
        NRetranslocationSupply = new double[Organs.Count];
        NReallocation = new double[Organs.Count];
        NUptake = new double[Organs.Count]; 
        NFixation = new double[Organs.Count];
        NRetranslocation = new double[Organs.Count];
        FixationWtLoss = new double[Organs.Count];
        NLimitedGrowth = new double[Organs.Count];
        NAllocated = new double[Organs.Count];
        
        // GET ALL INITIAL STATE VARIABLES FOR MASS BALANCE CHECKS
        double StartingN = 0;
        for (int i = 0; i < Organs.Count; i++)
            StartingN += Organs[i].Live.N + Organs[i].Dead.N;

        // GET ALL SUPPLIES AND DEMANDS AND CALCULATE TOTALS
        for (int i = 0; i < Organs.Count; i++)
        {
            NDemandPlant[i] = Organs[i].NDemand;
            NReallocationSupply[i] = Organs[i].NReallocationSupply;
            NUptakeSupply[i] = Organs[i].NUptakeSupply;
            NFixationSupply[i] = Organs[i].NFixationSupply;
            NRetranslocationSupply[i] = Organs[i].NRetranslocationSupply;
            NReallocation[i] = 0;
            NUptake[i] = 0;
            NFixation[i] = 0;
            NRetranslocation[i] = 0;
            NAllocated[i] = 0;            
            FixationWtLoss[i] = 0;
        }
               TotalNDemand = MathUtility.Sum(NDemandPlant);
        double TotalNReallocationSupply = MathUtility.Sum(NReallocationSupply);
        double TotalNUptakeSupply = MathUtility.Sum(NUptakeSupply);
        double TotalNFixationSupply = MathUtility.Sum(NFixationSupply);
        double TotalNRetranslocationSupply = MathUtility.Sum(NRetranslocationSupply);

        //Set relative N demands of each organ
        for (int i = 0; i< Organs.Count; i++)
            RelativeNDemand[i] = Organs[i].NDemand / TotalNDemand;
#endregion

 #region Reallocate Senesced Nitrogen
        double NReallocationAllocated = 0;
        if (TotalNReallocationSupply > 0.00000000001)
        {
            //Calculate how much reallocated N (and associated biomass) each demanding organ gets based on relative demands
            double NDemandFactor = 1.0;
            double DMretranslocationFactor = 1.0;
            Allocate(Organs, TotalNReallocationSupply, ref NReallocationAllocated, NDemandFactor, DMretranslocationFactor);

            //Then calculate how much N (and associated biomass) is realloced from each supplying organ based on relative supply
            for (int i = 0; i < Organs.Count; i++)
            {
                if (NReallocationSupply[i] > 0)
                {
                    double RelativeSupply = NReallocationSupply[i] / TotalNReallocationSupply;
                    NReallocation[i] += NReallocationAllocated * RelativeSupply;
                    DMRetranslocation[i] += NReallocationAllocated * RelativeSupply * _RetransWtNRatio; // convert N to crude protein or NO3 (depending on the value of DMRetransFact) 
                }
            }
        }
 #endregion

 #region Allocate Nitrogen Uptake
        double NUptakeAllocated = 0;
        if (TotalNUptakeSupply > 0.00000000001)
        {
            // Calculate how much uptake N each demanding organ gets based on relative demands
            double NDemandFactor = 1.0;
            double DMretranslocationFactor = 0.0;
            Allocate(Organs, TotalNUptakeSupply, ref NUptakeAllocated, NDemandFactor, DMretranslocationFactor);

            // Then calculate how much N is taken up by each supplying organ based on relative supply
            for (int i = 0; i < Organs.Count; i++)
            {
                if (NUptakeSupply[i] > 0.00000000001)
                {
                    double RelativeSupply = NUptakeSupply[i] / TotalNUptakeSupply;
                    NUptake[i] += NUptakeAllocated * RelativeSupply;
                }
            }
         }
 #endregion

 #region Determine Nitrogen Fixation
        double NFixationAllocated = 0;
        double TotalFixationWtloss = 0;
        if (TotalNFixationSupply > 0.00000000001)
        {
            // Calculate how much fixation N each demanding organ gets based on relative demands
            double NDemandFactor = 0.7;
            double DMretranslocationFactor = 1.0;
            Allocate(Organs, TotalNFixationSupply, ref NFixationAllocated, NDemandFactor, DMretranslocationFactor);

            // Then calculate how much N is fixed from each supplying organ based on relative supply
            for (int i = 0; i < Organs.Count; i++)
            {
                if (NFixationSupply[i] > 0.00000000001)
                {
                    double RelativeSupply = NFixationSupply[i] / TotalNFixationSupply;
                    NFixation[i] = NFixationAllocated * RelativeSupply;
                    double Respiration = NFixationAllocated * RelativeSupply * FixationMetabolicCost.Value;  //Calculalte how much respirtion is associated with fixation
                    FixationWtLoss[i] = Respiration; // allocate it to the organ
                    TotalFixationWtloss += Respiration; // total fixation respiration up
                }
            }
        }
 #endregion
        
 #region Retranslocate Nitrogen
        double NRetranslocationAllocated = 0;
        if (TotalNReallocationSupply > 0.00000000001) //Fixme this is labeled wrong
        {
            // Calculate how much retranslocation N (and associated biomass) each demanding organ gets based on relative demands
            double NDemandFactor = 1.0;  // NOTE: Setting this below 1.0 did not effect retrans in potatoes becasue the RetransFactor was dominating.  Reducing this in Peas reduced fixation.  This is because N demand for grain is based only on daily increment but other organs is based no deficit.  Increasing retranslocation to grain will increase deficit in other organs and increase fixation.
            double DMretranslocationFactor = 1.0;
            Allocate(Organs, TotalNRetranslocationSupply, ref NRetranslocationAllocated, NDemandFactor, DMretranslocationFactor);

            /// Then calculate how much N (and associated biomass) is retranslocated from each supplying organ based on relative supply
            for (int i = 0; i < Organs.Count; i++)
            {
                if (NRetranslocationSupply[i] > 0.00000000001)
                {
                    double RelativeSupply = NRetranslocationSupply[i] / TotalNRetranslocationSupply;
                    NRetranslocation[i] += NRetranslocationAllocated * RelativeSupply;
                    DMRetranslocation[i] += NRetranslocationAllocated * RelativeSupply * _RetransWtNRatio; // convert N to crude protein or NO3 (depending on the value of DMRetransFact) 
                }
            }
        }
 #endregion

 #region Actual DM allocation
        // Work out the amount of biomass (if any) lost due to the cost of N fixation
        double NetWtLossFixation = 0;
        if (NFixationAllocated > 0.00000000001)
        {
            //First determine it the cost of N fixation can be met by potential biomass production that was surpless to growing organ demands
            NetWtLossFixation = Math.Max(0.0, TotalFixationWtloss - TotalWtNotAllocatedSinkLimitation);
            if (NetWtLossFixation > 0.00000000001)
            {  
            TotalWtAllocated -= NetWtLossFixation; //If not reduce biomass allocations to account for the cost of fixation
            double WtLossNotAttributed = NetWtLossFixation;
            for (int i = 0; i < Organs.Count; i++) //The reduce allocation to individual organs and don't constrain an organ if that will cause its N conc to exceed maximum (i.e constrain the growth of the organs in larger defict so they move closer to maxNconc)
                {   
                double MinposbileDM = (Organs[i].Live.N + NAllocated[i]) / Organs[i].MaxNconc;
                double CurrentDM = Organs[i].Live.Wt + DMAllocation[i];
                double Possibleloss = Math.Max(0.0, CurrentDM - MinposbileDM);
                DMAllocation[i] -= Math.Min(Possibleloss, WtLossNotAttributed);
                WtLossNotAttributed -= Math.Min(Possibleloss, WtLossNotAttributed); 
                }
            if (WtLossNotAttributed > 0.00000000001)
                throw new Exception("Crop is trying to Fix excessive amounts of N.  Check partitioning coefficients are giving realistic nodule size and that FixationRatePotential is realistic");
            }
        }
        
        // Calculate posible growth based on Minimum N requirement of organs
        for (int i = 0; i < Organs.Count; i++)
        {
            if (NAllocated[i] >= NDemandPlant[i])
                NLimitedGrowth[i] = 100000000; //given high value so where there is no N deficit in organ and N limitation to growth  
            else
                NLimitedGrowth[i] = NAllocated[i] / Organs[i].MinNconc;
        }

        // Reduce DM allocation below potential if insufficient N to reach Min n Conc or if DM was allocated to fixation
        double NLimitatedWtAllocation = 0;
        for (int i = 0; i < Organs.Count; i++)
        {
            DMAllocation[i] = Math.Min(DMAllocation[i], NLimitedGrowth[i]);
            NLimitatedWtAllocation += DMAllocation[i];
        }
        double TotalWtLossNShortage = TotalWtAllocated - NLimitatedWtAllocation;
 #endregion
        
 #region Send arbitration results
        // =======================================
        // Send DM allocations to all Plant Organs
        // =======================================
        for (int i = 0; i < Organs.Count; i++)
        {
            Organs[i].DMAllocation = DMAllocation[i];
            Organs[i].DMRespired = FixationWtLoss[i];
            Organs[i].DMRetranslocation = DMRetranslocation[i];
        }

        // ======================================
        // Send N allocations to all Plant Organs
        // ======================================

        for (int i = 0; i < Organs.Count; i++)
        {
            if (NAllocated[i] < -0.00001)
                throw new Exception("-ve N Allocation");
            else if (NAllocated[i] < 0.0)
                NAllocated[i] = 0.0;

            Organs[i].NReallocation = NReallocation[i];
            Organs[i].NUptake = NUptake[i];
            Organs[i].NFixation = NFixation[i];
            Organs[i].NRetranslocation = NRetranslocation[i];
            Organs[i].NAllocation = NAllocated[i];
        }
 #endregion
 
 #region Mass balance checking
        // ==============================================================================
        // CHECK OVERALL MASS BALANCE
        // ==============================================================================
        double EndN = 0;
        for (int i = 0; i < Organs.Count; i++)
            EndN += Organs[i].Live.N + Organs[i].Dead.N;
        double NBalanceError = (EndN - (StartingN + TotalNUptakeSupply + TotalNFixationSupply));
        if (NBalanceError > 0.000000001)
            throw new Exception("Daily Plant N increment is greater than N supply");

        double EndWt = 0;
        for (int i = 0; i < Organs.Count; i++)
            EndWt += Organs[i].Live.Wt + Organs[i].Dead.Wt;
        DMBalanceError = Math.Abs(EndWt - (StartingMass + TotalDMSupply - TotalWtNotAllocatedSinkLimitation - TotalWtLossNShortage - NetWtLossFixation));
        if (DMBalanceError > 0.01)
            throw new Exception("Mass Balance Error in Overall DM Allocation");
 #endregion

    }

    private void Allocate(List<Organ> Organs, double TotalNReallocationSupply, ref double NReallocationAllocated, double NDemandFactor, double DMretranslocationFactor)
    {
        for (int i = 0; i < Organs.Count; i++)
        {
            double Requirement = Math.Max(0.0, NDemandPlant[i] * NDemandFactor - NAllocated[i]);
            double Allocation = 0.0;
            if (Requirement > 0.0)
            {
                Allocation = Math.Min(TotalNReallocationSupply * RelativeNDemand[i], Requirement);
                NAllocated[i] += Allocation;
                NReallocationAllocated += Allocation;
                if (_RetransWtNRatio > 0 && DMretranslocationFactor == 1.0) //Reallocate DM associated with N retranslocation
                {
                    DMAllocation[i] += Allocation * _RetransWtNRatio; // convert N to crude protein or NO3 (depending on the value of DMRetransFact) 
                }
            }
        }
    }
}
