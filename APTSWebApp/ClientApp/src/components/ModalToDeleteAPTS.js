import React from 'react';
import { Button, Modal } from 'react-bootstrap';

const ModalToDeleteApts = (props) => {
    return (
        <Modal show={props.show} onHide={props.onClose} centered>
            <Modal.Header closeButton={props.onClose}>
                <Modal.Title>Удаление АПТС</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                Удалить выбранные телесигналы?
            </Modal.Body>
            <Modal.Footer>
                <Button variant="danger" onClick={props.onDelete}>Удалить</Button>{' '}
                <Button variant="secondary" onClick={props.onClose}>Отмена</Button>
            </Modal.Footer>
        </Modal>
    );
}

export default ModalToDeleteApts;
