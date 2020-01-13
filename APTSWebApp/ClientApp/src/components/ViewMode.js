import React from 'react';
import { Button, Dropdown, DropdownButton } from 'react-bootstrap';
import { DPicker } from './DPicker';

const modes = ["Оперативный", "Архив"];

export class ViewMode extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            selectedMode: modes[0],
            isArchive: false
        }
    }

    handleSelect(eventKey, event) {
        let flag = false;
        if (modes[eventKey] !== this.state.selectedMode) {
            if (modes[eventKey] !== modes[0])
                flag = true;
            this.setState({ selectedMode: modes[eventKey], isArchive: flag });
            this.props.isArchiveMode(flag);
        }
    }

    callbackGetDataPicker = (sDate, eDate) => {
        this.props.dataArchiveMode(sDate, eDate);
    }

    render() {
        const { selectedMode, isArchive } = this.state;

        return (
            <div style={{ display: 'inline-flex' }}>
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
                        ?   <DPicker dataPicker={this.callbackGetDataPicker.bind(this)} />
                        :   <></>
                }
            </div>            
        );
    };
}