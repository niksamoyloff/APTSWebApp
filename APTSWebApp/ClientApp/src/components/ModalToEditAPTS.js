import React, { useState, useEffect } from 'react';
import { Button, Modal, Form, Row, Col } from 'react-bootstrap';

function ModalToEditAPTS(props) {
    const [commentVal, setCommentVal] = useState(props.comment)
    const [isStatusTS, setIsStatusTS] = useState(props.status)
    //const [isOicTS, setIsOicTS] = useState(props.isOic)
    const [tsId] = useState(props.tsId)

    const handleCommentChange = (val) => {
        setCommentVal(val);        
    }

    const handleStatusChange = () => {
        setIsStatusTS(!isStatusTS);
    }

    const handleToSave = () => {
        props.onEdit(tsId, isStatusTS, commentVal)
    }

    useEffect(() => {
        setIsStatusTS(props.status);
        setCommentVal(props.comment);
    }, [props.status, props.comment])

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
                        <Form.Check checked={isStatusTS} style={{ marginTop: '2px' }} onChange={() => handleStatusChange()} />
                    </Form.Group>
                    <Form.Group as={Row} controlId="exampleForm.ControlTextarea1">
                        <Form.Label column sm="3">
                            Примечание:
                        </Form.Label>
                        <Col sm="12">
                            <Form.Control as="textarea" rows="3" value={commentVal} onChange={(e) => handleCommentChange(e.target.value)} />
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