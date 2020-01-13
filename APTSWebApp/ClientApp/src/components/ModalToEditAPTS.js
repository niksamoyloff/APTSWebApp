import React, { useState } from 'react';
import { Button, Modal, Form, Row, Col } from 'react-bootstrap';

const ModalToEditAPTS = (props) => {
    const [commentVal, setCommentVal] = useState(props.comment)
    const [status, setStatus] = useState(props.status)
    const [tsId] = useState(props.tsId)
    const [isEditComment, setIsEditComment] = useState(false);
    const [isEditStatus, setIsEditStatus] = useState(false);

    const handleCommentChange = e => {
        const { value } = e.target;
        setIsEditComment(true);
        setCommentVal(value);        
    }

    const handleStatusChange = () => {
        setIsEditStatus(true);
        setStatus(!props.status);
    }

    const handleToSave = () => {
        let st = isEditStatus ? status : props.status;
        let cmt = isEditComment ? commentVal : props.comment;

        props.onEdit(tsId, st, cmt)
    }
    return (
        <Modal dialogClassName="modalToEdit" show={props.show} onHide={props.onClose} centered>
            <Modal.Header closeButton={props.onClose}>
                <Modal.Title>{props.label}</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <Form>
                    <Form.Group as={Row} controlId="formPlaintextEmail">
                        <Form.Label style={{ padding:'0 15px' }} >
                            Сигнал состояния:
                        </Form.Label>
                        <Form.Check defaultChecked={props.status} style={{ marginTop: '2px' }} onChange={handleStatusChange} />
                    </Form.Group>
                    <Form.Group as={Row} controlId="exampleForm.ControlTextarea1">
                        <Form.Label column sm="3">
                            Примечание:
                        </Form.Label>
                        <Col sm="12">
                            <Form.Control as="textarea" rows="3" defaultValue={props.comment} onChange={handleCommentChange} />
                        </Col>
                    </Form.Group>
                </Form>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="primary" onClick={handleToSave}>Сохранить</Button>{' '}
                <Button variant="secondary" onClick={props.onClose}>Отмена</Button>
            </Modal.Footer>
        </Modal>
    );
}

export default ModalToEditAPTS;