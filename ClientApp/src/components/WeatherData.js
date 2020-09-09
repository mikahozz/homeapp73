import React, { Component } from 'react';
import { LineChart, Line, CartesianGrid, XAxis, YAxis, ResponsiveContainer } from 'recharts';
import moment from 'moment';
import { Tooltip } from 'reactstrap';

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
    this.setState({ weatherdata: data, loading: false });
  }

  static renderWeatherContents(weatherdata) {
    return (
      <div>

      <ResponsiveContainer width="95%" height={300}>
        <LineChart data={weatherdata}>
          <CartesianGrid stroke="rgb(35,48,74)" />
          <Line type="monotone" strokeWidth="5" dot={false} dataKey="temperature" stroke="#00b6ff" />
          <XAxis dataKey="datetime" tick={{ fill: "rgb(151, 151, 151)" }} tickFormatter={datetime => moment(datetime).format('HH:mm')} />
          <YAxis tick={{ fill: "rgb(151, 151, 151)" }} />
        </LineChart>
      </ResponsiveContainer>

        <table>
          <thead>
            <tr>
              <th>Datetime</th>
              <th>Temperature</th>
            </tr>
          </thead>
          <tbody>
            {weatherdata.map(weatherdataitem =>
              <tr key={weatherdataitem.datetime}>
                <td>{weatherdataitem.datetime}</td>
                <td>{weatherdataitem.temperature}</td>
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
