import React, { Component } from 'react';
import { Collapse, Container, Navbar, NavbarBrand, NavbarToggler, NavItem, NavLink } from 'reactstrap';
//import { Nav, Navbar, Container } from 'react-bootstrap';
import { NavLink as RRNavLink, Link } from 'react-router-dom';
import './NavMenu.css';

export class NavMenu extends Component {
    static displayName = NavMenu.name;

    constructor(props) {
        super(props);

        this.toggleNavbar = this.toggleNavbar.bind(this);
        this.getUserName = this.getUserName.bind(this);
        this.state = {
            collapsed: true,
            userName: '',
        };
    }

    toggleNavbar() {
        this.setState({
            collapsed: !this.state.collapsed
        });
    }

    componentDidMount() {
        this.getUserName();
    }

    async getUserName() {
        const response = await fetch('Home/GetUserName');
        const name = await response.text();
        this.setState({ userName: name });
    }

    render() {
        return (
            <header>
                { 
                    // <Navbar fixed="top" className="navbar navbar-expand-sm navbar-toggleable-sm border-bottom box-shadow mb-3" light>
                    <Navbar expand="lg" color="light" fixed="top" className="navbar-expand-sm navbar-toggleable-sm border-bottom box-shadow mb-3" light>
                        <Container>
                            <NavbarBrand tag={Link} to="/"><span id="Logo">Мониторинг АПТС</span></NavbarBrand>
                            <NavbarToggler onClick={this.toggleNavbar} className="mr-2" />
                            <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={!this.state.collapsed} navbar>
                                <ul className="navbar-nav flex-grow">
                                    <NavItem>
                                        <NavLink to="/" exact tag={RRNavLink}><span className="navItemName">Мониторинг</span></NavLink>
                                    </NavItem>
                                    <NavItem>
                                        <NavLink to="/tree" tag={RRNavLink}><span className="navItemName">Энергообъекты</span></NavLink>
                                    </NavItem>
                                    <NavItem>
                                        <NavLink to="/history" tag={RRNavLink}><span className="navItemName">История изменений</span></NavLink>
                                    </NavItem>
                                    <NavItem>
                                        <NavLink>Пользователь: {this.state.userName}</NavLink>
                                    </NavItem>                                    
                                </ul>
                            </Collapse>
                        </Container>
                    </Navbar>
                }
                
            </header>
        );
    }
}

{
    //
    //      reactstrap
    //
    //<Navbar className="navbar-expand-sm navbar-toggleable-sm ng-white border-bottom box-shadow mb-3" light>
    //    <Container>
    //        <NavbarBrand tag={Link} to="/">APTSWebApp</NavbarBrand>
    //        <NavbarToggler onClick={this.toggleNavbar} className="mr-2" />
    //        <Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={!this.state.collapsed} navbar>
    //            <ul className="navbar-nav flex-grow">
    //                <NavItem>
    //                    <NavLink tag={Link} className="text-dark" to="/">Мониторинг</NavLink>
    //                </NavItem>
    //                <NavItem>
    //                    <NavLink tag={Link} className="text-dark" to="/tree">Энергообъекты</NavLink>
    //                </NavItem>
    //            </ul>
    //        </Collapse>
    //    </Container>
    //</Navbar>


    //
    //      react-bootstrap
    //
    //<Navbar bg="light" variant="light" fixed="top">
    //    <Container>
    //        <Navbar.Brand tag={Link} href="/" to="/">АПТС</Navbar.Brand>
    //        <Navbar.Toggle onClick={this.toggleNavbar} className="mr-2" />
    //        <Navbar.Collapse className="d-sm-inline-flex flex-sm-row-reverse" isOpen={!this.state.collapsed} navbar>
    //            <ul className="navbar-nav flex-grow">
    //                <Nav.Item>
    //                    <Nav.Link tag={Link} href="/" className="text-dark" to="/">Мониторинг</Nav.Link>
    //                </Nav.Item>
    //                <Nav.Item>
    //                    <Nav.Link tag={Link} href="/tree" className="text-dark" to="/tree">Энергообъекты</Nav.Link>
    //                </Nav.Item>
    //            </ul>
    //        </Navbar.Collapse>
    //    </Container>
    //</Navbar>
}