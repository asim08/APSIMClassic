﻿using System;
using System.Collections.Generic;
using System.Text;
using ModelFramework;
using CSGeneral;
using System.Reflection;
using System.Collections;


public class OilPalm
{
    [Link]
    Component My = null;
    public string Name { get { return My.Name; } }
    [Output]
    public string plant_status = "out";
    [Link]
    public Paddock MyPaddock; // Can be used to dynamically get access to simulation structure and variables
    [Input]
    DateTime Today;   // Equates to the value of the current simulation date - value comes from CLOCK
    //[Param] string A;         // The value for this will come from the Properties page.
    //[Output] double B;        // An example of how to make a variable available to other APSIM modules

    [Output]
    [Param]
    public string Crop_Type = "";
    [Output]
    [Param]
    double height = 0.0;
    [Output]
    [Param]
    double cover_tot = 0.0;
    [Output]
    double interception = 0.0;

    [Param]
    double Ndemand = 0.0;
    [Output]
    double RootDepth = 0.0;
    [Param]
    double InitialRootDepth = 0.0;
    [Param]
    double MaximumRootDepth = 0.0;
    [Param]
    double[] kl;
    [Param]
    double[] ll;
    [Param]
    double[] xf;
    [Param]
    double InterceptionFraction = 0.0;
    [Input]
    double[] bd = null;
    [Input]
    double[] ll15_dep = null;
    [Input]
    double[] dul_dep = null;
    [Input]
    double[] sw_dep = null;
    [Input]
    double[] dlayer = null;
    [Input]
    double[] no3 = null;
    [Input]
    double rain = 0.0;
    [Input]
    double Radn = 0.0;
    [Input]
    double eo = 0.0;
    [Output]
    double[] PotSWUptake;
    [Output]
    double[] SWUptake;
    [Output]
    double PEP = 0.0;
    [Output]
    double EP = 0.0;
    [Output]
    double DltDM = 0.0;
    [Output]
    double FW = 0.0;
    [Output]
    double CumulativeFrondNumber = 0.0;
    [Output]
    double CumulativeBunchNumber = 0.0;
    [Output]
    double CumulativeYield = 0.0;
    [Output]
    double ReproductiveGrowthFraction = 0.0;
    [Output]
    double CarbonStress = 0.0;
    
    [Output]
    double Age = 0.0;
    double Population = 0.0;
    public SowPlant2Type SowingData;

    double[] PotNUptake;
    double[] NUptake;
    double StemGrowth = 0.0;
    double FrondGrowth = 0.0;
    double RootGrowth = 0.0;
    
    //FrondType[] Frond;
    public List<FrondType> Fronds = new List<FrondType>();
    public List<BunchType> Bunches = new List<BunchType>();
    public List<RootType> Roots = new List<RootType>();

    //Component MySoilWat;
    //Component MySoilN;

    [Link]
    public Function FrondAppRate = null;
    [Link]
    public Function FrondMaxArea = null;
    [Link]
    public Function ExtinctionCoeff = null;
    [Link]
    public Function ExpandingFronds = null;
    [Link]
    public Function InitialFrondNumber = null;
    [Link]
    public Function RUE = null;
    [Link]
    public Function RootFrontVelocity = null;
    [Link]
    public Function RootSenescenceRate = null;
    [Link]
    public Function SpecificLeafArea = null;
    [Link]
    public Function SpecificLeafAreaMax = null;
    [Link]
    public Function RootFraction = null;
    [Link]
    public Function BunchSizeMax = null;
    [Link]
    public Function FemaleFlowerFraction = null;
    [Link]
    public Function StemPartitionFraction = null;
    [Link]
    public Function FlowerAbortionFraction = null;
    [Link]
    public Function KNO3 = null;
    [Link]
    public Function StemNConcentration = null;
    [Link]
    public Function BunchNConcentration = null;
    [Link]
    public Function RootNConcentration = null;
    [Link]
    public Function BunchOilConversionFactor = null;
    [Link]
    public Function RipeBunchWaterContent = null;
    [Link]
    public Function FrondCriticalNConcentration = null;


    public class RootType
    {
        public double Mass = 0;
        public double N = 0;
        public double Length = 0;
    }

    public class FrondType
    {
        public double Mass;
        public double N;
        public double Area;
        public double Age;
    }
    public class BunchType
    {
        public double Mass=0;
        public double N=0;
        public double Age=0;
        public double FemaleFraction=1;
    }

