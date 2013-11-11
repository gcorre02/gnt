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
    public class Friday : Strategy
    {
        #region Variables
        // Wizard generated variables

        private int entryLevelDif = 0; // Default setting for EntryLevelDif
        private int stopLoss = 25; // Default setting for StopLoss
        private int profit = 100; // Default setting for Profit
        private int rSIPeriod = 26; // Default setting for RSIPeriod
		private int roundTickDist = 10;
		private int trailStopLoss = 1;
		private int turnTrailOn = 0;
		private int turnExitOn = 0;
		private int lastStopMin = 0;
        // User defined variables (add any user defined variables below)
		private double aboveLimit = 0;
		private double belowLimit = 100;
		private int itsOn = 0;
		private int timeOff = 1;
        #endregion

        /// <summary>
        /// This method is used to configure the §strategy and is called once before any strategy method is called.
        /// </summary>
        protected override void Initialize()
        {
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
		protected String checkRecentLossDirection(){
			Boolean recentLoss = false;
 			// Check to make sure there is at least one trade in the collection
    		if (Performance.AllTrades.LosingTrades.Count > 0)
    		{
				//references last trade
				Trade lastTrade = Performance.AllTrades.LosingTrades[Performance.AllTrades.LosingTrades.Count -1 ];
       			//gets the difference in time between last losing and current time
				TimeSpan losingTime =( Times[0][0].TimeOfDay - lastTrade.ExitExecution.Time.TimeOfDay );
				//prints to log that difference
				//Log(losingTime.TotalSeconds.ToString(), LogLevel.Information);
      		if(losingTime.TotalSeconds>0 && losingTime.TotalSeconds < LastStopMin){
				recentLoss = true;
				} else {
					recentLoss = false;
				}
		    
				if(recentLoss && lastTrade.Entry.MarketPosition == MarketPosition.Long){
					return "Long";
				} else if(recentLoss && lastTrade.Entry.MarketPosition == MarketPosition.Short){
					return "Short";
				} else {
					return "Nop";
				}
			}else {
					return "Nop";
			}
		}
        protected override void OnBarUpdate()
        {
//			Boolean recentLoss = false;
// 			// Check to make sure there is at least one trade in the collection
//    		if (Performance.AllTrades.LosingTrades.Count > 0)
//    		{
//				//references last trade
//			  	Trade lastTrade = Performance.AllTrades.LosingTrades[Performance.AllTrades.LosingTrades.Count -1 ];
//       		//gets the difference in time between last losing and current time
//				TimeSpan losingTime =( Times[0][0].TimeOfDay - lastTrade.ExitExecution.Time.TimeOfDay );
//				//prints to log that difference
//				//Log(losingTime.TotalSeconds.ToString(), LogLevel.Information);
  //      		if(losingTime.TotalSeconds>0 && losingTime.TotalSeconds < LastStopMin){
	//				recentLoss = true;
//				} else {
//					recentLoss = false;
//				}
//		    }

			if (Times[0][0].DayOfWeek == DayOfWeek.Friday){
				//&& Times[0][0].Hour >= StartingHour && Times[0][0].Hour <= EndingHour){
				timeOff =1 ;
			} else {
				timeOff = 0;
			}
			if((GetCurrentAsk()-GetCurrentBid())<3*TickSize && checkRoundNum() ){//&& !recentLoss){
				itsOn = 1;
				//double num = (GetCurrentAsk()-GetCurrentBid());
				//Log(num.ToString() + " "+ 3 * TickSize,LogLevel.Information);
			} else {
				itsOn = 0;
			}
			if(CurrentBar < RSIPeriod || itsOn == 0 || timeOff ==0 ){
				return;
			}
			
			//reset limits
			belowLimit = 100;
			aboveLimit = 0;
			//condition for oversold
			if(RSI(RSIPeriod,3)[0]<33){
				belowLimit = RSI(RSIPeriod,3)[1];
        	    if (CrossAbove(RSI(RSIPeriod,3), belowLimit, 1) 
					&& (RSI(RSIPeriod,3)[0]-belowLimit>=entryLevelDif )
					&& checkRecentLossDirection() != "Long")
           	 	{
            	    EnterLong(DefaultQuantity, "");
            	}
			}
			//condition for overbought
			if(RSI(RSIPeriod,3)[0]>67){
				aboveLimit = RSI(RSIPeriod,3)[1];         
            	if (CrossBelow(RSI(RSIPeriod,3), aboveLimit, 1)
					&& ( aboveLimit-RSI(RSIPeriod,3)[0]>=entryLevelDif) 
					&& checkRecentLossDirection() != "Short")
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
        public int LastStopMin
        {
            get { return lastStopMin; }
            set { lastStopMin = Math.Max(0, value); }
        }

		[Description("")]
        [GridCategory("Parameters")]
        public int TurnExitOn
        {
            get { return turnExitOn; }
            set { turnExitOn = Math.Max(0, value); }
        }
        #endregion
    }
}
