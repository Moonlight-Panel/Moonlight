function flashbang() {
    const light = document.getElementById("flashbang");
    light.style.boxShadow = "0 0 10000px 10000px white, 0 0 250px 10px #FFFFFF";
    light.style.animation = "flashbang 5s linear forwards";
    light.onanimationend = clearFlashbang;
}

function clearFlashbang() {
    const light = document.getElementById("flashbang");
    light.style.animation = "";
    light.style.opacity = "0";
}