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
    /// Enter the description of your strategy here
    /// </summary>
    [Description("Enter the description of your strategy here")]
    public class RSIMEANwVolume : Strategy
    {
        #region Variables
        // Wizard generated variables
        private int positionSize = 1; // Default setting for PositionSize
        private int addOnOff = 1; // Default setting for AddOnOff
        private int overBoughtRsi = 67; // Default setting for OverBoughtRsi
        private int overSoldRsi = 33; // Default setting for OverSoldRsi
        private int rsiBars = 23; // Default setting for RsiBars
        private double addAtTicks = 10; // Default setting for AddAtTicks
        private int stopLoss = 19; // Default setting for StopLoss
        private int limit = 43; // Default setting for Limit
		private int volumeStrategyOnOff = 1;
		private int targetVolume = 2000;
        // User defined variables (add any user defined variables below)
		private double volumeSize = 0 ;
		private int volumeTargetOn = 0;
        #endregion	

        /// <summary>
        /// This method is used to configure the strategy and is called once before any strategy method is called.
        /// </summary>
        protected override void Initialize()
        {
            SetProfitTarget("", CalculationMode.Ticks, Limit);
            SetStopLoss("", CalculationMode.Ticks, StopLoss, false);
			EntriesPerDirection = PositionSize+1;
            CalculateOnBarClose = true;
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
			volumeSize = Volume[1] + Volume[2] + Volume[3];
			Log(volumeSize.ToString(),LogLevel.Information);
			if(volumeSize < TargetVolume && VolumeStrategyOnOff == 1){volumeTargetOn =1;}
            // Condition set 1
            if (CrossBelow(RSI(RsiBars, 3), OverBoughtRsi, 1)
				&& Positions[0].Quantity ==0
				&& volumeTargetOn ==1
				)
            {
				EnterShortLimit(PositionSize,Close[0]+TickSize*AddAtTicks, "");
				EnterShort(PositionSize);
   
            }

            // Condition set 3
            if (CrossAbove(RSI(RsiBars, 3), OverSoldRsi, 1)&& Positions[0].Quantity ==0)
            {
				EnterLongStop(PositionSize, Close[0]-TickSize*AddAtTicks, "");
				EnterLong(PositionSize);
            }

        }

        #region Properties
        [Description("")]
        [GridCategory("Parameters")]
        public int PositionSize
        {
            get { return positionSize; }
            set { positionSize = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public int AddOnOff
        {
            get { return addOnOff; }
            set { addOnOff = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public int OverBoughtRsi
        {
            get { return overBoughtRsi; }
            set { overBoughtRsi = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public int OverSoldRsi
        {
            get { return overSoldRsi; }
            set { overSoldRsi = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public int RsiBars
        {
            get { return rsiBars; }
            set { rsiBars = Math.Max(1, value); }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public double AddAtTicks
        {
            get { return addAtTicks; }
            set { addAtTicks = Math.Max(1, value); }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public int StopLoss
        {
            get { return stopLoss; }
            set { stopLoss = Math.Max(1, value); }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public int Limit
        {
            get { return limit; }
            set { limit = Math.Max(1, value); }
        }
        [Description("")]
        [GridCategory("Parameters")]
        public int VolumeStrategyOnOff
        {
            get { return volumeStrategyOnOff; }
            set { volumeStrategyOnOff = Math.Max(0, value); }
        }
		[Description("")]
        [GridCategory("Parameters")]
        public int TargetVolume
        {
            get { return targetVolume; }
            set { targetVolume = Math.Max(0, value); }
        }
        #endregion
    }
}
