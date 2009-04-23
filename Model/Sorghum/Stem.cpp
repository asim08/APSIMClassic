//------------------------------------------------------------------------------------------------
#include "Plant.h"
#include "Stem.h"

//------------------------------------------------------------------------------------------------
//------ Stem Constructor
//------------------------------------------------------------------------------------------------
Stem::Stem(ScienceAPI &api, Plant *p) : PlantPart(api) 
   {
   plant = p;
   name = "Stem";
   partNo = 2;

   initialize();
   doRegistrations();
   }
//--------------------------------------------------------------------------------------------------
// Register variables for other modules
//--------------------------------------------------------------------------------------------------
void Stem::doRegistrations(void)
   {
   scienceAPI.expose("StemGreenWt",    "g/m^2", "Stem dry weight",           false, dmGreen);
   scienceAPI.expose("StemGreenN",     "g/m^2", "N in stem",                 false, nGreen);
   scienceAPI.expose("StemGreenNConc", "%",     "Live stem N concentration", false, nConc);
   scienceAPI.expose("DeltaStemGreenN","g/m^2", "Today's N increase in stem",false, dltNGreen);
   scienceAPI.expose("StemGreenNConc", "%",     "Live stem N concentration", false, nConc);
   scienceAPI.expose("StemGreenP",     "g/m^2" ,"P in live Stem",            false, pGreen);
   }
//------------------------------------------------------------------------------------------------
//------- Initialize variables
//------------------------------------------------------------------------------------------------
void Stem::initialize(void)
   {
   canopyHeight    = 0.0;
   dmGreenStem     = 0.0;
   dltCanopyHeight = 0.0;

   PlantPart::initialize();
   }
//------------------------------------------------------------------------------------------------
//------ read Stem parameters
//------------------------------------------------------------------------------------------------
void Stem::readParams (void)
   {
   heightFn.read(scienceAPI,"x_stem_wt","y_height");
   scienceAPI.read("dm_stem_init",   "", 0, initialDM);
   scienceAPI.read("stem_trans_frac","", 0, translocFrac);
   scienceAPI.read("retransRate",    "", 0, retransRate);
   // nitrogen
   scienceAPI.read("initialStemNConc", "", 0, initialNConc);
   targetNFn.read (scienceAPI,"x_stem_n","targetStemNConc");
   structNFn.read (scienceAPI,"x_stem_n","structStemNConc");

   scienceAPI.read("stemDilnNSlope","", 0, dilnNSlope);
   scienceAPI.read("stemDilnNInt",  "", 0, dilnNInt);


   // phosphorus
   pMaxTable.read(scienceAPI, "x_p_stage_code","y_p_conc_max_stem");
   pMinTable.read(scienceAPI, "x_p_stage_code","y_p_conc_min_stem");
   pSenTable.read(scienceAPI, "x_p_stage_code","y_p_conc_sen_stem");
   scienceAPI.read("p_conc_init_stem", "", 0, initialPConc);

   density = plant->getPlantDensity();
   }

//------------------------------------------------------------------------------------------------
void Stem::process(void)
   {
   calcCanopyHeight();
   }
//------------------------------------------------------------------------------------------------
//------ update Stem variables
//------------------------------------------------------------------------------------------------
void Stem::updateVars(void)
   {
   float dayNConc = divide(nGreen,dmGreen,0);
   dmGreen += dltDmGreen;
   dmGreen += dmRetranslocate;

   dmGreenStem = dmGreen / density;

   canopyHeight += dltCanopyHeight;
   nGreen += (dltNGreen +  dltNRetranslocate);
   nConc = divide(nGreen,dmGreen,0);
   dltNConc = dayNConc - nConc;

   stage = plant->phenology->currentStage();
   }
