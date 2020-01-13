import React from 'react';
import { Button, Modal } from 'react-bootstrap';

const NestedModalToActionAPTS = (props) => {
    return (
        <Modal show={props.show} onHide={props.onClose} centered>
            <Modal.Header closeButton={props.onClose}>
                <Modal.Title>Предупреждение</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                Вы действительно хотите прервать { props.action === "Add" ? "добавление" : "изменение" } АПТС?
            </Modal.Body>
            <Modal.Footer>
                <Button variant="info" onClick={props.onAbort}>Да</Button>{' '}
                <Button variant="secondary" onClick={props.onClose}>Отмена</Button>
            </Modal.Footer>
        </Modal>
    );
}

export default NestedModalToActionAPTS;