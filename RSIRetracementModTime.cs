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
    /// To localise the entry level of the RSI with respect to its extremes.
    /// </summary>
    [Description("To localise the entry level of the RSI with respect to its extremes.")]
    public class RSIRetracementModwTime : Strategy
    {
        #region Variables
        // Wizard generated variables
        private int lookBackRSI = 1; // Default setting for LookBackRSI
        private int entryLevelDif = 0; // Default setting for EntryLevelDif
        private int stopLoss = 25; // Default setting for StopLoss
        private int profit = 40; // Default setting for Profit
        private int rSIPeriod = 26; // Default setting for RSIPeriod
        // User defined variables (add any user defined variables below)
		private double aboveLimit = 0;
		private double belowLimit = 100;
		private int itsOn = 0;
		private int StartingHour = 0;
		private int EndingHour = 23;
        #endregion

        /// <summary>
        /// This method is used to configure the strategy and is called once before any strategy method is called.
        /// </summary>
        protected override void Initialize()
        {
            SetProfitTarget("", CalculationMode.Ticks, Profit);
            SetTrailStop("", CalculationMode.Ticks, StopLoss, false);
            CalculateOnBarClose = false;
			ExitOnClose = true;
		    ExitOnCloseSeconds = 30;
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
			if (Times[0][0].DayOfWeek == DayOfWeek.Monday
				&& Times[0][0].Hour >= StartingHour && Times[0][0].Hour <= EndingHour){
				itsOn =1 ;
			} else {
				itsOn = 0;
			}
			if(CurrentBar < RSIPeriod|| itsOn == 0 ){
				return;
			}
			
			//reset limits
			belowLimit = 100;
			aboveLimit = 0;
			//condition for oversold
			if(RSI(RSIPeriod,3)[0]<33){
				belowLimit = RSI(RSIPeriod,3)[1];
        	    if (CrossAbove(RSI(RSIPeriod,3), belowLimit, 1) 
					&& (RSI(RSIPeriod,3)[0]-belowLimit>=entryLevelDif ))
           	 	{
            	    EnterLong(DefaultQuantity, "");
            	}
			}
			//condition for overbought
			if(RSI(RSIPeriod,3)[0]>67){
				aboveLimit = RSI(RSIPeriod,3)[1];         
            	if (CrossBelow(RSI(RSIPeriod,3), aboveLimit, 1)
					&& ( aboveLimit-RSI(RSIPeriod,3)[0]>=entryLevelDif) )
            	{
                	EnterShort(DefaultQuantity, "");
            	}
			}
        }

        #region Properties
        [Description("number of bars ago to find min/max.")]
        [GridCategory("Parameters")]
        public int LookBackRSI
        {
            get { return lookBackRSI; }
            set { lookBackRSI = Math.Max(1, value); }
        }

        [Description("Diferential from max/min to open position, in RSI percentage.")]
        [GridCategory("Parameters")]
        public int EntryLevelDif
        {
            get { return entryLevelDif; }
            set { entryLevelDif = Math.Max(0, value); }
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
        public int Profit
        {
            get { return profit; }
            set { profit = Math.Max(1, value); }
        }

        [Description("")]
        [GridCategory("Parameters")]
        public int RSIPeriod
        {
            get { return rSIPeriod; }
            set { rSIPeriod = Math.Max(1, value); }
        }
        #endregion
    }
}
