function showSidebarDrawer() {
    let sidebar = document.getElementsByClassName("app-sidebar")[0];

    sidebar.classList.add("drawer");
    sidebar.classList.add("drawer-start");
    sidebar.setAttribute("data-kt-drawer-overlay", "true");
    
    setTimeout(() => {
        sidebar.classList.add("drawer-on");

        let disableOverlay = document.createElement("div");

        disableOverlay.classList.add("drawer-overlay");
        disableOverlay.style.zIndex = "105";

        disableOverlay.onclick = () => hideSidebarDrawer();

        document.body.appendChild(disableOverlay);
    }, 100);
}

function hideSidebarDrawer()
{
    let sidebar = document.getElementsByClassName("app-sidebar")[0];

    sidebar.classList.remove("drawer-on");

    setTimeout(() => {
        sidebar.classList.remove("drawer");
        sidebar.classList.remove("drawer-start");


        sidebar.setAttribute("data-kt-drawer-overlay", "false");

        let disableOverlay = document.getElementsByClassName("drawer-overlay")[0];
        disableOverlay.remove();
    }, 350);
}
