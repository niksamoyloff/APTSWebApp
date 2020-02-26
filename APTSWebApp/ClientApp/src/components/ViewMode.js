import React from 'react';
import { Button, Dropdown, DropdownButton, Form } from 'react-bootstrap';
import { DPicker } from './DPicker';
import Export from './ExportListMonAPTS';

const modes = ["Оперативный", "Архив"];

export class ViewMode extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            selectedMode: modes[0],
            isArchive: false,
            viewTsRZA: true,
            viewTsOIC: false
        }
    }

    handleSelect(eventKey, event) {
        let flag = false;
        if (modes[eventKey] !== this.state.selectedMode) {
            if (modes[eventKey] !== modes[0])
                flag = true;
            this.setState({ selectedMode: modes[eventKey], isArchive: flag });
            this.props.isArchiveMode(flag, this.state.viewTsRZA, this.state.viewTsOIC);
        }
    }

    callbackGetDataPicker = (sDate, eDate) => {
        this.props.dataArchiveMode(this.state.viewTsRZA, this.state.viewTsOIC, sDate, eDate);
    }

    handleSwitch(e) {
        let tempViewTsRZA = this.state.viewTsRZA;
        let tempViewTsOIC = this.state.viewTsOIC;
        if (e.target.id == 'rzaSwitch') {
            this.setState({ viewTsRZA: !this.state.viewTsRZA});
            tempViewTsRZA = !this.state.viewTsRZA;
        }
        else {
            this.setState({ viewTsOIC: !tempViewTsOIC });
            tempViewTsOIC = !this.state.viewTsOIC;
        }

        if (!this.state.isArchive)
            this.props.isArchiveMode(this.state.isArchive, tempViewTsRZA, tempViewTsOIC);
    }

    render() {
        const {
            selectedMode,
            isArchive,
            viewTsRZA,
            viewTsOIC
        } = this.state;

        let fileName = viewTsRZA && viewTsOIC
            ? 'SummaryExportedListMonAPTS'
            : viewTsRZA
                ? 'RZAExportedListMonAPTS'
                : viewTsOIC
                    ? 'OICExportedListMonAPTS'
                    : '';

        return (
            <div style={{ display: 'inline-flex', width: '100%' }}>
                <div style={{ margin: '5px 10px 5px 0' }}>
                    <span>
                        <b>Режим просмотра:</b>
                    </span>
                </div>
                <DropdownButton
                    title={selectedMode}
                    variant={isArchive ? "secondary" : "success" }
                    id="document-type"
                    onSelect={this.handleSelect.bind(this)}
                >
                    {modes.map((mode, i) => (
                        <Dropdown.Item key={i} eventKey={i}>
                            {mode}
                        </Dropdown.Item>
                    ))}
                </DropdownButton>
                {
                    isArchive
                        ? <DPicker dataPicker={this.callbackGetDataPicker.bind(this)} isDisabled={!viewTsRZA && !viewTsOIC ? true : false} />
                        :   <></>
                }
                <div style={{ margin: '7px 0 5px 30px' }}>
                    <span>
                        <Form.Check
                            type="switch"
                            label="ТС РЗА"
                            id="rzaSwitch"
                            checked={viewTsRZA}
                            onChange={this.handleSwitch.bind(this)}
                            disabled={this.props.loading ? "disabled" : false}
                        />
                    </span>
                </div>
                <div style={{ margin: '7px 0 5px 30px' }}>
                    <span>
                        <Form.Check
                            type="switch"
                            label="ТС ОИК"
                            id="oicSwitch"
                            checked={viewTsOIC}
                            onChange={this.handleSwitch.bind(this)}
                            disabled={this.props.loading ? "disabled" : false}
                        />
                    </span>
                </div>
                <div style={{ margin: '5px 0 5px 30px', marginLeft: 'auto', float: 'right' }}>
                    <span>
                        <Export filename={fileName} data={this.props.listToExport} disabled={this.props.loading}/>
                    </span>
                </div>
            </div>            
        );
    };
}