window.logInfo = function (prefix, message)
{
    console.log(
        '%c[%cINFO%c] [%c' + prefix + '%c] %c' + message,
        'color: white', // [
        'color: aqua', // INFO
        'color: white', // ]
        'color: purple', // {prefix}
        'color: white', // ]
        'color: lightgray' // {message}
    );
};

window.logWarn = function (prefix, message)
{
    console.log(
        '%c[%cWARN%c] [%c' + prefix + '%c] %c' + message,
        'color: white', // [
        'color: orange', // WARN
        'color: white', // ]
        'color: purple', // {prefix}
        'color: white', // ]
        'color: lightgray' // {message}
    );
};

window.logError = function (prefix, message)
{
    console.log(
        '%c[%cERROR%c] [%c' + prefix + '%c] %c' + message,
        'color: white', // [
        'color: red', // ERROR
        'color: white', // ]
        'color: purple', // {prefix}
        'color: white', // ]
        'color: lightgray' // {message}
    );
};

window.logDebug = function (prefix, message)
{
    console.log(
        '%c[%cDEBUG%c] [%c' + prefix + '%c] %c' + message,
        'color: white', // [
        'color: green', // DEBUG
        'color: white', // ]
        'color: purple', // {prefix}
        'color: white', // ]
        'color: lightgray' // {message}
    );
};