import React, { Component } from 'react';
import ReactTable from 'react-table';
import { ReactTableDefaults } from 'react-table'
import 'react-table/react-table.css';
import './CustomTableMon.css';
import matchSorter from 'match-sorter'

//Object.assign(ReactTableDefaults, {
//    previousText: 'Предыдущая',
//    nextText: 'Следующая',
//    loadingText: 'Загрузка...',
//    pageText: 'Страница',
//    ofText: 'из',
//    rowsText: 'строк',
//})

export class History extends Component {
    displayName = History.name

    constructor() {
        super();

        this.state = { data: [], loading: true };
        this.getActions = this.getActions.bind(this);
    }

    componentDidMount() {
        this.getActions();
        this.interval = setInterval(() => this.getActions(), 10000);
    }

    shouldComponentUpdate(nextProps, nextState) {
        let prevData = JSON.stringify(this.state.data);
        let newData = JSON.stringify(nextState.data);

        if (prevData !== newData)
            return true;    
        return false;
    }

    componentWillUnmount() {
        clearInterval(this.interval);
    }

    async getActions() {
        //this.setState({ loading: true, data: [] });
        const response = await fetch('Admin/GetActions');
        const list = await response.json();
        this.setState({ loading: false, data: list });
    }

    render() {
        const { loading, data } = this.state;

        const columns = [{
            Header: () => <b>Время</b>,
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
            Header: () => <b>Пользователь</b>,
            accessor: 'userName',
            minWidth: 100,
            width: 150,
            filterMethod: (filter, rows) =>
                matchSorter(rows, filter.value, { keys: ["userName"] }),
            filterAll: true
        }, {
            Header: () => <b>Действие</b>,
            accessor: 'actionName',
            minWidth: 100,
            width: 100,
            filterMethod: (filter, rows) =>
                matchSorter(rows, filter.value, { keys: ["actionName"] }),
            filterAll: true,
            className: "cellTextCenter"
        }, {
            Header: () => <b>Номер ТС</b>,
            accessor: 'tsOicId',
            minWidth: 50,
            width: 100,
            filterMethod: (filter, rows) =>
                matchSorter(rows, filter.value, { keys: ["tsOicId"] }),
            filterAll: true,
            className: "cellTextCenter"
        }, {
            Header: () => <b>Наименование ТС</b>,
            accessor: 'tsName',
            minWidth: 200,
            width: 600,
            filterMethod: (filter, rows) =>
                matchSorter(rows, filter.value, { keys: ["tsName"] }),
            filterAll: true,
            style: { 'whiteSpace': 'normal' }
        }, {
            Header: () => <b>Наименование устройства РЗА</b>,
            accessor: 'devName',
            minWidth: 200,
            width: 500,
            filterMethod: (filter, rows) =>
                matchSorter(rows, filter.value, { keys: ["devName"] }),
            filterAll: true,
            style: { 'whiteSpace': 'normal' }
        }, {
            Header: () => <b>Объект</b>,
            accessor: 'objName',
            minWidth: 150,
            width: 260,
            filterMethod: (filter, rows) =>
                matchSorter(rows, filter.value, { keys: ["objName"] }),
            filterAll: true,
            style: { 'whiteSpace': 'unset' }
        }]

        return (
            <div className="tableHistoryLyt d-flex">
                <ReactTable
                    loading={loading}
                    data={data}
                    //resolveData={data => data.map(row => row)}
                    columns={columns}
                    className="-highlight tableMonLyt"
                    filterable
                    defaultFilterMethod={(filter, row) =>
                        String(row[filter.id]) === filter.value}
                    noDataText={!loading && data.length === 0 ? "Действий пока нет." : ""}
                    previousText="Предыдущая"
                    nextText="Следующая"
                    loadingText="Получение данных..."
                    pageText="Страница"
                    ofText="из"
                    rowsText="строк"
                    pageSizeOptions={[10, 15, 20, 25, 50, 100]}
                    defaultPageSize={25}
                />
            </div>
        )
    }
}