//------------------------------------------------------------------------------------------------
//------- react to a phenology event
//------------------------------------------------------------------------------------------------
void Stem::phenologyEvent(int iStage)
   {
   ExternalMassFlowType EMF;
   switch (iStage)
      {
      case emergence :
         dmGreen = initialDM * density;
         nGreen = initialNConc * dmGreen;
         pGreen = initialPConc * dmGreen;
         EMF.PoolClass = "crop";
         EMF.FlowType = "gain";
         EMF.DM = 0.0;
         EMF.N  = nGreen * gm2kg/sm2ha;
         EMF.P  = pGreen * gm2kg/sm2ha;
         EMF.C = 0.0; // ?????
         EMF.SW = 0.0;
         scienceAPI.publish("ExternalMassFlow", EMF);
         break;
      case flowering :
         //set the minimum weight of stem; used for translocation to grain and stem
         float dmPlantStem = divide (dmGreen, density);
         dmPlantMin = dmPlantStem * (1.0 - translocFrac);
         break;
      }
   }
//------------------------------------------------------------------------------------------------
void Stem::calcCanopyHeight(void)
   {
   float newHeight = heightFn.value(dmGreenStem);
   dltCanopyHeight = Max(0.0,newHeight - canopyHeight);
   }
//------------------------------------------------------------------------------------------------
float Stem::calcNDemand(void)
   {
   nDemand = 0.0;
   // STEM demand (g/m2) to keep stem [N] at levels from  targetStemNConc
   float nRequired = (dmGreen + dltDmGreen) * targetNFn.value(stage);
   nDemand = Max(nRequired - nGreen,0.0);
   return nDemand;
   }
//------------------------------------------------------------------------------------------------
float Stem::calcStructNDemand(void)
   {
   // calculate N required to maintain structural [N]
   float structNDemand = 0.0;
   if(stage >= startGrainFill)return structNDemand;

   // STEM demand to keep stem [N] at levels of structStemNConc
   float nRequired = (dmGreen + dltDmGreen) * structNFn.value(stage);
   structNDemand = Max(nRequired - nGreen,0.0);
   return structNDemand;
   }
//------------------------------------------------------------------------------------------------

float Stem::provideN(float requiredN)
   {
   // calculate the N available for translocation to other plant parts
   // N could be required for structural Stem/Rachis N, new leaf N or grain N
   // Stem N is availavle at a rate which is a function of stem [N]
   // dltStemNconc per dd  = 0.0062 * stemNconcPct - 0.001
   // cannot take below Structural stem [N]% 0.5

   float nProvided;

   if(dltNGreen > requiredN)
      {
      nProvided = requiredN;
      dltNRetranslocate -= nProvided;
      return nProvided;
      }
   else
      {
      nProvided = dltNGreen;
      requiredN -= nProvided;
      }


   float stemNConcPct = divide((nGreen),(dmGreen + dltDmGreen)) * 100;
   if(stemNConcPct < structNFn.value(stage) * 100)
      return 0;
   
   float dltStemNconc = (dilnNSlope * (stemNConcPct) + dilnNInt)
                         * plant->phenology->getDltTT();

   float availableN = (dltStemNconc) / 100 * (dmGreen + dltDmGreen);
   // cannot take below structural N
   float structN =  (dmGreen + dltDmGreen) * structNFn.value(stage);
   availableN = Min(availableN,(nGreen) - structN);
  
   availableN = Max(availableN,0.0);
   
   nProvided += Min(availableN,requiredN);
   dltNRetranslocate -= nProvided;
   return nProvided;
   }
//------------------------------------------------------------------------------------------------
float Stem::dmRetransAvailable(void)
   {
   // calculate dry matter available for translocation to grain
   float stemWt = dmGreen + dltDmGreen;
   float stemWtAvail = (stemWt - (dmPlantMin * density)) * retransRate;
   return Max(stemWtAvail,0.0);
   }
//------------------------------------------------------------------------------------------------
float Stem::calcPDemand(void)
   {
   // Leaf P demand

   float rel_growth_rate = divide(plant->biomass->getDltDMPotRUE(),
         plant->biomass->getAboveGroundBiomass(),0.0);

   float deficit = pConcMax() * dmGreen * (1.0 + rel_growth_rate) - pGreen;

   pDemand = Max(deficit,0.0);
   return pDemand;
   }
//------------------------------------------------------------------------------------------------

