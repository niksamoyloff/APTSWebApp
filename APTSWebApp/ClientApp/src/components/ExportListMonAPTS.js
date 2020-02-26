import React from "react";
import ReactExport from "react-data-export";

const ExcelFile = ReactExport.ExcelFile;
const ExcelSheet = ReactExport.ExcelFile.ExcelSheet;
const ExcelColumn = ReactExport.ExcelFile.ExcelColumn;

const Export = (props) => {
    return (
        <ExcelFile
            filename={props.filename}
            element=
            {
                <button
                    className="btn btn-outline-success btn-sm"
                    disabled={props.disabled || !props.data.length ? "disabled" : false}
                >
                    Excel
                </button>}
        >
            <ExcelSheet data={props.data} name="Лист 1">
                <ExcelColumn label="Время приема ТС" value="dt" />
                <ExcelColumn label="Наименование принятого ТС" value="tsName" />
                <ExcelColumn label="ID ОИК" value="tsOicId" />
                <ExcelColumn label="Значение" value="value" />
                <ExcelColumn label="Объект" value="enObj" />
                <ExcelColumn label="Устройство РЗА" value="device" />
            </ExcelSheet>
        </ExcelFile>
    );
}

export default Export;