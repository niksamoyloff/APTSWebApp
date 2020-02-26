import React, { Component } from 'react';
import { Input, ListGroup } from 'reactstrap';
import TreeMenu from 'react-simple-tree-menu';
import ExportListAPTS from './ExportListAPTS';
import { ListItem } from './ListItem';
import LoaderAPTS from './LoaderAPTS';
import '../../node_modules/react-simple-tree-menu/dist/main.css';

export class TreePowObj extends Component {
    displayName = TreePowObj.name

    constructor(props) {
        super(props);
        this.state = { enObjects: [], treeToExport: [], loading: true };
    }

    componentDidMount() {
        this.populateEnObjects();
        this.exportDevTreeAPTS();
    }

    async populateEnObjects() {
        const response = await fetch('Admin/GetTree');
        const data = await response.json();
        this.setState({ enObjects: data, loading: false });
    }    

    async exportDevTreeAPTS() {        
        const response = await fetch('Admin/ExportDevTreeAPTS');
        const list = await response.json();
        this.setState({ treeToExport: list });
    }

    render() {
        let contents = this.state.loading
            ?
            <LoaderAPTS />
            :
            <>
                <ExportListAPTS data={this.state.treeToExport} filename="ExportedTreeDeviceListAPTS" />
                <div style={{ marginTop: '5px' }}>
                    <TreeMenu data={this.state.enObjects} debounceTime={125} >
                        {({ search, items }) => (
                            <>
                                {
                                    //{<Input onChange={e => search(e.target.value)} placeholder="Поиск..." />
                                }
                                <ListGroup>
                                    {items.map(({ ...props }) => (
                                        <ListItem {...props} onClick={(e) => this.props.getKey(e, props)} />
                                    ))}
                                </ListGroup>
                            </>
                        )}
                    </TreeMenu>
                </div>
            </>          
        
        return (
            <div>
                <h3>Оборудование РЗА</h3>
                {contents}
            </div>
        );
    }
}


