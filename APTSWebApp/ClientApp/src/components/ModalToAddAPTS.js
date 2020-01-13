import React, { Component } from 'react';
import { Table, Button, Modal, InputGroup, FormControl } from 'react-bootstrap';
import './CustomModal.css';
import $ from 'jquery'
import LoaderAPTS from './LoaderAPTS';

export class ModalToAddAPTS extends Component {

    constructor(props) {
        super(props);

        this.search = this.search.bind(this);
        this.checkAPTSisAlreadyAdded = this.checkAPTSisAlreadyAdded.bind(this);
        this.editAllCheckboxesStatesListTSFromOIC = this.editAllCheckboxesStatesListTSFromOIC.bind(this);
        this.getCountCheckedCheckboxesListTSFromOIC = this.getCountCheckedCheckboxesListTSFromOIC.bind(this);
    }

    componentDidUpdate() {
        if (!this.props.loading)
            this.checkAPTSisAlreadyAdded();
    }

    search() {
        let input = document.getElementById("searchTS");
        let filter = input.value.toUpperCase();
        let table = document.getElementById("tsListFromOicTable");
        let tr = table.getElementsByTagName("tr");
        let countTS = document.getElementById("countTS");
        let count = 0;
        for (let i = 0; i < tr.length; i++) {
            let tdId = tr[i].getElementsByTagName("td")[1];
            let tdName = tr[i].getElementsByTagName("td")[2];
            let tdEnObj = tr[i].getElementsByTagName("td")[3];

            if (tdId || tdName || tdEnObj) {
                let txtValueId = tdId.textContent || tdId.innerText;
                let txtValueName = tdName.textContent || tdName.innerText;                
                let txtValueEnObj = tdEnObj.textContent || tdEnObj.innerText;
                if (
                    txtValueId.toUpperCase().indexOf(filter) > -1 ||
                    txtValueName.toUpperCase().indexOf(filter) > -1 ||
                    txtValueEnObj.toUpperCase().indexOf(filter) > -1
                )
                {
                    tr[i].style.display = "";
                    count++;
                } else {
                    tr[i].style.display = "none";
                }                
            }
        }
        countTS.innerText = count;
    }

    getCountCheckedCheckboxesListTSFromOIC() {
        let content = document.getElementById("tBodyContentListFromOIC");
        let countCheckboxes = content.querySelectorAll('input[type="checkbox"]:not([disabled]):not(.tsStatus)').length
        let countCheckedCheckboxes = content.querySelectorAll('input[type="checkbox"]:not([disabled]):not(.tsStatus):checked').length;

        countCheckboxes !== countCheckedCheckboxes
            ? document.getElementById("tsListFromOicContent").querySelector('input[type="checkbox"]').checked = false
            : document.getElementById("tsListFromOicContent").querySelector('input[type="checkbox"]').checked = true;

        countCheckedCheckboxes > 0
            ? this.props.aptsCanBeAddHandler(true)
            : this.props.aptsCanBeAddHandler(false);
    }

    editAllCheckboxesStatesListTSFromOIC(e) {
        let content = document.getElementById("tBodyContentListFromOIC");
        let checkboxes = content.querySelectorAll('input[type="checkbox"]:not([disabled]):not(.tsStatus)');
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
            ? this.props.aptsCanBeAddHandler(true)
            : this.props.aptsCanBeAddHandler(false);
    }

    checkAPTSisAlreadyAdded() {
        let contentToAdd = document.getElementById("tBodyContentListFromOIC");
        if (contentToAdd) {
            let tsAdded = this.props.list;

            for (let i in tsAdded) {
                let itemToAdd = contentToAdd.querySelector('input[oicid="' + tsAdded[i] + '"]');
                if (itemToAdd !== null || itemToAdd !== undefined) {
                    itemToAdd.disabled = true;
                    itemToAdd.checked = true;
                    $(itemToAdd).closest("tr").find(".tsStatus").prop("disabled", true);
                    $(itemToAdd).closest("tr").addClass("addedTS"); // supported in IE and Chrome
                    //itemToAdd.closest("tr").classList.add("addedTS"); // supported in Chrome, not - in IE 11
                }                                        
            }
        }        
    }

    renderList = tsList => {
        const { deviceId } = this.props;
        return (
            <div id="tsListFromOicContent">
                <InputGroup>
                    <FormControl id="searchTS" onChange={e => this.search(e.target.value)} placeholder="Поиск..." />
                    <InputGroup.Append>
                        <InputGroup.Text>Количество АПТС:&nbsp;<b><span id="countTS">{tsList.length}</span></b></InputGroup.Text>
                    </InputGroup.Append>
                </InputGroup>
                <Table id="tsListFromOicTable">
                    <thead>
                        <tr>
                            <th>
                                <input type="checkbox" onClick={this.editAllCheckboxesStatesListTSFromOIC}/>
                            </th>
                            <th style={{ width: "100px" }}>ID в ОИК</th>
                            <th style={{ width: "700px" }}>Наименование</th>
                            <th style={{ width: "300px" }}>Энергообъект</th>
                            <th style={{ width: "170px" }}>Сигнал состояния</th>
                        </tr>
                    </thead>
                    <tbody id="tBodyContentListFromOIC">
                        {
                            tsList.length
                                ? tsList.map(ts => (
                                    <tr>
                                        <td>
                                            <input type="checkbox" oicid={ts.oicId} name={ts.label} device={deviceId} onClick={this.getCountCheckedCheckboxesListTSFromOIC} />
                                        </td>
                                        <td>{ts.oicId}</td>
                                        <td>{ts.label}</td>
                                        <td>{ts.enObj}</td>
                                        <td className="text-center">
                                            <input type="checkbox" className="tsStatus" defaultChecked={ts.isStatus} />
                                        </td>
                                    </tr>
                                ))
                                : (
                                    <tr>
                                        <td colSpan="3">
                                            <span><em>Нет телесигналов.</em></span>
                                        </td>
                                    </tr>
                                )
                        }
                    </tbody>
                </Table>
            </div>
        );
    };

    render() {
        const {
            loading,
            show,
            onClose,
            onAdd,
            data,
            aptsCanBeAdd,
            deviceName,
            enObjName
        } = this.props;
        return (
            <Modal dialogClassName="modalToAdd" show={show} onHide={onClose} centered>
                <Modal.Header closeButton={onClose}>
                    <Modal.Title>
                        {enObjName} / {deviceName}
                    </Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    {
                        loading
                            ? <LoaderAPTS loading={loading} />
                            : this.renderList(data)
                    }
                </Modal.Body>
                <Modal.Footer>
                    <div className="float-right">
                        {
                            aptsCanBeAdd
                                ? <Button variant="success" onClick={onAdd}>Добавить</Button>
                                : <Button variant="success" disabled>Добавить</Button>
                        }
                        <Button variant="secondary" onClick={onClose}>Отмена</Button>
                    </div>
                </Modal.Footer>
            </Modal>
        );
    }    
}
