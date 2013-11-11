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
    /// Mean Revertion strategy on boolean lines
    /// </summary>
    [Description("Mean Revertion strategy on boolean lines")]
    public class BollMean : Strategy
    {
        #region Variables
        // Wizard generated variables
        private int bollBars = 125; // Default setting for BollBars
        private double numberOfStd = 3.4; // Default setting for NumberOfStd
        private double tickLimit = 43; // Default setting for TickLimit
        private double tickStop = 19; // Default setting for TickStop
        private int profitTarget = 0; // Default setting for limit position : open long tick differential and Default setting for open short tick differential
        private double losstarget = 0.001; // define minimum standard deviation
        private int positionSize = 3; // Default setting for PositionSize
		//private double entryTarget = 0;
		private double readStDev = 0; // make it a user input checks the std deviation value
        // User defined variables (add any user defined variables below)
		//private double limitDayLoss = -200;//-2000
		//private double currentDayLoss = 0 ;
        #endregion

        /// <summary>
        /// This method is used to configure the strategy and is called once before any strategy method is called.
        /// </summary>
        protected override void Initialize()
        {
            Add(Bollinger(NumberOfStd, BollBars));
            SetProfitTarget("", CalculationMode.Ticks, TickLimit);
            SetStopLoss("", CalculationMode.Ticks, TickStop, false);

            CalculateOnBarClose = false;
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
			//currentDayLoss = GetAtmStrategyRealizedProfitLoss("id");
		//	if(currentDayLoss < limitDayLoss){
		//		positionSize = 0;
		//		AtmStrategyClose("idValue");
		//	}
			readStDev = StdDev(bollBars)[0];
			//Log("Current Standard dev = " + readStDev+ " at " + Time[0].ToString(),LogLevel.Information );
            // Condition set 1
            if (CrossBelow(Close, Bollinger(NumberOfStd, BollBars).Lower, 1)&& readStDev>=Losstarget)
            {	
				EnterLongLimit(0,true,PositionSize, (Close[0] - 0.01*ProfitTarget) , "");
				//entryTarget = Close[0] - 0.01*Losstarget;
			//}
			//if (entryTarget >= Close[0]){
                //EnterLongStopLimit(PositionSize, ProfitTarget, Losstarget, "");
				//EnterLong(PositionSize, "");
				//entryTarget = 0;
				//Log( "Created Long postition at " + DateTime.Now.ToString() + " " + Close[0].ToString(), NinjaTrader.Cbi.LogLevel.Alert);
            }
			if (CrossAbove(Close, Bollinger(NumberOfStd, BollBars).Upper, 1)&& readStDev>=Losstarget)
            {
				EnterShortLimit(0,true,PositionSize,(Close[0] + 0.01*profitTarget), "");
            // Condition set 2
            //if (CrossAbove(Close, Bollinger(NumberOfStd, BollBars).Upper, 1))
            //{
			//	entryTarget = Close[0] + 0.01*profitTarget;
			//}
			//if(entryTarget <= Close[0]){
              //  EnterShort(PositionSize, "");
				//entryTarget=0;
				//Log( "Created Long postition at " + DateTime.Now.ToString() + " " + Close[0].ToString(), NinjaTrader.Cbi.LogLevel.Alert);
            }
        }

        #region Properties
        [Description("Number of previous bars to average")]
        [GridCategory("Parameters")]
        public int BollBars
        {
            get { return bollBars; }
            set { bollBars = Math.Max(0, value); }
        }

        [Description("Number of Standard Deviation amplitude")]
        [GridCategory("Parameters")]
        public double NumberOfStd
        {
            get { return numberOfStd; }
            set { numberOfStd = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public double TickLimit
        {
            get { return tickLimit; }
            set { tickLimit = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public double TickStop
        {
            get { return tickStop; }
            set { tickStop = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public int ProfitTarget
        {
            get { return profitTarget; }
            set { profitTarget = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public double Losstarget
        {
            get { return losstarget; }
            set { losstarget = Math.Max(0, value); }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public int PositionSize
        {
            get { return positionSize; }
            set { positionSize = Math.Max(0, value); }
        }
        #endregion
    }
}
