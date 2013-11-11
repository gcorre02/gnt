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
    /// uses the volume of previous bar to activate the rsi system
    /// </summary>
    [Description("uses the volume of previous bar to activate the rsi system")]
    public class RSIVolActivateNoLim : Strategy
    {
        #region Variables
        // Wizard generated variables
        private int lookBackBars = 5; // Default setting for LookBackBars
        private int barRangeTarget = 15; // Default setting for BarRangeTarget
        private int targetVol = 750; // Default setting for TargetVol
        private int addOnOff = 0; // Default setting for AddOnOff
        private int overPurchased = 17; // Default setting for OverPurchased
        private int rsiBars = 23; // Default setting for RsiBars
        private int stopLoss = 19; // Default setting for StopLoss
     //   private int limit = 43; // Default setting for Limit
        // User defined variables (add any user defined variables below)
		private int OverBoughtRsi = 50;
		private int OverSoldRsi = 50;
		private int turnOnRsi = 0;
//		private int barRange;
//		private int rangeAnalysisPosition;
//		private int volumePosition;
        #endregion

        /// <summary>
        /// This method is used to configure the strategy and is called once before any strategy method is called.
        /// </summary>
        protected override void Initialize()
        {
		//	SetProfitTarget("", CalculationMode.Ticks, Limit);
            SetStopLoss("", CalculationMode.Ticks, StopLoss, false);
			EntriesPerDirection = 1;
            CalculateOnBarClose = true;
			OverBoughtRsi = 50 + OverPurchased;
			OverSoldRsi = 50 - OverPurchased;
        }
		public void checkVolume(){
			for(int i =1; i <= lookBackBars;i++){
				if(Volume[i]<750
					&& (High[i]-Low[i])>= barRangeTarget*TickSize){
						turnOnRsi = 1;
						i = lookBackBars+1;
					}
			}
		}

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
			turnOnRsi= 0;
			checkVolume();
            // Condition set 1
         
			if (CrossBelow(RSI(RsiBars, 3), OverBoughtRsi, 1)
				//&& Positions[0].Quantity ==0
				&& turnOnRsi == 1)
            {
				if(addOnOff ==1){
					EnterShortLimit(DefaultQuantity,Close[0]+10*TickSize, "");}
				EnterShort(Positions[0].Quantity+1);
				turnOnRsi = 0;
            }
			 if (CrossAbove(RSI(RsiBars, 3), OverSoldRsi, 1)
				//&& Positions[0].Quantity ==0
				&& turnOnRsi == 1)
            {
				if(addOnOff ==1){
					EnterLongLimit(DefaultQuantity, Close[0]-10*TickSize, "");}
				EnterLong(Positions[0].Quantity+1);
				turnOnRsi = 0;
            }
			
        }

        #region Properties
        [Description("in bars")]
        [GridCategory("Parameters")]
        public int LookBackBars
        {
            get { return lookBackBars; }
            set { lookBackBars = Math.Max(0, value); }
        }

        [Description("in ticks")]
        [GridCategory("Parameters")]
        public int BarRangeTarget
        {
            get { return barRangeTarget; }
            set { barRangeTarget = Math.Max(0, value); }
        }

        [Description("in volume")]
        [GridCategory("Parameters")]
        public int TargetVol
        {
            get { return targetVol; }
            set { targetVol = Math.Max(0, value); }
        }

        [Description("TURN ADD ON")]
        [GridCategory("Parameters")]
        public int AddOnOff
        {
            get { return addOnOff; }
            set { addOnOff = Math.Max(0, value); }
        }

        [Description("distance to 50 of the price")]
        [GridCategory("Parameters")]
        public int OverPurchased
        {
            get { return overPurchased; }
            set { overPurchased = Math.Max(0, value); }
        }

        [Description("look back RSI")]
        [GridCategory("Parameters")]
        public int RsiBars
        {
            get { return rsiBars; }
            set { rsiBars = Math.Max(1, value); }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public int StopLoss
        {
            get { return stopLoss; }
            set { stopLoss = Math.Max(0, value); }
        }

//       [Description("")]
//        [GridCategory("Parameters")]
//        public int Limit
//        {
//            get { return limit; }
//            set { limit = Math.Max(1, value); }
//        }
        #endregion
    }
}
