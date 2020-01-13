import React from 'react';

const NotFound = () => {
    var divStyle = {
        marginTop: 100
    }

    return (
        <div style={divStyle}>
            <h2>404.</h2>
            <p>Запрошенный ресурс не найден.</p>
        </div>
    );
}

export default NotFound;