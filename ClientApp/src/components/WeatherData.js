import React, { Component } from 'react';
import { LineChart, Line, CartesianGrid, XAxis, YAxis, ResponsiveContainer } from 'recharts';
import moment from 'moment';
import { Tooltip } from 'reactstrap';
import _ from 'lodash'

export class WeatherData extends Component {
  static displayName = WeatherData.name;

  constructor(props) {
    super(props);
    this.state = { weatherdata: [], loading: true };
  }

  componentDidMount() {
    this.populateWeatherData();
  }

  async populateWeatherData() {
    const response = await fetch('weather');
    const data = await response.json();
    const sorted = _.sortBy(data, element => element.dt);
    const grouped = _.groupBy(sorted, element => element.dt.substring(0, 10))
    const mapped = _.map(grouped, (val, id, col) => ({
      dt: id,
      t2m: _.meanBy(val, 't2m')
    }))
    console.log(mapped);
    this.setState({ weatherdata: mapped, loading: false });
  }

  static renderWeatherContents(weatherdata) {
    return (
      <div>

      <ResponsiveContainer width="95%" height={300}>
        <LineChart data={weatherdata}>
          <CartesianGrid stroke="rgb(35,48,74)" />
          <Line type="monotone" strokeWidth="5" dot={false} dataKey="t2m" stroke="#00b6ff" />
          <XAxis dataKey="dt" tick={{ fill: "rgb(151, 151, 151)" }} tickFormatter={dt => moment(dt).format('DD')} />
          <YAxis tick={{ fill: "rgb(151, 151, 151)" }} />
        </LineChart>
      </ResponsiveContainer>

        <table className="weatherTable">
          <thead>
            <tr>
              <th>Datetime</th>
              <th>Temperature</th>
            </tr>
          </thead>
          <tbody>
            {weatherdata.map(weatherdataitem =>
              <tr key={weatherdataitem.dt}>
                <td>{weatherdataitem.dt}</td>
                <td>{_.round(weatherdataitem.t2m, 1)}˚</td>
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
        <h2>Weather stats</h2>
         {contents}
      </div>
    );
  }
}
