window.showAlertInfo = function (title, description)
{
    Swal.fire(
        title,
        description,
        'info'
    )
}

window.showAlertSuccess = function (title, description)
{
    Swal.fire(
        title,
        description,
        'success'
    )
}

window.showAlertWarning = function (title, description)
{
    Swal.fire(
        title,
        description,
        'warning'
    )
}

window.showAlertError = function (title, description)
{
    Swal.fire(
        title,
        description,
        'error'
    )
}

window.showAlertYesNo = function (title, yesText, noText)
{
    return Swal.fire({
        title: title,
        showDenyButton: true,
        confirmButtonText: yesText,
        denyButtonText: noText,
    }).then((result) => {
        if (result.isConfirmed) {
            return true;
        } else if (result.isDenied) {
            return false;
        }
    })
}

window.showAlertText = function (title, description) {
    const {value: text} = Swal.fire({
        title: title,
        input: 'text',
        inputLabel: description,
        inputValue: "",
        showCancelButton: false,
        inputValidator: (value) => {
            if (!value) {
                return 'Es muss ein Wert angegeben werden'
            }
        }
    })
    
    return text;
}