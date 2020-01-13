import React from 'react';

const AccessDenied = () => {
    var divStyle = {
        color: 'red',
        marginTop: 100
    }

    return (
        <div style={divStyle}>
            <h2>Доступ запрещен.</h2>
            <p>У Вас недостаточно прав для доступа к этому ресурсу.</p>
        </div>
    );
}

export default AccessDenied;