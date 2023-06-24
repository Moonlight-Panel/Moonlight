window.moonlight =
    {
        modals: {
            show: function (name) {
                $('#' + name).modal('show');
            },
            hide: function (name) {
                $('#' + name).modal('hide');
            }
        },
        alerts: {
            info: function (title, description) {
                Swal.fire(
                    title,
                    description,
                    'info'
                )
            },
            success: function (title, description) {
                Swal.fire(
                    title,
                    description,
                    'success'
                )
            },
            warning: function (title, description) {
                Swal.fire(
                    title,
                    description,
                    'warning'
                )
            },
            error: function (title, description) {
                Swal.fire(
                    title,
                    description,
                    'error'
                )
            },
            yesno: function (title, yesText, noText) {
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
            },
            text: function (title, description) {
                const {value: text} = Swal.fire({
                    title: title,
                    input: 'text',
                    inputLabel: description,
                    inputValue: "",
                    showCancelButton: false,
                    inputValidator: (value) => {
                        if (!value) {
                            return 'You need to enter a value'
                        }
                    }
                })

                return text;
            }
        },
        clipboard: {
            copy: function (text) {
                if (!navigator.clipboard) {
                    var textArea = document.createElement("textarea");
                    textArea.value = text;

                    // Avoid scrolling to bottom
                    textArea.style.top = "0";
                    textArea.style.left = "0";
                    textArea.style.position = "fixed";

                    document.body.appendChild(textArea);
                    textArea.focus();
                    textArea.select();

                    try {
                        var successful = document.execCommand('copy');
                        var msg = successful ? 'successful' : 'unsuccessful';
                    } catch (err) {
                        console.error('Fallback: Oops, unable to copy', err);
                    }

                    document.body.removeChild(textArea);
                    return;
                }
                navigator.clipboard.writeText(text).then(function () {
                    },
                    function (err) {
                        console.error('Async: Could not copy text: ', err);
                    }
                );
            }
        },
        recaptcha: {
            render: function (id, sitekey, page) {
                return grecaptcha.render(id, {
                    'sitekey': sitekey,
                    'callback': (response) => {
                        page.invokeMethodAsync('CallbackOnSuccess', response);
                    },
                    'expired-callback': () => {
                        page.invokeMethodAsync('CallbackOnExpired');
                    }
                });
            }
        },
        snow: {
            create: function () {
                (function () {
                    var requestAnimationFrame = window.requestAnimationFrame || window.mozRequestAnimationFrame || window.webkitRequestAnimationFrame || window.msRequestAnimationFrame ||
                        function (callback) {
                            window.setTimeout(callback, 1000 / 60);
                        };
                    window.requestAnimationFrame = requestAnimationFrame;
                })();


                var flakes = [],
                    canvas = document.getElementById("snow"),
                    ctx = canvas.getContext("2d"),
                    flakeCount = 200,
                    mX = -100,
                    mY = -100

                canvas.width = window.innerWidth;
                canvas.height = window.innerHeight;

                function snow() {
                    ctx.clearRect(0, 0, canvas.width, canvas.height);

                    for (var i = 0; i < flakeCount; i++) {
                        var flake = flakes[i],
                            x = mX,
                            y = mY,
                            minDist = 150,
                            x2 = flake.x,
                            y2 = flake.y;

                        var dist = Math.sqrt((x2 - x) * (x2 - x) + (y2 - y) * (y2 - y)),
                            dx = x2 - x,
                            dy = y2 - y;

                        if (dist < minDist) {
                            var force = minDist / (dist * dist),
                                xcomp = (x - x2) / dist,
                                ycomp = (y - y2) / dist,
                                deltaV = force / 2;

                            flake.velX -= deltaV * xcomp;
                            flake.velY -= deltaV * ycomp;

                        } else {
                            flake.velX *= .98;
                            if (flake.velY <= flake.speed) {
                                flake.velY = flake.speed
                            }
                            flake.velX += Math.cos(flake.step += .05) * flake.stepSize;
                        }

                        ctx.fillStyle = "rgba(255,255,255," + flake.opacity + ")";
                        flake.y += flake.velY;
                        flake.x += flake.velX;

                        if (flake.y >= canvas.height || flake.y <= 0) {
                            reset(flake);
                        }


                        if (flake.x >= canvas.width || flake.x <= 0) {
                            reset(flake);
                        }

                        ctx.beginPath();
                        ctx.arc(flake.x, flake.y, flake.size, 0, Math.PI * 2);
                        ctx.fill();
                    }
                    requestAnimationFrame(snow);
                };

                function reset(flake) {
                    flake.x = Math.floor(Math.random() * canvas.width);
                    flake.y = 0;
                    flake.size = (Math.random() * 3) + 2;
                    flake.speed = (Math.random() * 1) + 0.5;
                    flake.velY = flake.speed;
                    flake.velX = 0;
                    flake.opacity = (Math.random() * 0.5) + 0.3;
                }

                function init() {
                    for (var i = 0; i < flakeCount; i++) {
                        var x = Math.floor(Math.random() * canvas.width),
                            y = Math.floor(Math.random() * canvas.height),
                            size = (Math.random() * 3) + 2,
                            speed = (Math.random() * 1) + 0.5,
                            opacity = (Math.random() * 0.5) + 0.3;

                        flakes.push({
                            speed: speed,
                            velY: speed,
                            velX: 0,
                            x: x,
                            y: y,
                            size: size,
                            stepSize: (Math.random()) / 30,
                            step: 0,
                            opacity: opacity
                        });
                    }

                    snow();
                };

                canvas.addEventListener("mousemove", function (e) {
                    mX = e.clientX,
                        mY = e.clientY
                });

                window.addEventListener("resize", function () {
                    canvas.width = window.innerWidth;
                    canvas.height = window.innerHeight;
                })

                init();
            }
        },
        toasts: {
            info: function (msg) {
                toastr['info'](msg);
            },
            error: function (msg) {
                toastr['error'](msg);
            },
            success: function (msg) {
                toastr['success'](msg);
            },
            warning: function (msg) {
                toastr['warning'](msg);
            },
            create: function (id, text) {
                var toast = toastr.success(text, '',
                    {
                        closeButton: true,
                        progressBar: false,
                        tapToDismiss: false,
                        timeOut: 0,
                        extendedTimeOut: 0,
                        positionClass: "toastr-bottom-right",
                        preventDuplicates: false,
                        onclick: function () {
                            toastr.clear(toast);
                        }
                    });
                var toastElement = toast[0];
                toastElement.setAttribute('data-toast-id', id);
                toastElement.classList.add("bg-secondary");
            },
            modify: function (id, newText) {
                var toast = document.querySelector('[data-toast-id="' + id + '"]');

                if (toast) {
                    var toastMessage = toast.lastChild;
                    if (toastMessage) {
                        toastMessage.innerHTML = newText;
                    }
                }
            },
            remove: function (id) {
                var toast = document.querySelector('[data-toast-id="' + id + '"]');
                if (toast) {
                    toast.childNodes.item(1).click();
                }
            }
        },
        utils: {
            scrollToElement: function (id)
            {
                let e = document.getElementById(id);
                e.scrollTop = e.scrollHeight;
            },
            triggerResizeEvent: function ()
            {
                window.dispatchEvent(new Event('resize'));
            },
            showNotification: function (title, text, img) {
                let notification = new Notification(title, { body: text, icon: img });
            }
        },
        loading: {
            registerXterm: function()
            {
                console.log("Registering xterm addons");
                
                window.XtermBlazor.registerAddon("xterm-addon-fit", new window.FitAddon.FitAddon());
                //window.XtermBlazor.registerAddon("xterm-addon-search", new window.SearchAddon.SearchAddon());
                //window.XtermBlazor.registerAddon("xterm-addon-web-links", new window.WebLinksAddon.WebLinksAddon());
            },
            loadMonaco: function ()
            {
                console.log("Loading monaco");
                
                monaco.editor.defineTheme('moonlight-theme', {
                    base: 'vs-dark',
                    inherit: true,
                    rules: [
                    ],
                    colors: {
                        'editor.background': '#000000'
                    }
                });
            }
        },
        flashbang: {
            run: function()
            {
                const light = document.getElementById("flashbang");
                light.style.boxShadow = "0 0 10000px 10000px white, 0 0 250px 10px #FFFFFF";
                light.style.animation = "flashbang 5s linear forwards";
                light.onanimationend = moonlight.flashbang.clean;
            },
            clean: function()
            {
                const light = document.getElementById("flashbang");
                light.style.animation = "";
                light.style.opacity = "0";
            }
        },
        downloads:{
            downloadStream: async function (fileName, contentStreamReference){
                const arrayBuffer = await contentStreamReference.arrayBuffer();
                const blob = new Blob([arrayBuffer]);
                const url = URL.createObjectURL(blob);
                const anchorElement = document.createElement('a');
                anchorElement.href = url;
                anchorElement.download = fileName ?? '';
                anchorElement.click();
                anchorElement.remove();
                URL.revokeObjectURL(url);
            }
        },
        keyListener: {
            register: function (dotNetObjRef)
            {
                moonlight.keyListener.listener = (event) => 
                {
                    // filter here what key events should be sent to moonlight

                    if(event.code === "KeyS" && event.ctrlKey)
                    {
                        event.preventDefault();
                        dotNetObjRef.invokeMethodAsync('OnKeyPress', "saveShortcut");
                    }
                };
                
                window.addEventListener('keydown', moonlight.keyListener.listener);
            },
            unregister: function (dotNetObjRef)
            {
                window.removeEventListener('keydown', moonlight.keyListener.listener);
            }
        },
        serverList: {
            init: function ()
            {
                if(moonlight.serverList.Swappable)
                {
                    moonlight.serverList.Swappable.destroy();
                }
                
                let containers = document.querySelectorAll(".draggable-zone");

                if (containers.length !== 0) 
                {
                    moonlight.serverList.Swappable = new Draggable.Sortable(containers, {
                        draggable: ".draggable",
                        handle: ".draggable .draggable-handle",
                        mirror: {
                            //appendTo: selector,
                            appendTo: "body",
                            constrainDimensions: true
                        }
                    });
                }
            },
            getData: function ()
            {
                let groups = new Array();

                let groupElements = document.querySelectorAll('[ml-server-group]');
                
                groupElements.forEach(groupElement => {
                   let group = new Object();
                   group.name = groupElement.attributes.getNamedItem("ml-server-group").value;

                   let servers = new Array();
                   let serverElements = groupElement.querySelectorAll("[ml-server-id]");
                   
                   serverElements.forEach(serverElement => {
                      let id = serverElement.attributes.getNamedItem("ml-server-id").value;
                      
                      servers.push(id);
                   });
                   
                   group.servers = servers;
                   groups.push(group);
                });

                return groups;
            }
        }
    };