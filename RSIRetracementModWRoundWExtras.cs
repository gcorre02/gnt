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
    public class RSIv20 : Strategy
    {
        #region Variables
        // Wizard generated variables
        private int entryLevelDif = 0; // Default setting for EntryLevelDif
        private int stopLoss = 15; // Default setting for StopLoss
        private int profit = 40; // Default setting for Profit
        private int rSIPeriod = 26; // Default setting for RSIPeriod
		private int roundTickDist = 8;
		private int trailStopLoss = 3;
		private int turnTrailOn = 0;
		private int turnExitOn = 0;
		private int setBreakEven = 15;
        // User defined variables (add any user defined variables below)
		private double aboveLimit = 0;
		private double belowLimit = 100;
		private int itsOn = 0;
		private IOrder stopOrder = null;
        #endregion

        /// <summary>
        /// This method is used to configure the strategy and is called once before any strategy method is called.
        /// </summary>
        protected override void Initialize()
        {
			//Unmanaged = true;
            SetProfitTarget("", CalculationMode.Ticks, Profit);
			if(TurnExitOn ==1){
				ExitOnClose= true;
			} else if(TurnExitOn ==0){
				ExitOnClose = false;
			}
            if(TurnTrailOn == 1){
				SetTrailStop("", CalculationMode.Ticks, TrailStopLoss, false);
			} else {
				SetStopLoss(CalculationMode.Ticks,StopLoss);
			}
            CalculateOnBarClose = false;
        }

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
		protected Boolean checkRoundNum(){
			String check = Close[0].ToString();
			int len = check.Length-2;
			double units =100;
			if(len >= 3){
				units = double.Parse(check.Substring(len,2))*0.01/TickSize;
				//Log(Close[0].ToString() + " "+ units.ToString(), LogLevel.Information);
			}
			if (units >= 100 -RoundTickDist || units <= roundTickDist){
				return false;
			} else {
				return true;
			}
		}
        protected override void OnBarUpdate()
        {
//			if (Times[0][0].DayOfWeek == DayOfWeek.Monday 
//				|| Times[0][0].DayOfWeek == DayOfWeek.Thursday
//				&& Times[0][0].Hour >= StartingHour && Times[0][0].Hour <= EndingHour){
//				itsOn =1 ;
//			} else {
//				itsOn = 0;
//			}
			  // Raise stop loss to breakeven when you are at least 4 ticks in profit
		    if (stopOrder != null && stopOrder.StopPrice < Position.AvgPrice && Close[0] >= Position.AvgPrice + SetBreakEven * TickSize)
        		 ChangeOrder(stopOrder, stopOrder.Quantity, stopOrder.LimitPrice, Position.AvgPrice);

			//check if current is near a round number
			if((GetCurrentAsk()-GetCurrentBid())<3*TickSize && checkRoundNum()){
				itsOn = 1;
				//double num = (GetCurrentAsk()-GetCurrentBid());
				//Log(num.ToString() + " "+ 3 * TickSize,LogLevel.Information);
			} else {
				itsOn = 0;
			}
			if(CurrentBar < RSIPeriod || itsOn == 0 ){
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
        [Description("")]
        [GridCategory("Parameters")]
        public int RoundTickDist
        {
            get { return roundTickDist; }
            set { roundTickDist = Math.Max(0, value); }
        }
		[Description("")]
        [GridCategory("Parameters")]
        public int TrailStopLoss
        {
            get { return trailStopLoss; }
            set { trailStopLoss = Math.Max(0, value); }
        }
		
		[Description("")]
        [GridCategory("Parameters")]
        public int TurnTrailOn
        {
            get { return turnTrailOn; }
            set { turnTrailOn = Math.Max(0, value); }
        }
		
		[Description("")]
        [GridCategory("Parameters")]
        public int TurnExitOn
        {
            get { return turnExitOn; }
            set { turnExitOn = Math.Max(0, value); }
        }
		
		[Description("")]
        [GridCategory("Parameters")]
        public int SetBreakEven
        {
            get { return setBreakEven; }
            set { setBreakEven = Math.Max(0, value); }
        }
		
        #endregion
    }
}