    [Output] double StemMass = 0.0;
    [Output]
    double StemN = 0.0;
    [Output]
    double StemNConc 
    { 
        get
        {
            if (StemMass > 0)
                return StemN / StemMass*100;
            else
                return 0.0;
        }
    }

    // The following event handler will be called once at the beginning of the simulation
    [EventHandler]
    public void OnInitialised()
    {
        //MyPaddock.Parent.ChildPaddocks
        PotSWUptake = new double[ll15_dep.Length];
        SWUptake = new double[ll15_dep.Length];
        PotNUptake = new double[ll15_dep.Length];
        NUptake = new double[ll15_dep.Length];
        for (int i = 0; i < ll15_dep.Length; i++)
        {
            RootType R = new RootType();
            Roots.Add(R);
            Roots[i].Mass = 0.1;
            Roots[i].N = Roots[i].Mass * RootNConcentration.Value / 100;
        }

        for (int i = 0; i < (int)InitialFrondNumber.Value; i++)
        {
            FrondType F = new FrondType();
            F.Age = ((int)InitialFrondNumber.Value - i) * FrondAppRate.Value;
            F.Area = SizeFunction(F.Age);
            F.Mass = F.Area/SpecificLeafArea.Value;
            F.N = F.Mass * FrondCriticalNConcentration.Value/100.0;
            Fronds.Add(F);
            CumulativeFrondNumber += 1;
        }
        for (int i = 0; i < (int)InitialFrondNumber.Value + 50; i++)
        {
            BunchType B = new BunchType();
            B.FemaleFraction = FemaleFlowerFraction.Value;
            Bunches.Add(B);
        }


        RootDepth = InitialRootDepth;


    }
    [Event]
    public event NewCropDelegate NewCrop;
    [Event]
    public event NullTypeDelegate Sowing;
    [Event]
    public event FOMLayerDelegate IncorpFOM;

    [EventHandler]
    public void OnSow(SowPlant2Type Sow)
    {
        SowingData = Sow;
        plant_status = "alive";
        Population = SowingData.Population;

        if (NewCrop != null)
        {
            NewCropType Crop = new NewCropType();
            Crop.crop_type = Crop_Type;
            Crop.sender = Name;
            NewCrop.Invoke(Crop);
        }

        if (Sowing != null)
            Sowing.Invoke();

    }


    // The following event handler will be called each day at the beginning of the day
    [EventHandler]
    public void OnPrepare()
    {
        interception = rain * InterceptionFraction;
    }

    [EventHandler]
    public void OnProcess()
    {

        DoWaterBalance();
        DoGrowth();
        DoNBalance();
        DoDevelopment();
        DoFlowerAbortion();

    }

    private void DoFlowerAbortion()
    {
        Bunches[30].FemaleFraction *= (1.0 - FlowerAbortionFraction.Value);
    }

