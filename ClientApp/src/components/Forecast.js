import React, { Component } from 'react';
import moment from 'moment';

export class Forecast extends Component {
  static displayName = Forecast.name;

  constructor(props) {
    super(props);
    this.state = { forecast: [], loading: true };
  }

  componentDidMount() {
    this.populateForecastData();
  }
static renderRotate(degree) {
  return ({transform: 'rotate(' + degree + 'deg)'});
}
  static renderWeatherContents(forecastdata) {
    let style = {transform: 'rotate(180deg)'};
    return (
      <div>
        <table className="forecastTable">
          <tbody>
            {forecastdata.map(forecastitem =>
              <tr key={forecastitem.Datetime}>
                <td className="time-col">{moment(forecastitem.Datetime).format('HH:mm')}</td>
                <td><img width="32" height="32" src={"/img/" + forecastitem.WeatherSymbol3 + ".svg"} /></td>
                <td className="temperature-col">{Math.round(forecastitem.Temperature)}°</td>
                <td><div className="wind-container"><img style={Forecast.renderRotate(forecastitem.WindDirection-180)} src="/img/arrow.svg" width="32px" height="32px" /><span className="wind-text">{Math.round(forecastitem.WindSpeedMS)}</span></div></td>
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
      : Forecast.renderWeatherContents(this.state.forecastdata);

    return (
      <div id='forecast' className='box'>
        <h2>Forecast 24h</h2>
        <h3>Tapanila, Helsinki</h3>
         {contents}
      </div>
    );
  }

  async populateForecastData() {
    const response = await fetch('weatherfore');
    const data = await response.json();
    this.setState({ forecastdata: data, loading: false });
  }
}
