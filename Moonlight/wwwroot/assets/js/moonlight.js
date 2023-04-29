window.moonlight = 
{
    modals: {
        show: function (name)
        {
            $('#' + name).modal('show');
        },
        hide: function (name)
        {
            $('#' + name).modal('hide');
        }
    }    
};