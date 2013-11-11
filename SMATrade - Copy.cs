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
    /// Uses the SMA to determine sustained trends and reversing trends
    /// </summary>
    [Description("Uses the SMA to determine sustained trends and reversing trends")]
    public class SMATrade1 : Strategy
    {
        #region Variables
        // Wizard generated variables
        private double maxTime = 200; // Default setting for MaxTime
        private int rollStop = 25; // Default setting for RollStop
        private int highestTick = 1; // Default setting for HighestTick
        private int setLimitorStop = 2; // Default setting for SetLimitorStop
        private int stopLoss = 25; // Default setting for StopLoss
        private int upTickLimit = 15; // Default setting for UpTickLimit
        private int sMAPeriod = 53; // Default setting for SMAPeriod
        private int ticksBack = 8; // Default setting for TicksBack
        // User defined variables (add any user defined variables below)
		private int systemOn = 0; // turns system on when pri9ce crosses SMA
		private double crossingPrice = 0;
		private double highestPrice = 0;
		private double lowestPrice = 0;
		private double timer = 0;
		private double currentTime = 0;

        #endregion

        /// <summary>
        /// This method is used to configure the strategy and is called once before any strategy method is called.
        /// </summary>
        protected override void Initialize()
        {
            Add(SMA(SMAPeriod));
            SetTrailStop(CalculationMode.Ticks, RollStop);
			//SetStopLoss(CalculationMode.Ticks, RollStop);
	        CalculateOnBarClose = false;
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
			currentTime = ToTime(DateTime.Now);			
		    // Condition set 1
            if (CrossAbove(Close, SMA(SMAPeriod), 1))
            {
                systemOn = 1;
                crossingPrice = Close[0];
				lowestPrice = Close[0];		
				timer = ToTime(DateTime.Now);
			}

            // Condition set 2
            if (systemOn == 1
                && Close[0] > highestPrice)
            {
                highestPrice = Close[0];
            }
            if (systemOn == 1
                && Close[0] < lowestPrice)
            {
                lowestPrice = Close[0];
            }

            // Condition set 3
            if (highestPrice >= crossingPrice + UpTickLimit*TickSize 
				|| currentTime-timer>=MaxTime)
            {
                systemOn = 0;           
                highestPrice = 0;
                crossingPrice = 0;
            }

            // Condition set 4
            if (systemOn == 1
                && Close[0] <= crossingPrice-TicksBack*TickSize)
            {
				
                EnterLongStop(DefaultQuantity, crossingPrice+SetLimitorStop*TickSize, "");
				systemOn = 0;
                highestPrice = 0;
                crossingPrice = 0;
				timer = 0;

            }
		//	Log("current time is " + currentTime + "System is " + systemOn + "position time is " + timer,LogLevel.Information);
        }

        #region Properties
        [Description("Max Time of analysis for SMA sustained brake")]
        [GridCategory("Parameters")]
        public double MaxTime
        {
            get { return maxTime; }
            set { maxTime = Math.Max(0.000, value); }
        }

        [Description("Amount of ticks Rolling stop re establishes")]
        [GridCategory("Parameters")]
        public int RollStop
        {
            get { return rollStop; }
            set { rollStop = Math.Max(1, value); }
        }

        [Description("Checks the highest tick recently achieved")]
        [GridCategory("Parameters")]
        public int HighestTick
        {
            get { return highestTick; }
            set { highestTick = Math.Max(1, value); }
        }

        [Description("Number of ticks above recent high to set stop order")]
        [GridCategory("Parameters")]
        public int SetLimitorStop
        {
            get { return setLimitorStop; }
            set { setLimitorStop = Math.Max(1, value); }
        }

        [Description("Actual Stop Loss")]
        [GridCategory("Parameters")]
        public int StopLoss
        {
            get { return stopLoss; }
            set { stopLoss = Math.Max(1, value); }
        }

        [Description("if first cross above SMA (highest tick) is higher than upticklimit, no trade")]
        [GridCategory("Parameters")]
        public int UpTickLimit
        {
            get { return upTickLimit; }
            set { upTickLimit = Math.Max(1, value); }
        }

        [Description("minutes")]
        [GridCategory("Parameters")]
        public int SMAPeriod
        {
            get { return sMAPeriod; }
            set { sMAPeriod = Math.Max(1, value); }
        }

        [Description("number of ticks it goes back after crossing SMA")]
        [GridCategory("Parameters")]
        public int TicksBack
        {
            get { return ticksBack; }
            set { ticksBack = Math.Max(1, value); }
        }
        #endregion
    }
}
