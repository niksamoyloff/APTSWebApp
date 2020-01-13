import React from 'react';
import SplitterLayout from 'react-splitter-layout';
import 'react-splitter-layout/lib/index.css';
import { TreePowObj } from './TreePowObj';
import './CustomSplitter.css'
import TableAPTS from './TableAPTS';
import $ from 'jquery';

export class Splitter extends React.Component {
    displayName = Splitter.name

    constructor(props) {
        super(props);

        this.state = {
            deviceId: '',
            showModal: false,
            deviceName: '',
            enObjName: '',
            primaryName: '',
            powerSysName: ''
        };
    }

    getKeyForItemLevel = (e, p) => {
        p.onClick();
        if (p.level === 3) {
            this.getNames(e, p.key, p.label);
        }
        else {
            this.setState({
                deviceId: '',
                deviceName: '',
                enObjName: '',
                primaryName: '',
                powerSysName: ''
            });
        }        
    }

    getNames = (e, key, label) => {
        let objId = e.target.getAttribute("parent").split("/")[0];
        let primaryId = e.target.getAttribute("parent").split("/")[1]; 

        let objName = $(e.target).prevAll('li[parent="' + objId + '"]').first().clone().children().remove().end().text();
        let priName = $(e.target).prevAll('li[parent="' + objId + '/' + primaryId + '"]').first().clone().children().remove().end().text();
        let powSysName = $(e.target).prevAll('li[parent=""]').first().clone().children().remove().end().text();

        this.setState({
            deviceId: key,
            deviceName: label,
            enObjName: objName,
            primaryName: priName,
            powerSysName: powSysName
        })
    }

    render() {
        return (
            <SplitterLayout customClassName="splitterLyt" percentage primaryMinSize={25} secondaryMinSize={25}>
                <div className="splitterPane">
                    <TreePowObj getKey={this.getKeyForItemLevel}/>
                </div>
                <div className="splitterPane">
                    <h3>Перечень АПТС</h3>
                    {
                        this.state.deviceId === ''
                            ? <p><em>Необходимо выбрать устройство РЗА.</em></p>
                            : <TableAPTS
                                deviceId={this.state.deviceId}
                                deviceName={this.state.deviceName}
                                primaryName={this.state.primaryName}
                                enObjName={this.state.enObjName}
                                powerSysName={this.state.powerSysName}
                            />
                    }                    
                </div>
            </SplitterLayout>
        );
    }
}