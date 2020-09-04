# NinjaTrader

### NinjaTrader 8 Inside Bar Strategy

##### Inside bars are formed when the current bar does not close above or below the previous bar.

This strategy identifies inside bars and send buy or sell orders if price breaks above or below the inside bar. It is fully automated, meaning it will send an order to open a position and immediately send a target limit and stop order to close the position. Additionally, it will move the stop order using Average True Range.

### How To Use

1. **Open Ninjatrader**

2. **Select New, Ninjascript Editor**

3. **Press +, New Strategy, Next, name it 'InsideBar', press Generate**

4. **Paste the code in the NinjaScript Editor window, Save and Compile**

5. **Select New, Chart, Instrument: /MES, Righ-click the chart, Strategies**

6. **Add 'InsideBar' strategy, make sure Account = SIM, press OK**

6. **Press the Enabled checkbox to activate the strategy**

### DO NOT USE REAL MONEY TO TRADE THIS STRATEGY.

#### Note: When looking at the strategy on a chart, past occurances will not trigger until the close of the signal bar. This is just how Ninjatrader displays strategies in the past. However, live signals will be taken on the break of an inside bar.