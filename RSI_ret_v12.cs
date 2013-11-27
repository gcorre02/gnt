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
    public class RSI_ret_v12 : Strategy  // rsi updates more live, all arrays[0] point to the most recent one, not the highest of the most recent ones.// longs seem negative while shorts positive ? 
    {
        #region Variables
        // Wizard generated variables

        private int entryLevelDif = 0; // Default setting for EntryLevelDif
        private int stopLoss = 30; // Default setting for StopLoss
        private int profit = 40; // Default setting for Profit
        private int rSIPeriod = 11; // Default setting for RSIPeriod
		private int roundTickDist = 0;
		private int trailStopLoss = 0;
		private int turnTrailOn = 0;
		private int turnExitOn = 0;
		private int lastStopMin = 141;
		private int bidAsk = 3;
		private int priceDif = 1;
		private int isPositiveOn = 1; 
		private int rOCPeriod = 15;
		private double rOCDif = 0.05;
		private int overSoldLevel = 33;
		private int overBoughtLevel = 67;
		private int turnCheckRoundNumOn = 0;
		private int turnRocFilterOn = 1;
		private int turnLastStopMinOn = 0;
		private int longRsiPeriod =9;
		private int longRsiLength = 15;
		private int startingHour =0;
		private int startingMinute = 0;
		private int startingSecond = 0;
		private int startingHour2 =0;
		private int startingMinute2 = 0;
		private int startingSecond2 = 0;
		private int startingHour3 =0;
		private int startingMinute3 = 0;
		private int startingSecond3 = 0;
		private int startingHour4 =0;
		private int startingMinute4 = 0;
		private int startingSecond4 = 0;
		private int startingHour5 =0;
		private int startingMinute5 = 0;
		private int startingSecond5 = 0;
		private int endingHour = 23;
		private int endingMinute = 59;
		private int endingHour2 = 23;
		private int endingMinute2 = 59;
		private int endingHour3 = 23;
		private int endingMinute3 = 59;
		private int endingHour4 = 23;
		private int endingMinute4 = 59;
		private int endingHour5 = 23;
		private int endingMinute5 = 59;
		private bool getOutBeforeWindow = true;
		private int closeMinutesBefore = 15;

        // User defined variables (add any user defined variables below)
		private double aboveLimit = 0;
		private double belowLimit = 100;
		private int itsOn = 0;
		private double longPriceDif =0;
		private double shortPriceDif = 0;
		private int storePreviousRsiMax = 5;
		private Double[] rsiArray;
		private Double[] rsiArray15;
		private Double[] rocArray;
		//private int timeOff = 1;
        #endregion

        /// <summary>
        /// This method is used to configure the §strategy and is called once before any strategy method is called.
        /// </summary>
        protected override void Initialize()
        {
			//sendEmail(); need to configure it first under the settings;
            SetProfitTarget("", CalculationMode.Ticks, Profit);
			if(TurnExitOn ==1){
				ExitOnClose= true;
			} else if(TurnExitOn ==0){
				ExitOnClose = false;
			}
            if(TurnTrailOn == 1){
				SetTrailStop("onlyOrder", CalculationMode.Ticks, TrailStopLoss, false);
			} else {
				SetStopLoss("onlyOrder",CalculationMode.Ticks,StopLoss,false);
			}
			
            CalculateOnBarClose = false;
			rsiArray = new Double[storePreviousRsiMax];
			rsiArray15 = new Double[storePreviousRsiMax];
			rocArray = new Double[storePreviousRsiMax];
			Add(PeriodType.Minute, LongRsiLength);
		}
		protected void sendEmail(){
		//	SendMail("guilherme.ctr@gmail.com", "cbrundan@gmail.com", "NinjaMail", "Hello, mail sending is working. Pretty Simple actually.");
			SendMail("guilherme.ctr@gmail.com", "guilherme.ctr@gmail.com", "NinjaMail", "Hello, mail sending is working. Pretty Simple actually.");

		}
		protected void getRsiRecentHigh(){
			int len = storePreviousRsiMax;
			for (int i = len-1; i > 0; i--){
				rsiArray[i] = rsiArray[i-1];
			}
			rsiArray[0] = RSI(RSIPeriod,3)[0];
		}
		protected void getRsi15RecentHigh(){
			int len = storePreviousRsiMax;
			for (int i = len-1; i > 0; i--){
				rsiArray15[i] = rsiArray15[i-1];
			}
			rsiArray15[0] = RSI(BarsArray[1],LongRsiPeriod,3)[0];
		}
		protected void getRocRecentHigh(){
			int len = storePreviousRsiMax;
			for (int i = len-1; i > 0; i--){
				rocArray[i] = rocArray[i-1];
			}
			rocArray[0] = ROC(ROCPeriod)[0];
		}

		protected String getRsiArrayContents(){
			//this method just prints the contents of the array;
			String result = "";
			for(int i = 0; i < storePreviousRsiMax; i++){
				result = result + " , " + rsiArray[i];
			}
			return result;
		}
		protected bool rocFilter(){
			if(TurnRocFilterOn == 0){
				ROCDif = 0;
			}
			double maxROC = Math.Max(Math.Abs(rocArray[0]),Math.Abs(rocArray[1]));// this only gets the max between current and previous
			if(maxROC>ROCDif){
				return true;
			}  else {
				return false;
			}
			//Need to go through all the code and change double comparisons to something more exact
			/*absolute max value of roc between previous and current bar
			* is this less than optimized value?
			* dont take the trade
			* int ROC  Period(already Defined)
			* double Optimized value
			* 
			*/
		}
		protected bool checkOverBoughtRsi15min(){
			if(rsiArray15[0]>67){
				//Log("15 min RSI is long",LogLevel.Information);
				return true;
			} else {
				return false;
			}
		}
		protected bool checkOverSoldRsi15min(){
			if (rsiArray15[0]<33){
				//Log("15 min RSI is long",LogLevel.Information);
				return true;
			} else {
				return false;
			}
		}

		protected Boolean checkRoundNum(){
			if(TurnCheckRoundNumOn==0){
				RoundTickDist = 0;
			}
			String check = Close[0].ToString();
			int len = check.Length-2;
			double units =100;
			if(len >= 3){
				units = double.Parse(check.Substring(len,2))*0.01/TickSize;
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
      		if(losingTime.TotalSeconds>0 
				&& losingTime.TotalSeconds < LastStopMin
				&& TurnLastStopMinOn == 1){
				recentLoss = true;
				} else {
					recentLoss = false;
				}		    
				if(recentLoss && lastTrade.Entry.MarketPosition == MarketPosition.Long){
					this.longPriceDif=Close[0]-PriceDif*TickSize;
					return "Long";
				} else if(recentLoss && lastTrade.Entry.MarketPosition == MarketPosition.Short){
					this.shortPriceDif=Close[0]+PriceDif*TickSize;
					return "Short";
				} else {
					return "Nop";
				}
			}else {
					return "Nop";
			}
		}
		
		#region TimeFrames
		protected bool timeframe(){
			// check if it is less than 15 minutes away, so strategy can exit any current positions.
			DateTime targetTime = new DateTime(Times[0][0].Year,
				Times[0][0].Month,
				Times[0][0].Day,
				StartingHour, 
				StartingMinute,
				StartingSecond,
				DateTimeKind.Local); //= new DateTime();
			TimeSpan target = new TimeSpan(0,closeMinutesBefore,0);
			TimeSpan zero = new TimeSpan(0,0,0);
			//Print("target is "+target.ToString() + " and difference between setUp and currentTime is" + targetTime.Subtract(Times[0][0]));
			if(targetTime.Subtract(Times[0][0])<target
				&& targetTime.Subtract(Times[0][0])>zero
				&& GetOutBeforeWindow){
				if(Position.MarketPosition == MarketPosition.Long){
					ExitLong("onlyOrder");
				} else if(Position.MarketPosition == MarketPosition.Short){
					ExitShort("onlyOrder");
				}
			}
			//targetTime = DateTime.Parse(StartingHour.ToString() + "/"+StartingMinute.ToString() + "/00");
			//Times[0][0].
			if (Times[0][0].Hour >= StartingHour 
				&& Times[0][0].Minute >= StartingMinute 
				&& Times[0][0].Second>= StartingSecond
				&& Times[0][0].Hour <= EndingHour 
				&& Times[0][0].Minute <= EndingMinute ){
					return true;
				} else {
					return false;
				}
		}
		protected bool timeframe2(){
				DateTime targetTime = new DateTime(Times[0][0].Year,
				Times[0][0].Month,
				Times[0][0].Day,
				StartingHour2, 
				StartingMinute2,
				StartingSecond2,
				DateTimeKind.Local); //= new DateTime();
			TimeSpan target = new TimeSpan(0,closeMinutesBefore,0);
			TimeSpan zero = new TimeSpan(0,0,0);
			//Print("target is "+target.ToString() + " and difference between setUp and currentTime is" + targetTime.Subtract(Times[0][0]));
			if(targetTime.Subtract(Times[0][0])<target
				&& targetTime.Subtract(Times[0][0])>zero
				&& GetOutBeforeWindow){
				if(Position.MarketPosition == MarketPosition.Long){
					ExitLong("onlyOrder");
				} else if(Position.MarketPosition == MarketPosition.Short){
					ExitShort("onlyOrder");
				}
			}
			if (//Times[0][0].DayOfWeek == DayOfWeek.Monday && 
				Times[0][0].Hour >= StartingHour2 
				&& Times[0][0].Minute >= StartingMinute2 
				&& Times[0][0].Second>= StartingSecond2
				&& Times[0][0].Hour <= EndingHour2 
				&& Times[0][0].Minute <= EndingMinute2 ){
					return true;
				} else {
					return false;
				}
		}
		protected bool timeframe3(){
				DateTime targetTime = new DateTime(Times[0][0].Year,
				Times[0][0].Month,
				Times[0][0].Day,
				StartingHour3, 
				StartingMinute3,
				StartingSecond3,
				DateTimeKind.Local); //= new DateTime();
			TimeSpan target = new TimeSpan(0,closeMinutesBefore,0);
			TimeSpan zero = new TimeSpan(0,0,0);
			//Print("target is "+target.ToString() + " and difference between setUp and currentTime is" + targetTime.Subtract(Times[0][0]));
			if(targetTime.Subtract(Times[0][0])<target
				&& targetTime.Subtract(Times[0][0])>zero
				&& GetOutBeforeWindow){
				if(Position.MarketPosition == MarketPosition.Long){
					ExitLong("onlyOrder");
				} else if(Position.MarketPosition == MarketPosition.Short){
					ExitShort("onlyOrder");
				}
			}
			if (//Times[0][0].DayOfWeek == DayOfWeek.Monday && 
				Times[0][0].Hour >= StartingHour3 
				&& Times[0][0].Minute >= StartingMinute3 
				&& Times[0][0].Second>= StartingSecond3
				&& Times[0][0].Hour <= EndingHour3 
				&& Times[0][0].Minute <= EndingMinute3 ){
					return true;
				} else {
					return false;
				}
		}
		protected bool timeframe4(){
				DateTime targetTime = new DateTime(Times[0][0].Year,
				Times[0][0].Month,
				Times[0][0].Day,
				StartingHour4, 
				StartingMinute4,
				StartingSecond4,
				DateTimeKind.Local); //= new DateTime();
			TimeSpan target = new TimeSpan(0,closeMinutesBefore,0);
			TimeSpan zero = new TimeSpan(0,0,0);
			//Print("target is "+target.ToString() + " and difference between setUp and currentTime is" + targetTime.Subtract(Times[0][0]));
			if(targetTime.Subtract(Times[0][0])<target
				&& targetTime.Subtract(Times[0][0])>zero
				&& GetOutBeforeWindow){
				if(Position.MarketPosition == MarketPosition.Long){
					ExitLong("onlyOrder");
				} else if(Position.MarketPosition == MarketPosition.Short){
					ExitShort("onlyOrder");
				}
			}
			if (//Times[0][0].DayOfWeek == DayOfWeek.Monday && 
				Times[0][0].Hour >= StartingHour4 
				&& Times[0][0].Minute >= StartingMinute4 
				&& Times[0][0].Second>= StartingSecond4
				&& Times[0][0].Hour <= EndingHour4 
				&& Times[0][0].Minute <= EndingMinute4 ){
					return true;
				} else {
					return false;
				}
		}
		protected bool timeframe5(){
				DateTime targetTime = new DateTime(Times[0][0].Year,
				Times[0][0].Month,
				Times[0][0].Day,
				StartingHour5, 
				StartingMinute5,
				StartingSecond5,
				DateTimeKind.Local); //= new DateTime();
			TimeSpan target = new TimeSpan(0,closeMinutesBefore,0);
			TimeSpan zero = new TimeSpan(0,0,0);
			//Print("target is "+target.ToString() + " and difference between setUp and currentTime is" + targetTime.Subtract(Times[0][0]));
			if(targetTime.Subtract(Times[0][0])<target
				&& targetTime.Subtract(Times[0][0])>zero
				&& GetOutBeforeWindow){
				if(Position.MarketPosition == MarketPosition.Long){
					ExitLong("onlyOrder");
				} else if(Position.MarketPosition == MarketPosition.Short){
					ExitShort("onlyOrder");
				}
			}
			if (//Times[0][0].DayOfWeek == DayOfWeek.Monday && 
				Times[0][0].Hour >= StartingHour5 
				&& Times[0][0].Minute >= StartingMinute5 
				&& Times[0][0].Second>= StartingSecond5
				&& Times[0][0].Hour <= EndingHour5 
				&& Times[0][0].Minute <= EndingMinute5 ){
					return true;
				} else {
					return false;
				}
		}
		#endregion
		
        protected override void OnBarUpdate()
        {
			if((GetCurrentAsk()-GetCurrentBid())<BidAsk*TickSize 
				&& checkRoundNum()){
				itsOn = 1;
			} else {
				itsOn = 0;
			}
			timeframe();
			timeframe2();
			timeframe3();
			timeframe4();
			timeframe5();
			if(CurrentBar < RSIPeriod || itsOn == 0){
				return;
			}
			
			//reset limits
			belowLimit = 100;
			aboveLimit = 0;
			//Load rsi values to array
			getRsiRecentHigh();
			getRsi15RecentHigh();
			getRocRecentHigh();
			//Log(getRsiArrayContents(),LogLevel.Information);
			
			//condition for oversold
			
			Boolean shortIsPositive = true;
			if (Position.MarketPosition == MarketPosition.Short){ 
         		if( Position.GetProfitLoss(Close[0], PerformanceUnit.Points)<0 
					&& IsPositiveOn == 1){
					shortIsPositive = false;
				}
			}
			if(rsiArray[0]<OverSoldLevel){
				belowLimit = rsiArray[1];
        	    if (CrossAbove(RSI(RSIPeriod,3), belowLimit, 1) 
					&& (RSI(RSIPeriod,3)[0]-belowLimit>=entryLevelDif )
					&& checkRecentLossDirection() != "Long"
					&& (Close[0] < this.longPriceDif|| this.longPriceDif == 0)
					&& shortIsPositive
					&& rocFilter()
					&& !checkOverBoughtRsi15min()
					&& (timeframe()
					|| timeframe2()
					|| timeframe3()
					|| timeframe4()
					|| timeframe5()))
           	 	{
					this.longPriceDif = 0;
            	    EnterLong(DefaultQuantity, "onlyOrder");
            	}
			}
			//condition for overbought
			Boolean longIsPositive = true;
			if (Position.MarketPosition == MarketPosition.Long){ 
         		if( Position.GetProfitLoss(Close[0], PerformanceUnit.Points)<0 
					&& IsPositiveOn == 1){
					longIsPositive = false;
				}
			}
			if(rsiArray[0]>OverBoughtLevel){
				aboveLimit = rsiArray[1];         
            	if (CrossBelow(RSI(RSIPeriod,3), aboveLimit, 1)
					&& ( aboveLimit-RSI(RSIPeriod,3)[0]>=entryLevelDif) 
					&& checkRecentLossDirection() != "Short"
					&& (Close[0] > this.shortPriceDif || this.shortPriceDif == 0)
					&& longIsPositive
					&& rocFilter()
					&& !checkOverSoldRsi15min()
					&& (timeframe()
					|| timeframe2()
					|| timeframe3()
					|| timeframe4()
					|| timeframe5())
					)
            	{
					this.shortPriceDif= 0;
                	EnterShort(DefaultQuantity, "onlyOrder");
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
	
		[Description("")]
        [GridCategory("Parameters")]
        public int PriceDif
        {
            get { return priceDif; }
            set { priceDif = Math.Max(0, value); }
        }
	
		[Description("")]
        [GridCategory("Parameters")]
        public int IsPositiveOn	
        {
            get { return isPositiveOn; }
            set { isPositiveOn = Math.Max(0, value); }
        }
		
		[Description("")]
        [GridCategory("Parameters")]
        public int ROCPeriod
        {
            get { return rOCPeriod; }
            set { rOCPeriod = Math.Max(1, value); }
        }
				[Description("")]
		
        [GridCategory("Parameters")]
        public double ROCDif
        {
            get { return rOCDif; }
            set { rOCDif = Math.Max(0, value); }
        }
		
		[GridCategory("Parameters")]
        public int BidAsk
        {
            get { return bidAsk; }
            set { bidAsk = Math.Max(1, value); }
        }
		[GridCategory("Parameters")]
        public int OverSoldLevel
        {
            get { return overSoldLevel; }
            set { overSoldLevel = Math.Max(1, value); }
        }
		[GridCategory("Parameters")]
        public int OverBoughtLevel
        {
            get { return overBoughtLevel; }
            set { overBoughtLevel = Math.Max(1, value); }
        }
		[GridCategory("Parameters")]
        public int LongRsiLength 
        {
            get { return longRsiLength ; }
            set { longRsiLength  = Math.Max(0, value); }
        }

		[GridCategory("Parameters")]
        public int TurnCheckRoundNumOn
        {
            get { return turnCheckRoundNumOn; }
            set { turnCheckRoundNumOn = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int TurnLastStopMinOn
        {
            get { return turnLastStopMinOn; }
            set { turnLastStopMinOn = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int LongRsiPeriod
        {
            get { return longRsiPeriod; }
            set { longRsiPeriod = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int TurnRocFilterOn
        {
            get { return turnRocFilterOn; }
            set { turnRocFilterOn = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int StartingHour
        {
            get { return startingHour; }
            set { startingHour = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int StartingMinute
        {
            get { return startingMinute; }
            set { startingMinute = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int StartingSecond
        {
            get { return startingSecond; }
            set { startingSecond = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int StartingHour2
        {
            get { return startingHour2; }
            set { startingHour2 = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int StartingMinute2
        {
            get { return startingMinute2; }
            set { startingMinute2 = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int StartingSecond2
        {
            get { return startingSecond2; }
            set { startingSecond2 = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int StartingHour3
        {
            get { return startingHour3; }
            set { startingHour3 = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int StartingMinute3
        {
            get { return startingMinute3; }
            set { startingMinute3 = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int StartingSecond3
        {
            get { return startingSecond3; }
            set { startingSecond3 = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int StartingHour4
        {
            get { return startingHour4; }
            set { startingHour4 = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int StartingMinute4
        {
            get { return startingMinute4; }
            set { startingMinute4 = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int StartingSecond4
        {
            get { return startingSecond4; }
            set { startingSecond4 = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int StartingHour5
        {
            get { return startingHour5; }
            set { startingHour5= Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int StartingMinute5
        {
            get { return startingMinute5; }
            set { startingMinute5 = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int StartingSecond5
        {
            get { return startingSecond5; }
            set { startingSecond5 = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int EndingHour 
        {
            get { return endingHour; }
            set { endingHour= Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int EndingMinute 
        {
            get { return endingMinute ; }
            set { endingMinute = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int EndingHour2
        {
            get { return endingHour2; }
            set { endingHour2= Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int EndingMinute2
        {
            get { return endingMinute2; }
            set { endingMinute2 = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int EndingHour3
        {
            get { return endingHour3; }
            set { endingHour3= Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int EndingMinute3
        {
            get { return endingMinute3; }
            set { endingMinute3 = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int EndingHour4
        {
            get { return endingHour4; }
            set { endingHour4= Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int EndingMinute4
        {
            get { return endingMinute4; }
            set { endingMinute4 = Math.Max(0, value); }
        }
		[GridCategory("Parameters")]
        public int EndingHour5
        {
            get { return endingHour5; }
            set { endingHour5= Math.Max(0, value); }
        }
		
		[GridCategory("Parameters")]
        public int CloseMinutesBefore
        {
            get { return closeMinutesBefore; }
            set { closeMinutesBefore= Math.Max(0, value); }
        }

		[GridCategory("Parameters")]
        public int EndingMinute5
        {
            get { return endingMinute5; }
            set { endingMinute5= Math.Max(0, value); }
        }
		
		[GridCategory("Parameters")]
        public bool GetOutBeforeWindow
        {
            get { return getOutBeforeWindow; }
            set { getOutBeforeWindow =value; }
        }



        #endregion
    }
}