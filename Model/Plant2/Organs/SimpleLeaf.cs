using System;
using System.Collections.Generic;
using System.Text;

public class SimpleLeaf : Organ
   {
   private double _WaterAllocation;
   private double EP = 0;
   private double PEP = 0;
   private Biomass _Live = new Biomass();
   private Biomass _Dead = new Biomass();

   [Event] public event ApsimTypeDelegate NewPotentialGrowth;
   [Event] public event ApsimTypeDelegate New_Canopy;

   [Input] private float maxt = 0;
   [Input] private float mint = 0;
   [Input] private float vp = 0;
   [Param("Height")] private double _Height;          // Height of the canopy (mm) 
   [Param("LAI")] private double _LAI;                // Leaf Area Index (Green)
   [Param("LAIDead")] private double _LAIDead;        // Leaf Area Index (Dead)
   [Param("Frgr")] private double _Frgr;              // Relative Growth Rate Factor
   [Param] private LinearInterpolation FT = null;   // Temperature effect on Growth Interpolation Set
   [Param] private LinearInterpolation FVPD = null; // VPD effect on Growth Interpolation Set
   [Param] private double K = 0;                      // Extinction Coefficient (Green)
   [Param] private double KDead = 0;                  // Extinction Coefficient (Dead)

   public override Biomass Live
      {
      get { return Live; }
      }
   public override Biomass Dead
      {
      get { return Live; }
      }

   public override double DMDemand
      {
      get { return 1; }
      }
   public override double DMSupply
      {
      get { return 1; }
      }
   public override double DMAllocation
      {
      set
         {
         Live.StructuralWt += value;
         }
      }
   public override double DMRetranslocationSupply {get {return 0.0;} }
   public override double DMRetranslocation { set {  } }

   [Output][Units("mm")]public override double WaterDemand { get { return PEP; } }
   public override double WaterSupply { get { return 0; } }

   public override double WaterAllocation
      {
      get { return _WaterAllocation; }
      set
         {
         _WaterAllocation = value;
         EP = EP + _WaterAllocation;
         }
      }
   [Output]
   public double Frgr
      {
      get { return _Frgr; }
      set
         {
         _Frgr = value;
         PublishNewCanopyEvent();
         }
      }
   [Output]
   public double Ft
      {
      get
         {
         double Tav = (maxt + mint) / 2.0;
         return FT.Value(Tav);
         }
      }
   [Output]
   public double Fvpd
      {
      get
         {
         const double SVPfrac = 0.66;

         double VPDmint = VBMet.Humidity.svp(mint) - vp;
         VPDmint = Math.Max(VPDmint, 0.0);

         double VPDmaxt = VBMet.Humidity.svp(maxt) - vp;
         VPDmaxt = Math.Max(VPDmaxt, 0.0);

         double VPD = SVPfrac * VPDmaxt + (1 - SVPfrac) * VPDmint;

         return FVPD.Value(VPD);
         }
      }
   [Output]
   public double LAI
      {
      get { return _LAI; }
      set
         {
         _LAI = value;
         PublishNewCanopyEvent();
         }
      }
   [Output]
   public double LAIDead
      {
      get { return _LAIDead; }
      set
         {
         _LAIDead = value;
         PublishNewCanopyEvent();
         }
      }
   [Output("Height")][Units("mm")]
   public double Height
      {
      get { return _Height; }
      set
         {
         _Height = value;
         PublishNewCanopyEvent();
         }
      }
   [Output("Cover_green")]
   public double CoverGreen
      {
      get { return 1.0 - Math.Exp(-K * LAI); }
      }
   [Output("Cover_tot")]
   public double CoverTot
      {
      get { return 1.0 - (1 - CoverGreen) * (1 - CoverDead); }
      }
   [Output("Cover_dead")]
   public double CoverDead
      {
      get { return 1.0 - Math.Exp(-KDead * LAIDead); }
      }


   [EventHandler]
   public void OnSow(SowType Data)
      {
      PublishNewPotentialGrowth();
      PublishNewCanopyEvent();
      }

   [EventHandler]
   public void OnPrepare()
      {
      PublishNewPotentialGrowth();
      }

   [EventHandler]
   public void OnCanopy_Water_Balance(CanopyWaterBalanceType CWB)
      {
      for (int i = 0; i != CWB.Canopy.Length; i++)
         {
         if (CWB.Canopy[i].name == Parent.Name)
            PEP = CWB.Canopy[i].PotentialEp;
         }
      }
   private void PublishNewPotentialGrowth()
      {
      // Send out a NewPotentialGrowthEvent.
      if (NewPotentialGrowth != null)
         {
         NewPotentialGrowthType GrowthType = new NewPotentialGrowthType();
         GrowthType.sender = Plant.Name;
         GrowthType.frgr = (float)Math.Min(Math.Min(Frgr, Fvpd), Ft);
         NewPotentialGrowth.Invoke(GrowthType);
         }
      }
   private void PublishNewCanopyEvent()
      {
      if (New_Canopy != null)
         {
         NewCanopyType Canopy = new NewCanopyType();
         Canopy.sender = Plant.Name;
         Canopy.lai = (float)LAI;
         Canopy.lai_tot = (float)(LAI + LAIDead);
         Canopy.height = (float)Height;
         Canopy.depth = (float)Height;
         Canopy.cover = (float)CoverGreen;
         Canopy.cover_tot = (float)CoverTot;
         New_Canopy.Invoke(Canopy);
         }
      }

   }
   
