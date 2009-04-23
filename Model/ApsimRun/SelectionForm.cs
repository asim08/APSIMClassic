using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ApsimFile;
using System.Collections.Specialized;

namespace ApsimRun
   {
   public partial class SelectionForm : Form
      {
      private Runnable SimulationFile;

      public SelectionForm(Runnable SimulationFile)
         {
         InitializeComponent();
         this.SimulationFile = SimulationFile;
         SimulationList.Items.AddRange(SimulationFile.SimulationsToRun.ToArray());
         MakeDefaultSelections();
         }


      private void OnClosing(object sender, FormClosingEventArgs e)
         {
         if (DialogResult == DialogResult.OK)
            {
            SaveDefaultSelections();
            List<string> Selections = new List<string>();
            foreach (string Item in SimulationList.SelectedItems)
               Selections.Add(Item);
            SimulationFile.SimulationsToRun = Selections;
            }
         else
            SimulationFile.SimulationsToRun = new List<string>();
         }

      private void MakeDefaultSelections()
         {
         List<string> RecentSimulations = Configuration.Instance.Settings("Recent");
         foreach (String RecentSimulation in RecentSimulations)
            {
            int Index = SimulationList.Items.IndexOf(RecentSimulation);
            if (Index != -1)
               SimulationList.SetSelected(Index, true);
            }

         // If none are selected, then select all.
         if (SimulationList.SelectedItems.Count == 0)
            {
            for (int i = 0; i != SimulationList.Items.Count; i++)
               SimulationList.SetSelected(i, true);
            }
         }

      private void SaveDefaultSelections()
         {
         List<string> Selections = new List<string> ();
         foreach (string Selection in SimulationList.SelectedItems)
            Selections.Add(Selection);
         Configuration.Instance.SetSettings("Recent", Selections);
         }

      private void OnKeyDown(object sender, KeyEventArgs e)
         {
         if (e.KeyCode == Keys.Return)
            {
            DialogResult = DialogResult.OK;
            Close();
            }
         else if (e.KeyCode == Keys.Escape)
            {
            DialogResult = DialogResult.Cancel;
            Close();
            }
         }

      
      }
   }