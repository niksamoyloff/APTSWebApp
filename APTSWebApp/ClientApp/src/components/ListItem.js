import React, { Component } from 'react';
import { ListGroupItem } from 'reactstrap';

import closedIconImg from '../Icons/closedIcon.png';
import openedIconImg from '../Icons/openedIcon.png';

const iconStyle = {
    verticalAlign: 'text-bottom',
};

const openedIcon = <img src={openedIconImg} alt="-" style={iconStyle}></img>;
const closedIcon = <img src={closedIconImg} alt="+" style={iconStyle}></img>;

const DEFAULT_PADDING = 16;
const ICON_SIZE = 8;
const LEVEL_SPACE = 16;

const ToggleIcon = ({ on, openedIcon, closedIcon }) => <span style={{ marginRight: 8 }}>{on ? openedIcon : closedIcon}</span>;

export class ListItem extends Component {
    constructor(props) {
        super(props);
    }

    render() {
        const {
            level = 0,
            hasNodes,
            isOpen,
            label,
            searchTerm,
            openNodes,
            toggleNode,
            matchSearch,
            focused,
            ...props
        } = this.props;

        return (
            <ListGroupItem
                {...props}
                style={{
                    paddingLeft: DEFAULT_PADDING + ICON_SIZE + level * LEVEL_SPACE,
                    cursor: 'pointer',
                    boxShadow: focused ? '0px 0px 5px 0px #222' : 'none',
                    zIndex: focused ? 999 : 'unset',
                    position: 'relative'
                }}                
            >
                {hasNodes && (
                    <div
                        style={{ display: 'inline-block' }}
                        onClick={e => {
                            hasNodes && toggleNode && toggleNode();
                            e.stopPropagation();
                        }}
                    >
                        <ToggleIcon on={isOpen} openedIcon={openedIcon} closedIcon={closedIcon}/>
                    </div>
                )}
                {label}
            </ListGroupItem>
        );
    }
}