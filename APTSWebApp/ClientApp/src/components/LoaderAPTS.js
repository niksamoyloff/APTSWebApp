import React  from 'react';
import { Spinner } from 'react-bootstrap';
import './CustomLoader.css';

const LoaderAPTS = () => {
    return (
        <div className="row">
            <div className="align-self-center aptsLoader">
                <Spinner animation="border" />
            </div>
        </div>  
    );
}

export default LoaderAPTS;