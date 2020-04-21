import React from 'react';

const DropdownFilter = ({ data, fieldName, filter, onChange }) => {

    return (
        <select
            onChange={event => onChange(event.target.value == "all" ? "" : event.target.value)}
            style={{ width: "100%" }}
            value={filter ? filter.value : "all"}
        >
            <option value="all">Все</option>
            {data
                .map(item => item[fieldName])
                .filter((item, i, s) => s.lastIndexOf(item) == i)
                .map(function (value) {
                    return (
                        <option key={value} value={value}>
                            {value}
                        </option>
                    );
                })}
        </select>
    );
};

export default DropdownFilter;