    private void DoRootGrowth(double Allocation)
    {
      int RootLayer = LayerIndex(RootDepth);
      RootDepth = RootDepth + RootFrontVelocity.Value*xf[RootLayer];
      RootDepth = Math.Min(MaximumRootDepth, RootDepth);
      RootDepth = Math.Min(MathUtility.Sum(dlayer),RootDepth);

      // Calculate Root Activity Values for water and nitrogen
      double[] RAw = new double[dlayer.Length];
      double[] RAn = new double[dlayer.Length];
      double TotalRAw = 0;
      double TotalRAn = 0;

      for (int layer = 0; layer < dlayer.Length; layer++)
      {
          if (layer <= LayerIndex(RootDepth))
              if (Roots[layer].Mass > 0)
              {
                  RAw[layer] = SWUptake[layer] / Roots[layer].Mass
                             * dlayer[layer]
                             * RootProportion(layer, RootDepth);
                  RAw[layer] = Math.Max(RAw[layer], 1e-20);  // Make sure small numbers to avoid lack of info for partitioning

                  RAn[layer] = NUptake[layer] / Roots[layer].Mass
                             * dlayer[layer]
                             * RootProportion(layer, RootDepth);
                  RAn[layer] = Math.Max(RAw[layer], 1e-10);  // Make sure small numbers to avoid lack of info for partitioning

              }
              else if (layer > 0)
              {
                  RAw[layer] = RAw[layer - 1];
                  RAn[layer] = RAn[layer - 1];
              }
              else
              {
                  RAw[layer] = 0;
                  RAn[layer] = 0;
              }
          TotalRAw += RAw[layer];
          TotalRAn += RAn[layer];
      }
      double allocated = 0;
      for (int layer = 0; layer < dlayer.Length; layer++)
      {
          if (TotalRAw > 0)

              Roots[layer].Mass += Allocation * RAw[layer] / TotalRAw;
          else if (Allocation > 0)
              throw new Exception("Error trying to partition root biomass");
          allocated += Allocation * RAw[layer] / TotalRAw;
      }



      // Do Root Senescence
      FOMLayerLayerType[] FOMLayers = new FOMLayerLayerType[dlayer.Length];

      for (int layer = 0; layer < dlayer.Length; layer++)
      {
          double Fr = RootSenescenceRate.Value;
          double DM = Roots[layer].Mass * Fr * 10.0;
          double N = Roots[layer].N * Fr * 10.0; 
          Roots[layer].Mass *= (1.0 - Fr);
          Roots[layer].N *= (1.0 - Fr); 
          Roots[layer].Length *= (1.0 - Fr);


          FOMType fom = new FOMType();
          fom.amount = (float)DM;
          fom.N = (float)N;
          fom.C = (float)(0.40 * DM);
          fom.P = 0;
          fom.AshAlk = 0;

          FOMLayerLayerType Layer = new FOMLayerLayerType();
          Layer.FOM = fom;
          Layer.CNR = 0;
          Layer.LabileP = 0;

          FOMLayers[layer] = Layer;
      }
      FOMLayerType FomLayer = new FOMLayerType();
      FomLayer.Type = Crop_Type;
      FomLayer.Layer = FOMLayers;
      IncorpFOM.Invoke(FomLayer);


    }
    private void DoGrowth()
    {
        DltDM = RUE.Value * Radn * cover_green*FW;
        double DMAvailable = DltDM;

        RootGrowth = (DltDM * RootFraction.Value);
        DMAvailable -= RootGrowth;
        DoRootGrowth(RootGrowth);

        double[] BunchDMD = new double[Bunches.Count];
        for (int i = 0; i < 10; i++)
            BunchDMD[i] = BunchSizeMax.Value/(10*FrondAppRate.Value) * Population*Bunches[i].FemaleFraction*BunchOilConversionFactor.Value;
        double TotBunchDMD = MathUtility.Sum(BunchDMD);

        double[] FrondDMD = new double[Fronds.Count];
        for (int i = 0; i < Fronds.Count; i++)
            FrondDMD[i] = (SizeFunction(Fronds[i].Age + 1) - SizeFunction(Fronds[i].Age)) / SpecificLeafArea.Value * Population;
        double TotFrondDMD = MathUtility.Sum(FrondDMD);

        double StemDMD = DMAvailable * StemPartitionFraction.Value;

        double Fr = Math.Min(DMAvailable / (TotBunchDMD + TotFrondDMD + StemDMD), 1.0);
        double Excess = 0.0;
        if (Fr > 1.0)
            Excess = DMAvailable - (TotBunchDMD+TotFrondDMD+StemDMD);

        for (int i = 0; i < 10; i++)
            Bunches[i].Mass += BunchDMD[i] * Fr / Population / BunchOilConversionFactor.Value;
        ReproductiveGrowthFraction = TotBunchDMD * Fr / DltDM;

        for (int i = 0; i < Fronds.Count; i++)
        {
            FrondGrowth = FrondDMD[i] * Fr / Population;
            Fronds[i].Mass += FrondGrowth;
            if (Fr >= SpecificLeafArea.Value / SpecificLeafAreaMax.Value)
                Fronds[i].Area += (SizeFunction(Fronds[i].Age + 1) - SizeFunction(Fronds[i].Age));
            else
                Fronds[i].Area += FrondGrowth * SpecificLeafAreaMax.Value;
            
        }

        StemGrowth =StemDMD*Fr+Excess; 
        StemMass += StemGrowth;

        CarbonStress = Fr;

    }
    private void DoDevelopment()
    {
        Age = Age + 1.0 / 365.0;
        //for (int i = 0; i < Frond.Length; i++)
        //    Frond[i].Age += 1;
        foreach (FrondType F in Fronds)
        {
            F.Age += 1;
            //F.Area = SizeFunction(F.Age);
        }
        if (Fronds[Fronds.Count - 1].Age >= FrondAppRate.Value)
        {
            FrondType F = new FrondType();
            Fronds.Add(F);
            CumulativeFrondNumber += 1;
            
            BunchType B = new BunchType();
            B.FemaleFraction = FemaleFlowerFraction.Value;
            Bunches.Add(B);
        }

        if (Fronds[0].Age >= (40 * FrondAppRate.Value))
        {
            CumulativeBunchNumber += Bunches[0].FemaleFraction;
            CumulativeYield += Bunches[0].Mass * Population/(1.0-RipeBunchWaterContent.Value);
            Fronds.RemoveAt(0);
            Bunches.RemoveAt(0);
        }
    }
    private void DoWaterBalance()
    {
        PEP = eo * cover_green;


        for (int j = 0; j < ll15_dep.Length; j++)
            PotSWUptake[j] = Math.Max(0.0,RootProportion(j, RootDepth)* kl[j] * (sw_dep[j] - ll15_dep[j]));

        double TotPotSWUptake = MathUtility.Sum(PotSWUptake);

        EP = 0.0;
        for (int j = 0; j < ll15_dep.Length; j++)
        {
            SWUptake[j] = PotSWUptake[j] * Math.Min(1.0, PEP / TotPotSWUptake);
            EP += SWUptake[j];
            sw_dep[j] = sw_dep[j] - SWUptake[j];

        }
        if (!MyPaddock.Set(".paddock.Soil Water.sw_dep", sw_dep))
            throw new Exception("Unable to set sw_dep");
        if (PEP > 0.0)
            FW = EP / PEP;
        else
            FW = 1.0;

    }

