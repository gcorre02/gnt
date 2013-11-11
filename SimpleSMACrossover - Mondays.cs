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
    public class SimpleSMACrossoverMondays : Strategy
    {
        #region Variables
        // Wizard generated variables
		private int startingHour = 0;
		private int endingHour = 13;
        private int sMAPeriods = 800; // Default setting for SMAPeriods
        private int rollStop = 25; // Default setting for RollStop
		private int prfitTarget = 100;
        // User defined variables (add any user defined variables below)
		private int itsOn= 0;
		private int openQuantity=0;
        #endregion

        /// <summary>
        /// This method is used to configure the strategy and is called once before any strategy method is called.
        /// </summary>
        protected override void Initialize()
        {	
			EntriesPerDirection = 2;
			EntryHandling = EntryHandling.AllEntries;
            SetStopLoss("", CalculationMode.Ticks, RollStop, false);
			SetProfitTarget(CalculationMode.Ticks, PrfitTarget);
            CalculateOnBarClose = false;
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
		
        protected override void OnBarUpdate()
        {
			openQuantity = Positions[0].Quantity +1;
			if (Times[0][0].DayOfWeek == DayOfWeek.Monday 
				&& Times[0][0].Hour >= StartingHour && Times[0][0].Hour <= EndingHour)
			{
				itsOn =1 ;
			} else {
				itsOn = 0;
			}
            // Condition set 1
            if (CrossAbove(Close, SMA(SMAPeriods), 1)
				&&  itsOn==1 ){
                EnterLong(openQuantity, "");
            }
			if(CrossBelow(Close, SMA(SMAPeriods),1) 
				&& itsOn==1){
				EnterShort(openQuantity, "");
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
		 [Description("")]
        [GridCategory("Parameters")]
        public int StartingHour
        {
            get { return startingHour; }
            set { startingHour = Math.Max(0, value); }
        }
		 [Description("")]
        [GridCategory("Parameters")]
        public int EndingHour
        {
            get { return endingHour; }
            set { endingHour = Math.Max(0, value); }
        }
        #endregion
    }
}
