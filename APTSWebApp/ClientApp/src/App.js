import React, { Component } from 'react';
import { Route, Redirect, Switch } from 'react-router';
import { Layout } from './components/Layout';
import { TableMonAPTS } from './components/TableMonAPTS';
import { Splitter } from './components/Splitter';
import { History } from './components/History';
import AccessDenied from './components/AccessDenied';
import NotFound from './components/NotFound';
import PropTypes from 'prop-types';
import './custom.css';

export default class App extends Component {
    static displayName = App.name;

    constructor() {
        super();

        this.checkUserAuthorization = this.checkUserAuthorization.bind(this);
        this.state = {
            authorized: true
        };
    }

    static propTypes = {
        location: PropTypes.object.isRequired
    }

    componentDidMount() {
        this.checkUserAuthorization();
    }

    async checkUserAuthorization() {
        const response = await fetch('Home/CheckUserAuthorization');
        const flag = await response.text();
        if (flag === "0")
            this.setState({ authorized: false });
    }

    render() {

        const { authorized } = this.state;

        return (
            <Layout>
                <Switch>
                    <Route exact path='/' component={TableMonAPTS} />
                    <Route path='/tree' render={() => (
                        authorized
                            ?
                            <Splitter />
                            :
                            <Redirect to='/AccessDenied' />
                    )} />
                    <Route path='/history' render={() => (
                        authorized
                            ?
                            <History />
                            :
                            <Redirect to='/AccessDenied' />
                    )} />
                    <Route path='/AccessDenied' component={AccessDenied} />
                    <Route component={NotFound} />
                </Switch>
            </Layout>
        );
    }
}

