////////////////////////////////////////////////
// Copyright (C) 2020, github.com/iniguezdj
// Do NOT use real money to trade this strategy
////////////////////////////////////////////////

#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.NinjaScript;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
#endregion

namespace NinjaTrader.NinjaScript.Strategies {

    public class InsideBar : Strategy {

        public int TickQty;
        public string Mini;
        public string Email;
        public int ATRLength;
        public double ATRMultiplier;
        public double ATRStop;
        public string instrument;
        public int SignalBar;
        private Order entry = null;
        private Order initialStop = null;
        private Order stop = null;
        public int Lookback;
        double LongLimit = 0.0;
        double ShortLimit = 0.0;
        public double RealizedPnL;
        public int MorningStart;
        public int MorningEnd;
        public int AfternoonStart;
        public int AfternoonEnd;
        public int AfternoonClose;
        public int Offline;
        public int Dusk;
        public int Dawn;

        protected override void OnStateChange () {

            if (State == State.SetDefaults) {

                Description = @"Signals based off inside bar breakouts";
                Name = "InsideBar";
                Email = "youremail@email.com";
                ATRLength = 3;
                ATRMultiplier = 2.0;
                ContractQty = 1;
                MorningStart = ToTime (6, 30, 00);
                MorningEnd = ToTime (10, 00, 00);
                AfternoonStart = ToTime (10, 00, 00);
                AfternoonEnd = ToTime (12, 59, 00);
                AfternoonClose = ToTime (12, 59, 00);
                Offline = ToTime (13, 00, 00);
                Dusk = ToTime (23, 59, 59);
                Dawn = ToTime (00, 00, 59);
                Calculate = Calculate.OnPriceChange; // Set to OnBarClose to wait until the bar closes
                EntriesPerDirection = 1;
                EntryHandling = EntryHandling.AllEntries;
                IsExitOnSessionCloseStrategy = true;
                ExitOnSessionCloseSeconds = 30;
                IsFillLimitOnTouch = true;
                MaximumBarsLookBack = MaximumBarsLookBack.TwoHundredFiftySix;
                OrderFillResolution = OrderFillResolution.Standard;
                Slippage = 0;
                StartBehavior = StartBehavior.WaitUntilFlatSynchronizeAccount;
                TimeInForce = TimeInForce.Gtc;
                TraceOrders = false;
                RealtimeErrorHandling = RealtimeErrorHandling.StopCancelClose;
                StopTargetHandling = StopTargetHandling.PerEntryExecution;
                BarsRequiredToTrade = 5;
                IsInstantiatedOnEachOptimizationIteration = true;
                ConnectionLossHandling = ConnectionLossHandling.KeepRunning;
                IsAdoptAccountPositionAware = true;

            } else if (State == State.Configure) {

                ////////////////////////////////////
                // Add ES Data Series
                ////////////////////////////////////
                AddDataSeries ("ES 09-20", BarsPeriodType.Minute, 1); // Must update contract (12-20) when it rolls over quarterly

            } else if (State == State.DataLoaded) {

                instrument = this.Instrument.ToString ().Substring (0, 2);

                ////////////////////////////////////
                // Get instument tick value
                ////////////////////////////////////
                switch (instrument) {
                    case "ES":
                        Print ("ES tick value 12.50");
                        TickQty = 4;
                        break;
                    case "NQ":
                        Print ("NQ tick value 5.00");
                        TickQty = 10;
                        break;
                    case "MES":
                        Print ("MES tick value 1.25");
                        TickQty = 4;
                        break;
                    case "MNQ":
                        Print ("MNQ tick value 0.50");
                        TickQty = 10;
                        break;
                    case "MYM":
                        Print ("MYM tick value 0.50");
                        TickQty = 10;
                        break;
                    case "M2K":
                        Print ("M2KY tick value 0.50");
                        TickQty = 10;
                        break;
                    default:
                        Print ("Instrument not found...");
                        break;
                }

            }

        }

