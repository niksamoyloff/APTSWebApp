import React from "react";
import DatePicker from "react-datepicker";
import { Button, Form } from 'react-bootstrap';
import ru from 'date-fns/locale/ru';
import '../../node_modules/react-datepicker/dist/react-datepicker.css';
import './CustomDPicker.css';

export class DPicker extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            startDate: '',
            endDate: '',
            data: []
        };
    }    

    handleChangeStart = date => {
        this.setState({ startDate: date });
    };

    handleChangeEnd = date => {
        this.setState({ endDate: date });
    };

    async getDataByDate(sDate, eDate) {
        this.props.dataPicker(sDate, eDate);
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
        const StartCustomInput = ({ value, onClick }) => (
            <Form.Control
                id="startDT"
                onClick={onClick}
                value={value}
                className="dpickerInput"
                placeholder="Дата начала"
            />
        );
        const EndCustomInput = ({ value, onClick }) => (
            <Form.Control
                id="endDT"
                onClick={onClick}
                value={value}
                className="dpickerInput"
                placeholder="Дата окончания"
            />
        );

        const { startDate, endDate } = this.state;

        return (
            <>
                <DatePicker
                    todayButton="Сегодня"
                    locale={ru}
                    selected={startDate}
                    onChange={this.handleChangeStart.bind(this)}
                    customInput={<StartCustomInput />}
                    dateFormat="dd.MM.yyyy"
                />
                <div style={{ margin: '5px 0 5px 10px' }}>
                    <span>
                        <b>по:</b>
                    </span>
                </div>
                <DatePicker
                    todayButton="Сегодня"
                    locale={ru}
                    selected={endDate}
                    onChange={this.handleChangeEnd.bind(this)}
                    customInput={<EndCustomInput />}
                    dateFormat="dd.MM.yyyy"
                />
                <div style={{ marginLeft: '10px' }}>
                    {
                        startDate === '' && endDate === ''
                            ?
                            <Button variant="primary" disabled>Применить</Button>                        
                            :
                            <Button variant="primary" onClick={() => this.getDataByDate(startDate, endDate)}>Применить</Button>
                    }
                </div>
                
            </>
        );
    }
}