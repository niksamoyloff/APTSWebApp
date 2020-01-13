import React  from 'react';
import { ClipLoader } from 'react-spinners';
import './CustomLoader.css';

import { css } from '@emotion/core';

const override = css`
    display: block;
    margin: 0 auto;
    //border-color: red;
`;

const LoaderAPTS = (loading) => {
    return (
        <div className="row">
            <div className="align-self-center aptsLoader">
                <ClipLoader
                    css={override}
                    sizeUnit={"px"}
                    size={50}
                    color={'black'}
                    loading={loading}
                />
            </div>
        </div>  
    );
}

export default LoaderAPTS;