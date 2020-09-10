import React, { Component } from 'react';
import { Route } from 'react-router';
import { Layout } from './components/Layout';
import { Home } from './components/Home';

import './assets/custom.css'

export default class App extends Component {
  static displayName = App.name;

  render () {
    return (
      <div>
      <Layout>
        <Route exact path='/' component={Home} />
      </Layout>
      </div>
    );
  }
}