    private void DoNBalance()
    {
        double StartN = PlantN;

        double StemNDemand = StemGrowth * StemNConcentration.Value/100.0 * 10.0;  // factor of 10 to convert g/m2 to kg/ha
        double RootNDemand = Math.Max(0.0, (RootMass * RootNConcentration.Value / 100.0 - RootN)) * 10.0;  // kg/ha
        double FrondNDemand = Math.Max(0.0, (FrondMass * FrondCriticalNConcentration.Value / 100.0 - FrondN)) * 10.0;  // kg/ha 
        double BunchNDemand = Math.Max(0.0, (BunchMass * BunchNConcentration.Value / 100.0 - BunchN)) * 10.0;  // kg/ha 

        Ndemand = StemNDemand + FrondNDemand+RootNDemand+BunchNDemand;  //kg/ha


        for (int j = 0; j < ll15_dep.Length; j++)
        {
            double swaf = 0;
            swaf = (sw_dep[j] - ll15_dep[j]) / (dul_dep[j] - ll15_dep[j]);
            swaf = Math.Max(0.0, Math.Min(swaf, 1.0));
            double no3ppm = no3[j] * (100.0 / (bd[j] * dlayer[j]));
            PotNUptake[j] = Math.Max(0.0, RootProportion(j, RootDepth) * KNO3.Value * no3[j] * swaf);
        }

        double TotPotNUptake = MathUtility.Sum(PotNUptake);
        double Fr = Math.Min(1.0, Ndemand / TotPotNUptake);

        for (int j = 0; j < ll15_dep.Length; j++)
        {
            NUptake[j] = PotNUptake[j] * Fr;
            no3[j] = no3[j] - NUptake[j];

        }
        if (!MyPaddock.Set(".paddock.Soil Nitrogen.no3", no3))
            throw new Exception("Unable to set no3");

        if (Ndemand > 0)
            Fr = Math.Max(0.0, (MathUtility.Sum(NUptake) / Ndemand));
        else
            Fr = 0.0;

        StemN += StemNDemand/10 * Fr;

        double[] RootNDef = new double[ll15_dep.Length];
        double TotNDef = 1e-20;
        for (int j = 0; j < ll15_dep.Length; j++)
        {
            RootNDef[j] = Math.Max(0.0, Roots[j].Mass * RootNConcentration.Value/100.0 - Roots[j].N);
            TotNDef += RootNDef[j];
        }
        for (int j = 0; j < ll15_dep.Length; j++)
            Roots[j].N += RootNDemand/10 * Fr * RootNDef[j] / TotNDef;

        foreach (FrondType F in Fronds)
                F.N += Math.Max(0.0,F.Mass*FrondCriticalNConcentration.Value/100.0-F.N)*Fr;

        double Tot = 0;
        foreach (BunchType B in Bunches)
        {
            Tot += Math.Max(0.0, B.Mass * BunchNConcentration.Value / 100.0 - B.N) * Fr/SowingData.Population;
            B.N += Math.Max(0.0, B.Mass * BunchNConcentration.Value / 100.0 - B.N) * Fr;
        }
        double EndN = PlantN;
        double Change = EndN - StartN;
        double Uptake = MathUtility.Sum(NUptake) / 10.0;
        if (Math.Abs(Change-Uptake)>0.001)
            throw new Exception("Error in N Allocation");
    }


   
    [Output]
    public double LAI
    {
        get
        {
            double FrondArea = 0.0;

            //for (int i = 0; i < Frond.Length; i++)
            //   FrondArea = FrondArea + Frond[i].Area;
            foreach (FrondType F in Fronds)
                FrondArea += F.Area;
            return FrondArea*SowingData.Population;
        }

    }


