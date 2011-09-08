﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using CMPServices;

namespace ModelFramework
{
    /// <summary>
    /// --------------------------------------------------------------------
    /// Returns the singleton instance of a reflection class that is
    /// capable of returning metadata about the structure of the simulation.
    /// --------------------------------------------------------------------
    /// </summary>
    public class Component : TypedItem
    {
        protected ApsimComponent HostComponent;
        protected String FTypeName;
        protected String ParentCompName;            //Name of the parent component in the simulation
        protected String FQN;                       //Name of the actual component

        public String TypeName
        {
            get { return FTypeName; }
        }
        // --------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="TypeNameToMatch"></param>
        /// <returns></returns>
        // --------------------------------------------------------------------
        public override bool IsOfType(String TypeNameToMatch)
        {
            return TypeName.ToLower() == TypeNameToMatch.ToLower();
        }
        // --------------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="In">Instance of a root/leaf/shoot/phenology</param>
        // --------------------------------------------------------------------
        public Component(Instance In)
        {
            //get the name of the owner component of the Instance (e.g. Plant2)
            FQN = In.ParentComponent().GetName();
            ParentCompName = "";
            if (FQN.LastIndexOf('.') > -1)
                ParentCompName = FQN.Substring(0, FQN.LastIndexOf('.')); //e.g. Paddock
            //get the name of the host component for the calling object
            HostComponent = In.ParentComponent();   //e.g. Plant2
            FTypeName = HostComponent.CompClass;     //type of Plant2
        }
        // --------------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="_FullName">Name of the actual component</param>
        /// <param name="component">The apsim component that hosts this object</param>
        // --------------------------------------------------------------------
        public Component(String _FullName, ApsimComponent component)
        {
            FQN = _FullName;
            ParentCompName = "";
            if (FQN.LastIndexOf('.') > -1)
                ParentCompName = FQN.Substring(0, FQN.LastIndexOf('.'));
            HostComponent = component;
            //get the type for this component
            List<TComp> comps = new List<TComp>();
            if (_FullName != ".MasterPM") 
                component.Host.queryCompInfo(FQN, TypeSpec.KIND_COMPONENT, ref comps);
            if (comps.Count > 0)
                FTypeName = comps[0].CompClass;
            else
                FTypeName = this.GetType().Name;
        }
        // --------------------------------------------------------------------
        /// <summary>
        /// Return a list of all sibling components to caller.
        /// </summary>
        // --------------------------------------------------------------------
        public TypedList<Component> ComponentList
        {
            get
            {
                TypedList<Component> Children = new TypedList<Component>();
                foreach (KeyValuePair<uint, TComp> pair in HostComponent.SiblingComponents)
                {
                    Component C = new Component(pair.Value.name, HostComponent);
                    Children.Add(C);
                }
                return Children;
            }
        }
        // --------------------------------------------------------------------
        /// <summary>
        /// Returns a reference to a variable.
        /// </summary>
        /// <param name="VariableName"></param>
        /// <returns></returns>
        // --------------------------------------------------------------------
        public virtual Variable Variable(String VariableName)
        {
            return new Variable(HostComponent, FQN + '.' + VariableName);
        }
        // --------------------------------------------------------------------
        /// <summary>
        /// Publish a notification event (i.e. one that doesn't have any data 
        /// associated with it) to this component only.
        /// <param name="EventName"></param>
        /// </summary>
        // --------------------------------------------------------------------
        public virtual void Publish(String EventName)
        {
            HostComponent.Publish(FQN + "." + EventName, null);
        }
        // --------------------------------------------------------------------
        /// <summary>
        /// Publish an event that has associated data to this component only.
        /// </summary>
        /// <param name="EventName"></param>
        /// <param name="Data"></param>
        // --------------------------------------------------------------------
        public virtual void Publish(String EventName, ApsimType Data)
        {
            HostComponent.Publish(FQN + "." + EventName, Data);
        }
        // --------------------------------------------------------------------
        /// <summary>
        /// Subscribe to a notification event ie. one without any data associated with it.
        /// </summary>
        /// <param name="EventName"></param>
        /// <param name="F"></param>
        // --------------------------------------------------------------------
        public virtual void Subscribe(String EventName, RuntimeEventHandler.NullFunction F)
        {
            RuntimeEventHandler Event = new RuntimeEventHandler(EventName, F);
            HostComponent.Subscribe(Event);
        }
        // --------------------------------------------------------------------
        /// <summary>
        /// </summary>
        /// <returns>Unqualified name of the component.</returns>
        // --------------------------------------------------------------------
        public String Name()
        {
            return TRegistrar.unQualifiedName(FQN);
        }
        // --------------------------------------------------------------------
        /// <summary>
        /// Static helper function for getting a component name (eg. wheat) from a fully
        /// qualified component name (eg. .MasterPM.paddock1.wheat)
        /// </summary>
        /// <param name="fqn"></param>
        /// <returns>Unqualified name</returns>
        // --------------------------------------------------------------------
        public static String ComponentName(String fqn)
        {
            return TRegistrar.unQualifiedName(fqn);
        }
        // --------------------------------------------------------------------
        /// <summary>
        /// Fully qualified name of component eg. .MasterPM.paddock1.wheat
        /// </summary>
        // --------------------------------------------------------------------
        public String FullName
        {
            get
            {
                return FQN;
            }
        }
    }

}