import React, { Fragment, Component } from 'react'
import { Table, Button, Form } from 'react-bootstrap';
import ModalToDeleteApts from './ModalToDeleteApts';
import { ModalToAddApts } from './ModalToAddApts';
import NestedModalToAddApts from './NestedModalToAddApts';
import $ from 'jquery'
import ExportDevAPTS from './ExportDevAPTS';
import { EditApts } from './EditApts';
import LoaderAPTS from './LoaderAPTS';

export class TableAPTS extends Component {
    displayName = TableAPTS.name

    constructor(props) {
        super(props);
        this.state = {
            loadingAPTS: true,
            loadingTsFromOIC: true,
            aptsCanBeDelete: false,
            aptsCanBeAdd: false,
            showModalToDelete: false,
            showModalToAdd: false,
            showNestedModalToAdd: false,
            aptsList: [],
            tsListFromOIC: [],
            listToExport: [],
            isStatus: false
        };

        //this.getCountCheckedCheckboxesListAPTS = this.getCountCheckedCheckboxesListAPTS.bind(this);
        //this.editAllCheckboxesStatesListAPTS = this.editAllCheckboxesStatesListAPTS.bind(this);
        //this.handlerStateAPTStoBeAdd = this.handlerStateAPTStoBeAdd.bind(this);
        this.DeleteApts = this.DeleteApts.bind(this);
        this.AddApts = this.AddApts.bind(this);
        this.GetTsListFromOic = this.GetTsListFromOic.bind(this);
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
        this.setState({ showModalToDelete: true });
    }

    closeModalToDeleteHandler = () => {
        this.setState({ showModalToDelete: false });
    }

    openModalToAddHandler = () => {
        this.setState({ showModalToAdd: true, loadingTsFromOIC: true });
        this.GetTsListFromOic();
    }

    closeModalToAddHandler = () => {
        this.setState({ showNestedModalToAdd: true, aptsCanBeAdd: this.state.aptsCanBeAdd });
    }

    abortNestedModalToAddHandler = () => {
        this.setState({ showNestedModalToAdd: false, showModalToAdd: false, aptsCanBeAdd: false });
    }

    closeNestedModalToAddHandler = () => {
        this.setState({ showNestedModalToAdd: false, aptsCanBeAdd: this.state.aptsCanBeAdd })
    }

    handlerStateAPTStoBeAdd = (state) => {
        if (state)
            this.setState({ aptsCanBeAdd: true });
        else
            this.setState({ aptsCanBeAdd: false });
    }