        protected override void OnBarUpdate () {

            ////////////////////////////////////
            // Check for 1st bar of session
            //////////////////////////////////// 
            if (Bars.IsFirstBarOfSession && IsFirstTickOfBar) {

                // Good to go

            }

            ////////////////////////////////////
            // Check for enough bars
            //////////////////////////////////// 
            if (CurrentBars[0] <= BarsRequiredToTrade || CurrentBars[1] <= BarsRequiredToTrade) {

                return;

            }

            ////////////////////////////////////
            // Primary data series (MES)
            //////////////////////////////////// 
            if (BarsInProgress == 0) {

                if (CurrentBar < BarsRequiredToTrade) {
                    return;
                }

                ////////////////////////////////////
                // Check for weekday
                ////////////////////////////////////
                if (Time[0].DayOfWeek != DayOfWeek.Saturday && Time[0].DayOfWeek != DayOfWeek.Sunday) {

                    ///////////////////////////////////////
                    // Signals
                    ///////////////////////////////////////

                    bool Inside = (High[1] < High[2]) && (Low[1] > Low[2]);
                    bool Signal = Inside;

                    ////////////////////////////////////
                    // Market Open
                    ////////////////////////////////////
                    if (ToTime (Time[0]) == MorningStart) {
                        Print (this.Name + " is ONLINE " + Time[0].ToString ());
                    }

                    ////////////////////////////////////
                    // 12:50 - 13:00 Close Positions
                    ////////////////////////////////////
                    if ((ToTime (Time[0]) >= AfternoonClose) && (ToTime (Time[0]) <= Offline)) {

                        // Close any open positions before 1pm
                        Print ("Closing positions " + Time[0].ToString ());
                        ExitLong ();
                        ExitShort ();

                    }

                    ////////////////////////////////////
                    // Market Closed
                    ////////////////////////////////////
                    if (ToTime (Time[0]) == Offline) {

                        Print (this.Name + " is OFFLINE " + Time[0].ToString ());

                    }

                    ////////////////////////////////////
                    // ETH
                    ////////////////////////////////////
                    if ((ToTime (Time[0]) >= Offline && ToTime (Time[0]) <= Dusk) || (ToTime (Time[0]) >= Dawn && ToTime (Time[0]) < MorningStart)) {

                        // ETH Globex Hours

                    }

                    ////////////////////////////////////
                    // Lunch Hours
                    ////////////////////////////////////
                    if ((ToTime (Time[0]) >= MorningStart && ToTime (Time[0]) < AfternoonEnd)) {

                        if (Position.MarketPosition == MarketPosition.Long) {

                            if (CurrentBar == SignalBar) {

                                // ?

                            } else {

                                ////////////////////////////////////
                                // ATR Stop Management
                                ////////////////////////////////////
                                if ((High[1] - (ATRMultiplier * (ATR (ATRLength) [1]))) > ATRStop) { // ATR stops

                                    ATRStop = High[1] - (ATRMultiplier * (ATR (ATRLength) [1]));
                                    ExitLongStopMarket (0, true, 1, ATRStop, "initialStop", "entry");
                                    Dot stopDot = Draw.Dot (this, "stopOrderRunner" + CurrentBar, true, 0, ATRStop, Brushes.WhiteSmoke);
                                    Print (this.Name);
                                    Print ("This bar is " + CurrentBar);
                                    Print ("Moving ATR stop at " + ATRStop);
                                    Print (Time[0].ToString ());
                                    Print ("-----------------------------");

                                } else {

                                    Dot stoptDot = Draw.Dot (this, "stopOrderRunner" + CurrentBar, true, 0, ATRStop, Brushes.WhiteSmoke);

                                }

                            }
                        }

                        if (Position.MarketPosition == MarketPosition.Short) {

                            if (CurrentBar == SignalBar) {

                                // ?

                            } else {

                                ////////////////////////////////////
                                // ATR Stop Management
                                ////////////////////////////////////
                                if ((Low[1] + (ATRMultiplier * (ATR (ATRLength) [1]))) < ATRStop) {

                                    ATRStop = Low[1] + (ATRMultiplier * (ATR (ATRLength) [1]));
                                    ExitShortStopMarket (0, true, 1, ATRStop, "initialStop", "entry");
                                    Dot stopDot = Draw.Dot (this, "stopOrderRunner" + CurrentBar, true, 0, ATRStop, Brushes.WhiteSmoke);
                                    Print (this.Name);
                                    Print ("This bar is " + CurrentBar);
                                    Print ("Moving ATR stop at " + ATRStop);
                                    Print (Time[0].ToString ());
                                    Print ("-----------------------------");

                                } else {

                                    Dot stopDot = Draw.Dot (this, "stopOrderRunner" + CurrentBar, true, 0, ATRStop, Brushes.WhiteSmoke);

                                }

                            }

                        }

                    }

                    ////////////////////////////////////
                    // Tradable Hours
                    ////////////////////////////////////
                    if ((ToTime (Time[0]) >= MorningStart && ToTime (Time[0]) < MorningEnd) || (ToTime (Time[0]) >= AfternoonStart && ToTime (Time[0]) < AfternoonEnd)) {

                        if (Position.MarketPosition == MarketPosition.Flat) {

                            if (PositionAccount.Quantity == 0) {

                                SignalBar = 0;

                                if (Signal) {

                                    if (CrossAbove (Close, High[2], 1)) {
                                        EnterLong (0, ContractQty, "entry");
                                        SignalBar = CurrentBar;
                                        Print (this.Name);
                                        Print ("Long !!");
                                        Print ("CurrentBar " + CurrentBar);
                                        Print ("SignalBar " + SignalBar);
                                        Print (Time[0].ToString ());
                                        Print ("-----------------------------");
                                        BackBrush = Brushes.Green;
                                        BarBrush = Brushes.White;
                                    }

                                    if (CrossBelow (Close, Low[2], 1)) {
                                        EnterShort (0, ContractQty, "entry");
                                        SignalBar = CurrentBar;
                                        Print (this.Name);
                                        Print ("Short !!");
                                        Print ("CurrentBar " + CurrentBar);
                                        Print ("SignalBar " + SignalBar);
                                        Print (Time[0].ToString ());
                                        Print ("-----------------------------");
                                        BackBrush = Brushes.Red;
                                        BarBrush = Brushes.White;
                                    }

                                }

                            }

                        }

                    } else {

                        BackBrush = Brushes.LightGray;

                    }

                }

            }

            ////////////////////////////////////
            // Secondary Data Series (ES)
            ////////////////////////////////////
            if (BarsInProgress == 1) {

                // Print ("BarsInProgress[1] is ES ----------");

            }

        }

