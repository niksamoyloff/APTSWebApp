import React, { Component } from 'react';
import { Input, ListGroup } from 'reactstrap';
import TreeMenu from 'react-simple-tree-menu';
import Export from './ExportListAPTS';
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

    searchDeviceByName(val) {
        let filter = val.toUpperCase();
        let list = document.getElementById("devList");
        let li = list.getElementsByTagName("li");
        this.searchDevInDev(li, filter);
    }

    searchDevInDev(liList, filter) {
        for (let i = 0; i < liList.length; i++) {
            let liName = liList[i].innerText;

            if (liName) {
                if (liName.toUpperCase().indexOf(filter) > -1) {
                    liList[i].style.display = "";
                    let liImgs = li[i].getElementsByTagName("img");
                    if (liImgs.length)
                        liImg.item(0).click();
                }
                //else {
                //    li[i].style.display = "none";
                //}
            }
        }
    }

    render() {
        const { loading, treeToExport } = this.state;
        let contents = this.state.loading
            ?
            <LoaderAPTS />
            :
            <>
                <Export data={treeToExport} filename="ExportedTreeDeviceListAPTS" disabled={loading} />
                <div style={{ marginTop: '5px' }}>
                    <TreeMenu data={this.state.enObjects} debounceTime={125} >
                        {({ search, items }) => (
                            <>
                                {
                                    <Input onChange={e => search(e.target.value)} placeholder="Поиск..." />
                                }
                                <ListGroup id="devList">
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