    async GetAptsList() {
        const data = { id: this.props.deviceId };
        const response = await this.fetchData('Admin/GetAptsList', data);
        const list = await response.json();
        this.setState({ loadingAPTS: false, aptsList: list, aptsCanBeDelete: false });

        if (list.length)
            this.populateListToExport(list);
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
        let checkedCheckboxes = content.querySelectorAll('input[type="checkbox"]:not(.tsStatus):checked');

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
                if (statusAttr !== undefined && statusAttr !== false)
                    objToAdd["isStatus"] = true;
                else
                    objToAdd["isStatus"] = false;
                arrayOfObjects.push(objToAdd);
            }            
        }

        this.setState({ showModalToAdd: false, aptsCanBeDelete: false, loadingAPTS: true });
        await this.fetchData('Admin/AddApts', arrayOfObjects);
        this.GetAptsList();
    }

    async DeleteApts() {
        let arrayOfObjects = [], item;
        let key = "id";
        let content = document.getElementById("tBodyAPTSContent");
        let checkedCheckboxes = content.querySelectorAll('input[type="checkbox"]:not(.tsStatus):checked');

        for (let i = 0; i < checkedCheckboxes.length; i++) {
            let objToDelete = {};
            item = checkedCheckboxes[i];
            if (item.hasAttribute("id")) {
                objToDelete[key] = item.getAttribute("id");
            }
            arrayOfObjects.push(objToDelete);
        }

        this.setState({ showModalToDelete: false, aptsCanBeDelete: false, loadingAPTS: true });
        await this.fetchData('Admin/DeleteApts', arrayOfObjects);
        await this.GetAptsList();
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
        
    getCountCheckedCheckboxesListAPTS = () => {
        let content = document.getElementById("tBodyAPTSContent");
        let countCheckboxes = content.querySelectorAll('input[type="checkbox"]:not(.tsStatus)').length
        let countCheckedCheckboxes = content.querySelectorAll('input[type="checkbox"]:not(.tsStatus):checked').length;

        countCheckboxes !== countCheckedCheckboxes
            ? document.getElementById("aptsContent").querySelector('input[type="checkbox"]:not(.tsStatus)').checked = false
            : document.getElementById("aptsContent").querySelector('input[type="checkbox"]:not(.tsStatus)').checked = true;

        countCheckedCheckboxes > 0
            ? this.setState({ aptsCanBeDelete: true })
            : this.setState({ aptsCanBeDelete: false });
    }

    editAllCheckboxesStatesListAPTS = (e) => {
        let content = document.getElementById("tBodyAPTSContent");
        let checkboxes = content.querySelectorAll('input[type="checkbox"]:not(.tsStatus)');
        let countCheckedCheckboxes = 0;
        for (var i in checkboxes) {
            if (checkboxes[i].type === "checkbox") {
                if (e.target.checked) {
                    checkboxes[i].checked = true;
                    countCheckedCheckboxes += 1;
                }
                else
                    checkboxes[i].checked = false;
            }            
        }
        countCheckedCheckboxes > 0
            ? this.setState({ aptsCanBeDelete: true })
            : this.setState({ aptsCanBeDelete: false });
    }

    populateListToExport = (list) => {
        let arrayOfObjects = [];
        for (let i = 0; i < list.length; i++) {
            let objToExp = {};
            objToExp["powSys"] = this.props.powerSysName;
            objToExp["enObj"] = this.props.enObjName;
            objToExp["primary"] = this.props.primaryName;
            objToExp["device"] = this.props.deviceName;
            objToExp["tsName"] = list[i].label;
            objToExp["tsId"] = list[i].oicId;
            arrayOfObjects.push(objToExp);
        }

        if (arrayOfObjects.length)
            this.setState({ listToExport: arrayOfObjects });
    }

    renderButtons = (aptsCanBeDelete, aptsListLength, listToExport) => {
        return (
            <div>
                <div className="float-left">
                    {
                        aptsListLength
                            ? <ExportDevAPTS data={listToExport} />
                            : <Button variant="outline-success" size="sm" disabled>Excel</Button>
                    }                    
                </div>
                <div className="float-right">
                    <Button variant="outline-primary" size="sm" onClick={this.openModalToAddHandler}>Добавить</Button>
                    {
                        aptsCanBeDelete
                            ? <Button variant="outline-danger" size="sm" onClick={this.openModalToDeleteHandler}>Удалить</Button>
                            : <Button variant="outline-danger" size="sm" disabled>Удалить</Button>
                    }                
                </div>
            </div>
        );
    }

    renderModals = (showModalToDelete, showModalToAdd, showNestedModalToAdd) => {
        return (
            <Fragment>
                <ModalToDeleteApts show={showModalToDelete} onClose={this.closeModalToDeleteHandler} onDelete={this.DeleteApts} />
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
                />
                <NestedModalToAddApts show={showNestedModalToAdd} onAbort={this.abortNestedModalToAddHandler} onClose={this.closeNestedModalToAddHandler} />
            </Fragment>
        );
    };    

    renderList = aptsList => {
        const {
            aptsCanBeDelete,
            showModalToDelete,
            showModalToAdd,
            showNestedModalToAdd,
            listToExport
        } = this.state;
        return (
            <Fragment>
                {this.renderButtons(aptsCanBeDelete, aptsList.length, listToExport)}
                {this.renderModals(showModalToDelete, showModalToAdd, showNestedModalToAdd)}              
                <div id="aptsContent">
                    <Table responsive>
                        <thead>
                            <tr>
                                <td colSpan={3} style={{ borderTop: 0, padding: 5 }}>Количество АПТС:&nbsp;<b>{aptsList.length}</b></td>
                            </tr>
                            <tr>
                                <th>
                                    <Form.Check type="checkbox" onClick={this.editAllCheckboxesStatesListAPTS} />
                                </th>
                                <th>ID в ОИК</th>
                                <th className="text-center">Наименование</th>
                                <th className="text-center" style={{ width: '50px', padding: 0 }}>Сигнал состояния</th>
                                <th></th>
                            </tr>
                        </thead>
                        <tbody id="tBodyAPTSContent">
                            {
                                aptsList.length
                                    ? aptsList.map(ts => (
                                        <tr>
                                            <td>
                                                <Form.Check id={ts.key} type="checkbox" oicid={ts.oicId} onClick={this.getCountCheckedCheckboxesListAPTS} />
                                            </td>
                                            <td>{ts.oicId}</td>
                                            <td>{ts.label}</td>                                            
                                            <td>
                                                <Form.Check inline>
                                                    <Form.Check.Input type="checkbox" defaultChecked={ts.isStatus} className="tsStatus" disabled />
                                                </Form.Check>
                                            </td>                                                       
                                            <td className="text-center">
                                                <EditApts tsOicId={ts.oicId} isStatus={this.state.isStatus} editTsStatusHandler={this.editTsStatusHandler}/>
                                            </td>
                                        </tr>
                                    ))
                                    : (
                                        <tr>
                                            <td colSpan="3">
                                                <span><em>Привязанные АПТС отсутствуют.</em></span>
                                            </td>
                                        </tr>
                                    )
                            }
                        </tbody>
                    </Table>
                </div>
                {this.renderButtons(aptsCanBeDelete, aptsList.length, listToExport)}
            </Fragment>
        );
    };

    render() {
        const { loadingAPTS, aptsList } = this.state;
        return (
            <div>                
                {
                    loadingAPTS
                        ? <LoaderAPTS loading={loadingAPTS} />
                        : this.renderList(aptsList)
                }
            </div>
        );
    }
}


