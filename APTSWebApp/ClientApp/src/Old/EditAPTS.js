import React, { Fragment, Component } from 'react'
import { Button, Form } from 'react-bootstrap';
import { IoIosSave, IoMdSettings } from 'react-icons/io'
import './CustomTableAPTS.css';

export class EditAPTS extends Component {
    constructor(props) {
        super(props);

        this.state = {
            editTsStatus: false,
            isStatusTs: this.props.isStatus
        };
    }

    editTsStatusHandler = () => {
        this.updateTsStatus();
        this.setState({ editTsStatus: !this.state.editTsStatus });
    }

    changeTsStatusHandler = () => {
        this.setState({ isStatusTs: !this.state.isStatusTs });
    }

    async updateTsStatus() {    
        const data = { id: this.props.tsOicId, status: this.state.isStatusTs };
        await this.fetchData('Admin/UpdateStatusAPTS', data);
    }

    fetchData(url = '', data = {}) {
        return fetch(url, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(data)
        });
    }

    render() {
        const { isStatusTs } = this.state;
        return (
            <Fragment>
                <Button variant="link" size="lg" className="tsStatusBtnEdit"  onClick={this.editTsStatusHandler}>
                    <IoMdSettings />
                </Button>
            </Fragment>
        );
    }
}