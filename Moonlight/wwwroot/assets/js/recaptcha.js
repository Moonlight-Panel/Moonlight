window.recaptcha = new Object();
window.recaptcha.render = function (id, sitekey, page)
{
    return grecaptcha.render(id, {
        'sitekey': sitekey,
        'callback': (response) => { page.invokeMethodAsync('CallbackOnSuccess', response); },
        'expired-callback': () => { page.invokeMethodAsync('CallbackOnExpired'); }
    });
}