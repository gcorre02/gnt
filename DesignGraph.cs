#region Using declarations
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Indicator;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Strategy;
#endregion

// This namespace holds all strategies and is required. Do not change it.
namespace NinjaTrader.Strategy
{
    /// <summary>
    /// Takes Volume information in and prints it out in a graph.
    /// </summary>
    [Description("Takes Volume information in and prints it out in a graph.")]
    public class DesignGraph : Strategy
    {
        #region Variables
        // Wizard generated variables
        private int periodWidth = 1; // Default setting for PeriodWidth
        private int rangeInput = 1; // Default setting for RangeInput
        private int volumeInput = 1; // Default setting for VolumeInput
        // User defined variables (add any user defined variables below)
        #endregion

        /// <summary>
        /// This method is used to configure the strategy and is called once before any strategy method is called.
        /// </summary>
        protected override void Initialize()
        {
            CalculateOnBarClose = true;
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
			  using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Users\Guilherme\Documents\writtenFiles\thetext.txt", true))
        {
            file.WriteLine(Time[1].Date.ToLongDateString()+","+Time[1].TimeOfDay.ToString()+","+High[1].ToString()+","+Low[1].ToString()+","+Volume[1].ToString());
        }
			
			
        }

        #region Properties
        [Description("")]
        [GridCategory("Parameters")]
        public int PeriodWidth
        {
            get { return periodWidth; }
            set { periodWidth = Math.Max(1, value); }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public int RangeInput
        {
            get { return rangeInput; }
            set { rangeInput = Math.Max(1, value); }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public int VolumeInput
        {
            get { return volumeInput; }
            set { volumeInput = Math.Max(1, value); }
        }
        #endregion
    }
	
}
