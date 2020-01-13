import React, { Component } from 'react';
import ReactTable from 'react-table';
import { ReactTableDefaults } from 'react-table'
import { ViewMode } from './ViewMode'
import matchSorter from 'match-sorter'
import 'react-table/react-table.css';
import './CustomTableMon.css';

//Object.assign(ReactTableDefaults, {
//    previousText: 'Предыдущая',
//    nextText: 'Следующая',
//    loadingText: 'Загрузка...',
//    noDataText: 'Уведомлений нет.',
//    pageText: 'Страница',
//    ofText: 'из',
//    rowsText: 'строк',
//})

export class TableMonAPTS extends Component {
    displayName = TableMonAPTS.name

    constructor() {
        super();

        this.state = {
            timer: false,
            data: [],
            isArchive: false,
            loading: true
        };
        this.getList = this.getList.bind(this);
    }

    componentDidMount() {
        this.startTimer();   
    }

    shouldComponentUpdate(nextProps, nextState) {
        let prevData = JSON.stringify(this.state.data);
        let newData = JSON.stringify(nextState.data);
        let prevLoading = this.state.loading;
        let newLoading = nextState.loading;

        if (prevData !== newData || prevLoading !== newLoading) {            
            return true;
        }
        return false;
    }

    componentWillUnmount() {
        clearInterval(this.interval);
    }

    startTimer() {
        this.setState({ isArchive: false, timer: true, loading: true, data: [] });
        this.getList();
        this.timerID = setInterval(() => this.getList(), 30000);
    }

    stopTimer() {
        this.setState({ isArchive: true, timer: false, data: [] });
        clearInterval(this.timerID);
    }

    async getList() {
        //this.setState({ data: [] });
        const response = await this.fetchData('Home/GetData', []);
        const list = await response.json();
        this.setState({ data: list, loading: false });  
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

    async callbackGetDataArchiveMode(sDate, eDate) {
        this.setState({ loading: true });
        const response = await this.fetchData('Home/GetData', [sDate, eDate]);
        const list = await response.json();
        this.setState({ loading: false, data: list });
    }

    callbackIsArchiveMode = (flag) => {
        if (flag)
            this.stopTimer();
        else {
            this.startTimer();
        }
    }

    render() {
        const { loading, data } = this.state;
        
        const columns = [{
            Header: () => <b>Время приема ТС</b>,
            accessor: 'dt',
            minWidth: 100,
            width: 170,
            filterMethod: (filter, rows) =>
                matchSorter(rows, filter.value, { keys: ["dt"] }),
            filterAll: true,
            sortMethod: (a, b) => {
                let a1 = new Date(a).getTime();
                let b1 = new Date(b).getTime();
                if (a1 < b1)
                    return 1;
                else if (a1 > b1)
                    return -1;
                else
                    return 0;
            }
        }, {
            Header: () => <b>Объект</b>,
            accessor: 'objName',
            minWidth: 200,
            width: 260,
            filterMethod: (filter, rows) =>
                matchSorter(rows, filter.value, { keys: ["objName"] }),
            filterAll: true
        }, {
            Header: () => <b>Устройство РЗА</b>, 
            accessor: 'devName',
            minWidth: 200,
            width: 570,
            filterMethod: (filter, rows) =>
                matchSorter(rows, filter.value, { keys: ["devName"] }),
            filterAll: true,
            style: { 'whiteSpace': 'normal' }
        }, {
            Header: () => <b>Наименование принятого ТС</b>,
            accessor: 'tsName',
            minWidth: 200,
            width: 600,
            filterMethod: (filter, rows) =>
                matchSorter(rows, filter.value, { keys: ["tsName"] }),
            filterAll: true,
            style: { 'whiteSpace': 'normal' }
        }]

        return (
            <>
                <div className="viewMode">
                    <ViewMode isArchiveMode={this.callbackIsArchiveMode.bind(this)} dataArchiveMode={this.callbackGetDataArchiveMode.bind(this)} />
                </div>
                <div className="d-flex">
                    <ReactTable
                        loading={loading}
                        data={data}
                        //resolveData={data => data.map(row => row)}
                        columns={columns}
                        className="-highlight tableMonLyt"
                        filterable
                        defaultFilterMethod={(filter, row) =>
                            String(row[filter.id]) === filter.value}
                        SubComponent={row => {
                            return (
                                row.original.tsList.map(ts => (
                                    <div key={ts.key} style={{
                                        padding: "10px 20px",
                                        backgroundColor: "#c4def6",
                                        borderBottom: ts !== row.original.tsList[row.original.tsList.length - 1] ? "1px solid #f1f1f1" : ""
                                    }}>
                                        <em>
                                            <b>{ts.dt}</b><span>&nbsp;&nbsp;&nbsp;</span>{ts.label}<span>&nbsp;&nbsp;&nbsp;</span>
                                        </em>
                                        (ТС {ts.oicId})
                                        <span>&nbsp;&nbsp;&nbsp;</span>
                                        (Значение: <b>{ts.value}</b>)
                                        <br/>
                                        <span style={{ whiteSpace: "pre" }}>{ts.comment}</span>
                                    </div>
                                ))
                            );
                        }
                        }
                        previousText="Предыдущая"
                        nextText="Следующая"
                        loadingText="Получение данных..."
                        noDataText={!loading && data.length === 0 ? "Нет данных." : ""}
                        pageText="Страница"
                        ofText="из"
                        rowsText="строк"
                        pageSizeOptions={[10, 15, 20, 25, 50, 100]}
                        defaultPageSize={25}
                    />
                </div>
            </>
        )
    }
}
