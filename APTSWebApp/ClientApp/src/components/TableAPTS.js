import React, { Fragment, Component } from 'react'
import { Button, Form } from 'react-bootstrap';
import ModalToDeleteApts from './ModalToDeleteApts';
import { ModalToAddApts } from './ModalToAddApts';
import NestedModalToActionAPTS from './NestedModalToActionAPTS';
import Export from './ExportListAPTS';
import LoaderAPTS from './LoaderAPTS';
import matchSorter from 'match-sorter'
import PropTypes from "prop-types";
import Table from "react-table";
import selectTableHOC from "react-table/lib/hoc/selectTable";
import $ from 'jquery'
import 'react-table/react-table.css';
import './CustomTableAPTS.css'
import ModalToEditApts from './ModalToEditApts';
import { IoMdSettings } from 'react-icons/io'

const SelectTable = selectTableHOC(Table);

export default class TableAPTS extends Component {
    static defaultProps = {
        keyField: "key"
    };

    static propTypes = {
        keyField: PropTypes.string
    };  

    constructor(props) {
        super(props);
        this.state = {
            loadingAPTS: true,
            loadingTsFromOIC: true,
            aptsCanBeAdd: false,
            showModalToDelete: false,
            showModalToAdd: false,
            showModalToEdit: false,
            showNestedModal: false,            
            selectAll: false,
            aptsList: [],
            tsListFromOIC: [],
            listToExport: [],
            selection: [],
            statusOfEdit: false,
            labelOfEdit: '',
            tsOicId: '',
            commentOfEdit: '',
            actionName: '',
            keyOfEdit: '',
            isOicStatus: false
        };

        this.GetTsListFromOic = this.GetTsListFromOic.bind(this);
        this.DeleteApts = this.DeleteApts.bind(this);
        this.AddApts = this.AddApts.bind(this);
        this.EditApts = this.EditApts.bind(this);
    }

    componentDidUpdate(prevProps, prevState) {
        if (this.props.deviceId !== prevProps.deviceId) {
            this.setState({ loadingAPTS: true });
            this.GetAptsList();
        }
    }

    componentDidMount() {
        this.GetAptsList();
    }

    openModalToDeleteHandler = () => {
        this.setState({ showModalToDelete: true, actionName: "Delete" });
    }

    closeModalToDeleteHandler = () => {
        this.setState({ showModalToDelete: false });
    }

    openModalToAddHandler = () => {
        this.setState({ showModalToAdd: true, loadingTsFromOIC: true, actionName: "Add" });
        this.GetTsListFromOic();
    }

    closeModalToAddHandler = () => {
        this.setState({ showNestedModal: true, aptsCanBeAdd: this.state.aptsCanBeAdd });
    }

    abortNestedModalHandler = () => {
        this.setState({ showNestedModal: false, showModalToAdd: false, showModalToEdit: false, aptsCanBeAdd: false });
    }

    closeNestedModalHandler = () => {
        this.setState({ showNestedModal: false, aptsCanBeAdd: this.state.aptsCanBeAdd })
    }

    openModalToEditHandler = () => {
        this.setState({
            showModalToEdit: true,
            actionName: "Edit"
        });
    }

    closeModalToEditHandler = () => {
        this.setState({ showNestedModal: true });
    }

    handlerStateAPTStoBeAdd = state => {
        if (state)
            this.setState({ aptsCanBeAdd: true });
        else
            this.setState({ aptsCanBeAdd: false });
    }
    
    async GetAptsList() {
        let listToExp = [];
        const data = { id: this.props.deviceId };
        const response = await this.fetchData('Admin/GetAptsList', data);
        const list = await response.json();

        if (list.length)
            listToExp = this.populateListToExport(list);
        this.setState({
             loadingAPTS: false, 
             aptsList: list, 
             listToExport: listToExp,
             aptsCanBeAdd: false
        });
    }

    async GetTsListFromOic() {
        const response = await fetch('Admin/GetTsListFromOic');
        const list = await response.json();
        this.setState({ loadingTsFromOIC: false, tsListFromOIC: list });
    }

    async AddApts() {
        let arrayOfObjects = [], item;
        let keys = ["oicid", "name", "device"];
        let content = document.getElementById("tBodyContentListFromOIC");
        let checkedCheckboxes = content.querySelectorAll('input[type="checkbox"]:not(.tsStatus):not(.isOic):checked');

        for (let i = 0; i < checkedCheckboxes.length; i++) {
            let objToAdd = {};
            item = checkedCheckboxes[i];
            //if (item.closest("tr").style.display !== "none" && !item.closest("tr").classList.contains("addedTS")) { // JS "closest" - not supported in IE
            if ($(item).closest("tr").css("display") !== "none" && !$(item).closest("tr").hasClass("addedTS")) {
                for (let j = 0; j < keys.length; j++) {
                    if (item.hasAttribute(keys[j])) {
                        objToAdd[keys[j]] = item.getAttribute(keys[j]);
                    }
                }
                let statusAttr = $(item).closest("tr").find(".tsStatus").prop("checked");
                let isOicAttr = $(item).closest("tr").find(".isOic").prop("checked");
                
                objToAdd["isStatus"] = statusAttr !== undefined && statusAttr !== false ? true : false;
                objToAdd["isOic"] = isOicAttr !== undefined && isOicAttr !== false ? true : false;
                arrayOfObjects.push(objToAdd);
            }
        }

        this.setState({
             showModalToAdd: false, 
             loadingAPTS: true
        });
        await this.fetchData('Admin/AddApts', arrayOfObjects);
        this.GetAptsList();
    }