    [Output]
    public double FrondMass
    {
        get
        {
            double FrondMass = 0.0;

            //for (int i = 0; i < Frond.Length; i++)
            //   FrondArea = FrondArea + Frond[i].Area;
            foreach (FrondType F in Fronds)
                FrondMass += F.Mass;
            return FrondMass * SowingData.Population;
        }

    }
    [Output]
    public double FrondN
    {
        get
        {
            double FrondN = 0.0;

            //for (int i = 0; i < Frond.Length; i++)
            //   FrondArea = FrondArea + Frond[i].Area;
            foreach (FrondType F in Fronds)
                FrondN += F.N;
            return FrondN * SowingData.Population;
        }

    }
    [Output]
    public double FrondNConc
    {
        get
        {
            return FrondN/FrondMass*100.0;
        }

    }
    [Output]
    public double BunchMass
    {
        get
        {
            double BunchMass = 0.0;

            foreach (BunchType B in Bunches)
                BunchMass += B.Mass;
            return BunchMass * SowingData.Population;
        }

    }
    [Output]
    public double BunchN
    {
        get
        {
            double BunchN = 0.0;

            foreach (BunchType B in Bunches)
                BunchN += B.N * SowingData.Population;
            return BunchN;
        }

    }
    [Output]
    public double BunchNConc
    {
        get
        {
            if (BunchMass > 0)
                return BunchN / BunchMass * 100.0;
            else
                return 0;
        }

    }

    [Output]
    public double RootMass
    {
        get
        {
            double RootMass = 0.0;

            foreach (RootType R in Roots)
                RootMass += R.Mass;
            return RootMass;
        }

    }
    [Output]
    public double RootN
    {
        get
        {
            double RootN = 0.0;

            foreach (RootType R in Roots)
                RootN += R.N;
            return RootN;
        }

    }
    [Output]
    public double RootNConc
    {
        get
        {
            return RootN / RootMass * 100.0;
        }

    }
    [Output]
    public double PlantN
    {
        get
        {
            return FrondN+RootN+StemN+BunchN;
        }
    }
    [Output]
    public double FrondNumber
    {
        get
        {
            return Fronds.Count;
        }
    }

    [Output]
    public double cover_green
    {
        get
        {
            return 1.0 - Math.Exp(-ExtinctionCoeff.Value * LAI);
        }
    }
    [Output]
    public double SLA
    {
        get { return LAI * 10000.0 / FrondMass; }
    }
    protected double SizeFunction(double Age)
    {
        double GrowthDuration = ExpandingFronds.Value * FrondAppRate.Value;
        double alpha = -Math.Log((1 / 0.99 - 1) / (FrondMaxArea.Value / (FrondMaxArea.Value * 0.01) - 1)) / GrowthDuration;
        double leafsize = FrondMaxArea.Value / (1 + (FrondMaxArea.Value / (FrondMaxArea.Value * 0.01) - 1) * Math.Exp(-alpha * Age));
        return leafsize;

    }
    private double RootProportion(int layer, double root_depth)
    {
        double depth_to_layer_bottom = 0;   // depth to bottom of layer (mm)
        double depth_to_layer_top = 0;      // depth to top of layer (mm)
        double depth_to_root = 0;           // depth to root in layer (mm)
        double depth_of_root_in_layer = 0;  // depth of root within layer (mm)
        // Implementation Section ----------------------------------
        for (int i = 0; i <= layer; i++)
            depth_to_layer_bottom += dlayer[i];
        depth_to_layer_top = depth_to_layer_bottom - dlayer[layer];
        depth_to_root = Math.Min(depth_to_layer_bottom, root_depth);
        depth_of_root_in_layer = Math.Max(0.0, depth_to_root - depth_to_layer_top);

        return depth_of_root_in_layer / dlayer[layer];
    }
    private int LayerIndex(double depth)
    {
        double CumDepth = 0;
        for (int i = 0; i < dlayer.Length; i++)
        {
            CumDepth = CumDepth + dlayer[i];
            if (CumDepth >= depth) { return i; }
        }
        throw new Exception("Depth deeper than bottom of soil profile");
    }

}