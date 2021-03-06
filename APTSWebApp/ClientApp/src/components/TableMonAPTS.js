import React, { Fragment, Component } from 'react';
import ReactTable from 'react-table';
import { ReactTableDefaults } from 'react-table'
import { ViewMode } from './ViewMode';
import matchSorter, { rankings } from 'match-sorter';
import DropdownFilter from './DropdownFilter';
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
            loading: true,
            viewTsRZA: true,
            viewTsOIC: false,
            listToExport: []
        };
        this.getData = this.getData.bind(this);
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

    startTimer(viewTsRZA = true, viewTsOIC = false) {
        this.setState({ isArchive: false, timer: true, loading: true });
        this.getData(viewTsRZA, viewTsOIC);
        this.timerID = setInterval(() => this.getData(this.state.viewTsRZA, this.state.viewTsOIC), 30000);
    }

    stopTimer() {
        this.setState({ isArchive: true, timer: false, data: [], listToExport: [] });
        clearInterval(this.timerID);
    }

    async getData(viewTsRZA, viewTsOIC, sDate, eDate) {
        if (this.state.isArchive)
            this.setState({ loading: true });

        let listToExp = [];
        let objSend = {};
        objSend["sDate"] = sDate === undefined ? '' : sDate;
        objSend["eDate"] = eDate === undefined ? '' : eDate;
        objSend["viewTsRZA"] = viewTsRZA;
        objSend["viewTsOIC"] = viewTsOIC;

        const response = await this.fetchData('Home/GetData', objSend);
        const list = await response.json();
        if (list.length)
            listToExp = this.populateListToExport(list);

        this.setState({ data: list, loading: false, listToExport: listToExp });
    }

    populateListToExport(list) {
        let arrayOfObjects = [];
        for (let i = 0; i < list.length; i++) {
            let objToExp = {};
            list[i].tsList.map(ts => {
                objToExp["dt"] = ts.dt;
                objToExp["tsName"] = ts.label;
                objToExp["tsOicId"] = ts.oicId;
                objToExp["value"] = ts.value;
                objToExp["enObj"] = list[i].objName;
                objToExp["device"] = list[i].devName;

                arrayOfObjects.push(objToExp);
                objToExp = {};
            });
        }
        return arrayOfObjects;
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

    async callbackGetDataArchiveMode(tempViewTsRZA, tempViewTsOIC, sDate, eDate) {
        //this.setState({ loading: true });

        //let objSend = {};
        //objSend["sDate"] = sDate;
        //objSend["eDate"] = eDate;
        //objSend["viewTsRZA"] = tempViewTsRZA;
        //objSend["viewTsOIC"] = tempViewTsOIC;

        //const response = await this.fetchData('Home/GetData', objSend);
        //const list = await response.json();
        //this.setState({ loading: false, data: list });
        this.getData(tempViewTsRZA, tempViewTsOIC, sDate, eDate);
    }

    callbackIsArchiveMode = (flag, tempViewTsRZA, tempViewTsOIC) => {
        if (flag)
            this.stopTimer();
        else {
            clearInterval(this.timerID);
            this.setState({ viewTsRZA: tempViewTsRZA, viewTsOIC: tempViewTsOIC, data: [] });
            this.startTimer(tempViewTsRZA, tempViewTsOIC);
        }
    }

    getTrProps = (state, rowInfo, instance) => {
        if (rowInfo) {
            return {
                style: {
                    color: rowInfo.original.isOicTs ? 'blue' : 'black'
                }
            }
        }
        return {};
    }

    render() {
        const { loading, data, listToExport } = this.state;
        
        const columns = [{
            Header: () => <b>Время приема ТС</b>,
            accessor: 'dt',
            minWidth: 100,
            width: 170,
            filterMethod: (filter, rows) =>
                matchSorter(rows, filter.value, { keys: [{ threshold: rankings.CONTAINS, key: 'dt' }] }),
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
            filterMethod: (filter, row) => {
                return row[filter.id] === filter.value;
            },
            Filter: ({ filter, onChange }) =>
                <DropdownFilter data={data} fieldName='objName' filter={filter} onChange={onChange} />,
        }, {
            Header: () => <b>Устройство РЗА</b>, 
            accessor: 'devName',
            minWidth: 200,
            width: 570,
            filterMethod: (filter, rows) =>
                matchSorter(rows, filter.value, { keys: [{ threshold: rankings.CONTAINS, key: 'devName' }] }),
            filterAll: true,
            style: { 'whiteSpace': 'normal' }
        }, {
            Header: () => <b>Наименование принятого ТС</b>,
            accessor: 'tsName',
            minWidth: 200,
            width: 600,
            filterMethod: (filter, rows) =>
                matchSorter(rows, filter.value, { keys: [{ threshold: rankings.CONTAINS, key: 'tsName' }] }),
            filterAll: true,
            style: { 'whiteSpace': 'normal' }
        }]

        return (
            <Fragment>
                <div className="viewMode">
                    <ViewMode
                        loading={loading}
                        isArchiveMode={this.callbackIsArchiveMode.bind(this)}
                        dataArchiveMode={this.callbackGetDataArchiveMode.bind(this)}
                        listToExport={listToExport}
                    />
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
                        SubComponent={
                            row => {
                                return (
                                    row.original.tsList.map(ts => (
                                        <div key={ts.key} style={{
                                            padding: "10px 20px",
                                            backgroundColor: "#c4def6",
                                            borderBottom: ts !== row.original.tsList[row.original.tsList.length - 1]
                                                ? "1px solid #f1f1f1"
                                                : ""
                                        }}>
                                            <em>
                                                <b>{ts.dt}</b><span>&nbsp;&nbsp;&nbsp;</span>{ts.label
                                                }<span>&nbsp;&nbsp;&nbsp;</span>
                                            </em>
                                            (ТС {ts.oicId})
                                            <span>&nbsp;&nbsp;&nbsp;</span>
                                            (Значение: <b>{ts.value}</b>)
                                            <br />
                                            <span style={{ whiteSpace: "pre-wrap" }}>{ts.comment}</span>
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
                        getTrProps={this.getTrProps}
                        defaultSorted={[
                            {
                                id: 'dt',
                                desc: false
                            }
                        ]}
                    />
                </div>
            </Fragment>
        );
    }
}