    async DeleteApts() {
        let arrayOfObjects = [], item;
        let selection = [...this.state.selection];
        let key = "id";

        for (let i = 0; i < selection.length; i++) {
            let objToDelete = {};
            item = selection[i].split("-")[1]; 
            if (item !== undefined && item !== null && item.length !== 1) {
                objToDelete[key] = item;
            }
            arrayOfObjects.push(objToDelete);
        }

        this.setState({ showModalToDelete: false, loadingAPTS: true, selectAll: false });
        await this.fetchData('Admin/DeleteApts', arrayOfObjects);
        await this.GetAptsList();
    }

    async EditApts(tsOicId, isStatusTS, commentVal, isOicTS ) {
        const data = { id: tsOicId, status: isStatusTS, comment: commentVal, isOic: isOicTS };
        await this.fetchData('Admin/EditApts', data);

        this.setState({ showModalToEdit: false, loadingAPTS: true });
        this.GetAptsList();
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

    populateListToExport(list){
        let arrayOfObjects = [];
        for (let i = 0; i < list.length; i++) {
            let objToExp = {};
            objToExp["powSys"] = this.props.powerSysName;
            objToExp["enObj"] = this.props.enObjName;
            objToExp["primary"] = this.props.primaryName;
            objToExp["device"] = this.props.deviceName;
            objToExp["tsName"] = list[i].label;
            objToExp["tsId"] = list[i].oicId;
            objToExp["isStatus"] = list[i].isStatus ? "Да" : "Нет";
            objToExp["comment"] = list[i].comment;
            objToExp["isOic"] = list[i].isOic ? "Да" : "Нет";
            objToExp["currVal"] = list[i].currVal;
            arrayOfObjects.push(objToExp);
        }

        return arrayOfObjects;
    }

    /**
     * Toggle a single checkbox for select table
     */
    toggleSelection = (key, shift, row) => {
        // start off with the existing state
        let selection = [...this.state.selection];
        const keyIndex = selection.indexOf(key);

        // check to see if the key exists
        if (keyIndex >= 0) {
            // it does exist so we will remove it using destructing
            selection = [
                ...selection.slice(0, keyIndex),
                ...selection.slice(keyIndex + 1)
            ];
        } else {
            // it does not exist so add it
            selection.push(key);
        }
        this.setState({ selection });
    };

    /**
     * Toggle all checkboxes for select table
     */
    toggleAll = () => {
        const { keyField } = this.props;
        const selectAll = !this.state.selectAll;
        const selection = [];

        if (selectAll) {
            // we need to get at the internals of ReactTable
            const wrappedInstance = this.checkboxTable.getWrappedInstance();
            // the 'sortedData' property contains the currently accessible records based on the filter and sort
            const currentRecords = wrappedInstance.getResolvedState().sortedData;
            // we just push all the IDs onto the selection array
            currentRecords.forEach(item => {
                selection.push(`select-${item._original[keyField]}`);
            });
        }
        this.setState({ selectAll, selection });
    };

    /**
     * Whether or not a row is selected for select table
     */
    isSelected = key => {
        return this.state.selection.includes(`select-${key}`);
    };
    
    rowFn = (state, rowInfo, column, instance) => {
        const { selection } = this.state;

        return {
            onClick: (e, handleOriginal) => {                
                const rowData = rowInfo.original;
                this.setState({
                    keyOfEdit: rowData.key,
                    tsOicId: rowData.oicId,
                    statusOfEdit: rowData.isStatus,
                    labelOfEdit: rowData.label,
                    commentOfEdit: rowData.comment,
                    isOicStatus: rowData.isOic
                }, () => {
                    if (handleOriginal) {
                        handleOriginal()
                    }
                });
            },
            style: {
                background:
                    rowInfo &&
                    selection.includes(`select-${rowInfo.original.key}`) &&
                    "lightgreen"
            }
        };        
    };

    renderButtons = (selection, listToExport, loadingAPTS) => {
        return (
            <div className="aptsListBtns">
                <div className="float-left">
                    <Export data={listToExport} filename="ExportedDeviceListAPTS" disabled={loadingAPTS} />
                </div>
                <div className="float-right">
                    <Button variant="outline-primary" size="sm" onClick={this.openModalToAddHandler}>Добавить</Button>
                    {
                        selection.length
                            ? <Button variant="outline-danger" size="sm" onClick={this.openModalToDeleteHandler}>Удалить</Button>
                            : <Button variant="outline-danger" size="sm" disabled>Удалить</Button>
                    }
                </div>
            </div>
        );
    }

    renderModals = (
        showModalToAdd,
        showModalToEdit,
        showModalToDelete,
        showNestedModal
    ) => {

        const tsOicIdList = this.state.aptsList.map((obj) => obj.oicId);

        return (
            <Fragment>
                <ModalToAddApts
                    show={showModalToAdd}
                    onClose={this.closeModalToAddHandler}
                    onAdd={this.AddApts}
                    data={this.state.tsListFromOIC}
                    loading={this.state.loadingTsFromOIC}
                    deviceId={this.props.deviceId}
                    aptsCanBeAdd={this.state.aptsCanBeAdd}
                    aptsCanBeAddHandler={this.handlerStateAPTStoBeAdd}
                    deviceName={this.props.deviceName}
                    enObjName={this.props.enObjName}
                    list={tsOicIdList}
                />
                <ModalToEditApts
                    key={'editTs-' + this.state.keyOfEdit}
                    show={showModalToEdit}
                    tsId={this.state.tsOicId}
                    status={this.state.statusOfEdit}
                    label={this.state.labelOfEdit}
                    comment={this.state.commentOfEdit}
                    isOic={this.state.isOicStatus}
                    onClose={this.closeModalToEditHandler}
                    onEdit={this.EditApts}
                />
                <ModalToDeleteApts show={showModalToDelete} onClose={this.closeModalToDeleteHandler} onDelete={this.DeleteApts} />
                <NestedModalToActionAPTS show={showNestedModal} action={this.state.actionName} onAbort={this.abortNestedModalHandler} onClose={this.closeNestedModalHandler} />
            </Fragment>
        );
    };    

    renderList = aptsList => {
        const {
            showModalToAdd,
            showModalToEdit,
            showModalToDelete,
            showNestedModal,
            listToExport,
            selectAll,
            selection,
            loadingAPTS
        } = this.state;

        const columns = [
            {
                Header: () => <b>ID в ОИК</b>,
                accessor: 'oicId',
                minWidth: 60,
                //filterMethod: (filter, rows) =>
                //    matchSorter(rows, filter.value, { keys: ["oicId"] }),
                //filterAll: true,
                headerClassName: 'wordwrap'
            },
            {
                Header: () => <b>Наименование</b>,
                accessor: 'label',
                minWidth: 600,
                //filterMethod: (filter, rows) =>
                //    matchSorter(rows, filter.value, { keys: ["label"] }),
                //filterAll: true,
                style: { 'whiteSpace': 'normal' },
                headerClassName: 'wordwrap'
            },
            {
                Header: () => <b>Сигнал состояния</b>,
                accessor: 'isStatus',
                minWidth: 100,
                Cell: (row) => (
                    <Form.Check defaultChecked={row.value} className="tsStatus middleItems" disabled />
                ),
                headerClassName: 'wordwrap'
            },
            {
                Header: () => <b>ТС ОИК</b>,
                accessor: 'isOic',
                minWidth: 50,
                Cell: (row) => (
                    <Form.Check defaultChecked={row.value} className="tsStatus middleItems" disabled />
                ),
                headerClassName: 'wordwrap'
            },
            {
                Header: '',
                minWidth: 30,
                Cell: (row) => (
                    <div className="middleItems">
                        <Button variant="link" size="lg" className="tsStatusBtnEdit" onClick={this.openModalToEditHandler}>
                            <IoMdSettings />
                        </Button>
                    </div>
                )
            }
        ];

        return (
            <Fragment>
                {this.renderButtons(selection, listToExport, loadingAPTS)}
                {this.renderModals(showModalToAdd, showModalToEdit, showModalToDelete, showNestedModal)}
                <div id="aptsContent">
                    <SelectTable
                        data={aptsList}
                        columns={columns}
                        keyField="key"
                        ref={r => (this.checkboxTable = r)}
                        toggleSelection={this.toggleSelection}
                        selectAll={selectAll}
                        selectType="checkbox"
                        toggleAll={this.toggleAll}
                        isSelected={this.isSelected}
                        getTdProps={this.rowFn}
                        className="-highlight aptsListTable"
                        SubComponent={row => {
                                return (
                                    <div key={row.original.key}>
                                        <em>
                                            {
                                                !row.original.comment || 0 === row.original.comment.length
                                                    ?
                                                    <span>Примечание отсутствует.</span>
                                                    :
                                                    <span style={{ whiteSpace: "pre-wrap" }}>{row.original.comment}</span>
                                            }
                                        </em>
                                    </div>
                                );
                            }
                        }
                        loadingText="Получение данных..."
                        NoDataComponent={() => null}
                        showPagination={false}
                        defaultPageSize={aptsList.length !== 0 ? aptsList.length : 1}
                        sortable={false}
                        freezeWhenExpanded={true}
                    />
                </div>
                {this.renderButtons(selection, listToExport, loadingAPTS)}
            </Fragment>
        );
    };

    render() {
        const { loadingAPTS, aptsList } = this.state;
        return (
            <div>
                {
                    loadingAPTS
                        ? <LoaderAPTS />
                        : this.renderList(aptsList)
                }
            </div>
        );
    }
}
