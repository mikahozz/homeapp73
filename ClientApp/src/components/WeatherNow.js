import React, { Component } from 'react';

export class WeatherNow extends Component {
  static displayName = WeatherNow.name;

  constructor(props) {
    super(props);
    this.state = { weatherdata: this.props.weatherdata, loading: true };
  }

  componentDidMount() {
    this.populateWeatherData();
  }

  async populateWeatherData() {
    const response = await fetch('weathernow');
    const data = await response.json();
    this.setState({ weatherdata: data, loading: false });
  }

  render() {
    let contents = this.state.loading
      ? <p><em>Loading...</em></p>
      : <div>
          <p className="temperatureNow">{this.state.weatherdata[this.state.weatherdata.length-1].temperature }°</p>
        </div>

    return (
      <div id="weatherNow" className="box">
        <h2>Weather now</h2>
         {contents}
      </div>
    );
  }

}
