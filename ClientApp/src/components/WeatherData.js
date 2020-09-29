import React, { Component } from 'react';
import { LabelList, Tooltip, Legend, ComposedChart, Bar, Line, CartesianGrid, XAxis, YAxis, ResponsiveContainer } from 'recharts';
import moment from 'moment';
import _ from 'lodash'

export class WeatherData extends Component {
  static displayName = WeatherData.name;
  static intervalId;

  constructor(props) {
    super(props);
    this.state = { weatherdata: [], loading: true };
  }

  static renderCustomizedLabel = (props) =>{
    const { x, y, value } = props;
    return (
      <g>
       <text fontSize="10" x={x} y={y-5} fill="rgba(0, 182, 255, 0.8)">{value===0 || _.round(value, 1)}</text>
      </g>
    )
  };

  static legendMap = {t2m: 'temperature (avg)', r_1h: 'precipitation (mm)'};
  static renderCustomizedLegend = (value, entry) =>{
    const { color } = entry;
    console.log(color);
    return <span style={{ color }}>{WeatherData.legendMap[value]}</span>;  
  };

  componentDidMount() {
    this.populateWeatherData();
    // Refresh once in 24h
    this.intervalId = setInterval(this.populateWeatherData.bind(this), 24*60*60*1000);
  } 
  componentWillUnmount() {
    // Stop refreshing
    clearInterval(this.intervalId);
  }

  async populateWeatherData() {
    const response = await fetch('weather');
    const data = await response.json();
    const sorted = _.sortBy(data, element => element.dt);
    const grouped = _.groupBy(sorted, element => element.dt.substring(0, 10))
    const mapped = _.map(grouped, (val, id, col) => ({
      dt: id,
      t2m: _.meanBy(val, 't2m'),
      r_1h: _.sumBy(val, 'r_1h')
    }))
    console.log(mapped);
    this.setState({ weatherdata: mapped, loading: false });
  }


  static renderWeatherContents(weatherdata) {
    return (
      <div>

      <ResponsiveContainer width="95%" height={300}>
        <ComposedChart data={weatherdata}>
          <CartesianGrid stroke="rgb(35,48,74)" />
          <Tooltip />
          <Legend formatter={WeatherData.renderCustomizedLegend}/>
          <Line type="monotone" strokeWidth="5" dot={false} dataKey="t2m" stroke="#00b6ff" />
          <Bar dataKey="r_1h" barSize={20} fill="rgba(0, 182, 255, 0.5)">
            <LabelList dataKey="r_1h" position="insideTop" content={WeatherData.renderCustomizedLabel} />
          </Bar>
          <XAxis dataKey="dt" tick={{ fill: "rgb(151, 151, 151)" }} tickFormatter={dt => moment(dt).format('DD')} />
          <YAxis tick={{ fill: "rgb(151, 151, 151)" }} />
        </ComposedChart>
      </ResponsiveContainer>

        <table className="weatherTable">
          <thead>
            <tr>
              <th>Datetime</th>
              <th>Temperature</th>
              <th>Precipitation (mm)</th>
            </tr>
          </thead>
          <tbody>
            {weatherdata.map(weatherdataitem =>
              <tr key={weatherdataitem.dt}>
                <td>{weatherdataitem.dt}</td>
                <td>{_.round(weatherdataitem.t2m, 1)}˚</td>
                <td>{_.round(weatherdataitem.r_1h, 1)}</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    );
  }

  render() {
    let contents = this.state.loading
    ? <p><em>Loading...</em></p>
    : WeatherData.renderWeatherContents(this.state.weatherdata);

    return (
      <div id="weatherdata" className="box">
        <h2>Daily temperature (avg) and precipitation amount this month</h2>
         {contents}
      </div>
    );
  }

}
