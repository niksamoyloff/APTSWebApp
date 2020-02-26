import React from "react";
import ReactExport from "react-data-export";

const ExcelFile = ReactExport.ExcelFile;
const ExcelSheet = ReactExport.ExcelFile.ExcelSheet;
const ExcelColumn = ReactExport.ExcelFile.ExcelColumn;

const Export = (props) => {
    return (
        <ExcelFile filename={props.filename} element={<button className="btn btn-outline-success btn-sm" disabled={!props.data.length ? "disabled" : false} >Excel</button>}>
            <ExcelSheet data={props.data} name="Лист 1">
                <ExcelColumn label="Энергосистема" value="powSys" />
                <ExcelColumn label="Энергообъект" value="enObj" />
                <ExcelColumn label="Оборудование" value="primary" />
                <ExcelColumn label="Устройство РЗА" value="device" />
                <ExcelColumn label="Наименование ТС" value="tsName" />
                <ExcelColumn label="ID в ОИК" value="tsId" />
                <ExcelColumn label="Сигнал состояния" value="isStatus" />
                <ExcelColumn label="ТС ОИК" value="isOic" />
                <ExcelColumn label="Текущее значение" value="currentVal" />
                <ExcelColumn label="Примечание" value="comment" />
            </ExcelSheet>
        </ExcelFile>
    );
}

export default Export;