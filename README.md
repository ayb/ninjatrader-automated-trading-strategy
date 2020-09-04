# NinjaTrader Inside Bar

### Automated Trading Strategy

![Inside Bar](https://github.com/iniguezdj/ninjatrader_inside_strategy/blob/master/mes_5min.PNG)

### About

##### Inside bars are formed when the current bar does not close above or below the previous bar. This strategy identifies inside bars and sends buy or sell orders if price breaks above or below the inside bar. It is fully automated, meaning it will send an order to open a position and immediately send a target limit and stop order to close the position. Additionally, it will move the stop order using Average True Range. The script is written to use on a micro futures chart (/MES, /MNQ, /MYM, etc) but imports the mini futures data (/ES, /NQ, /YM) and uses data from the mini futures to signal a buy/sell order on the micro futures.

### How To Use

1. **Open Ninjatrader**

2. **From the Control Center, select New, Ninjascript Editor**

3. **Press +, select New Strategy, press Next, name it 'InsideBar', press Generate**

4. **Paste the code from 'inside_bar.cs' in the NinjaScript Editor window, Save and Compile**

5. **From the Control Center, select New, Chart, Instrument: /MES**

6. **Righ-click the chart, select Strategies**

7. **Add the 'InsideBar' strategy, make sure Account = SIM, press OK**

8. **From the Control Center, press the Enabled checkbox to activate the strategy**

### DO NOT USE REAL MONEY TO TRADE THIS STRATEGY. THERE IS ABSOLUTELY NO GUARANTEE THIS STRATEGY WORKS.

##### DISCLAIMER: When looking at the strategy on a chart, past occurances will not trigger until the close of the signal bar. This is just how Ninjatrader displays strategies in the past. However, live signals will be taken on the break of an inside bar. Additionally, some past signals may not show on the chart due to the signal bar 'failing' and reversing back across it's signal. This is not a holy grail. It is simply meant to be a template to help get you started with an automated strategy.