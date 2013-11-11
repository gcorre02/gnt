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
    /// Simple trades when SMA is cross
    /// </summary>
    [Description("Simple trades when SMA is cross")]
    public class SimpleSMACrossover : Strategy
    {
        #region Variables
        // Wizard generated variables
        private int sMAPeriods = 800; // Default setting for SMAPeriods
        private int rollStop = 25; // Default setting for RollStop
		private int prfitTarget = 100;
        // User defined variables (add any user defined variables below)
		private int itsOn= 0;
        #endregion

        /// <summary>
        /// This method is used to configure the strategy and is called once before any strategy method is called.
        /// </summary>
        protected override void Initialize()
        {	
            SetStopLoss("", CalculationMode.Ticks, RollStop, false);
			SetProfitTarget(CalculationMode.Ticks, PrfitTarget);
            CalculateOnBarClose = false;
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
		
        protected override void OnBarUpdate()
        {
			if (Times[0][0].DayOfWeek == DayOfWeek.Monday){
				itsOn =1 ;
			} else {
				itsOn = 0;
			}
            // Condition set 1
            if (CrossAbove(Close, SMA(SMAPeriods), 1)
				&&  itsOn==1 )
            {
                EnterLong(DefaultQuantity, "");
            }
        }

        #region Properties
        [Description("")]
        [GridCategory("Parameters")]
        public int SMAPeriods
        {
            get { return sMAPeriods; }
            set { sMAPeriods = Math.Max(1, value); }
        }

		[Description("")]
        [GridCategory("Parameters")]
        public int PrfitTarget
        {
            get { return prfitTarget; }
            set { prfitTarget = Math.Max(1, value); }
        }
		
        [Description("")]
        [GridCategory("Parameters")]
        public int RollStop
        {
            get { return rollStop; }
            set { rollStop = Math.Max(1, value); }
        }
        #endregion
    }
}