        protected override void OnOrderUpdate (Order order, double limitPrice, double stopPrice, int quantity, int filled, double averageFillPrice, OrderState orderState, DateTime time, ErrorCode error, string nativeError) {

            if (order.Name == "initialStop") {

                initialStop = order;

            }

            if (order.Name == "stop") {

                stop = order;

            }

            ////////////////////////////////////
            // Initial Stop Order Exists
            ////////////////////////////////////
            if (initialStop != null && initialStop == order) {

                if (order.OrderState == OrderState.Cancelled) {

                    // Reset order
                    initialStop = null;

                }

            }

            ////////////////////////////////////
            // Stop Order Exists
            ////////////////////////////////////
            if (stop != null && stop == order) {

                if (order.OrderState == OrderState.Cancelled) {

                    // Reset order
                    stop = null;

                }

            }

        }

        protected override void OnExecutionUpdate (Execution execution, string executionId, double price, int quantity, MarketPosition marketPosition, string orderId, DateTime time) {

            SendMail (Email, this.Name, "[" + instrument + "] " + execution.ToString () + " [" + Account.Name + "] [" + Position.MarketPosition + "]");
            Print (this.Name + " [" + instrument + "]");
            Print (execution.ToString ());
            Print (Time[0].ToString ());
            Print ("-----------------------------");

        }

        protected override void OnPositionUpdate (Position position, double averagePrice, int quantity, MarketPosition marketPosition) {

            if (Position.MarketPosition == MarketPosition.Long) {

                ////////////////////////////////////
                // Determine ATR Target
                ////////////////////////////////////
                var target = Position.AveragePrice + (ATR (14) [0]);

                ATRStop = High[1] - (ATRMultiplier * (ATR (ATRLength) [1]));
                ExitLongStopMarket (0, true, 1, ATRStop, "initialStop", "entry");
                Print (this.Name + " [" + instrument + "]");
                Print ("[" + instrument + "] Long " + Position.AveragePrice);
                Print (Time[0].ToString ());
                Print ("-----------------------------");
                Dot stopDot = Draw.Dot (this, "initialStop" + CurrentBar, true, 0, ATRStop, Brushes.WhiteSmoke);

                ////////////////////////////////////
                // Set limit target to ATR
                ////////////////////////////////////
                ExitLongLimit (0, true, 1, target, "exit", "entry");
                Square targetSquare = Draw.Square (this, "initialTarget" + CurrentBar, true, 0, target, Brushes.White);

            }

            if (Position.MarketPosition == MarketPosition.Short) {

                ////////////////////////////////////
                // Determine ATR Target
                ////////////////////////////////////
                var target = Position.AveragePrice - (ATR (14) [0]);

                ATRStop = Low[1] + (ATRMultiplier * (ATR (ATRLength) [1]));
                ExitShortStopMarket (0, true, 1, ATRStop, "initialStop", "entry");
                Print (this.Name + " [" + instrument + "]");
                Print ("[" + instrument + "] Short " + Position.AveragePrice);
                Print (Time[0].ToString ());
                Print ("-----------------------------");
                Dot stopDot = Draw.Dot (this, "initialStop" + CurrentBar, true, 0, ATRStop, Brushes.WhiteSmoke);

                ////////////////////////////////////
                // Set limit target to ATR
                ////////////////////////////////////
                ExitShortLimit (0, true, 1, target, "exit", "entry");
                Square targetSquare = Draw.Square (this, "initialTarget" + CurrentBar, true, 0, target, Brushes.White);

            }

        }

        #region Properties

        ////////////////////////////////////
        // Contract Quantity
        ////////////////////////////////////
        [NinjaScriptProperty]
        [Range (1, int.MaxValue)]
        [Display (Name = "ContractQty", Order = 1, GroupName = "Parameters")]
        public int ContractQty { get; set; }

        #endregion

    }

}