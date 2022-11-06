var selectedTab = "test1"

document.addEventListener('DOMContentLoaded', function () {

    var toggler = document.getElementsByClassName("caret");
    var i;


    // Expand tree by default
    for (i = 0; i < toggler.length; i++) {
        toggler[i].parentElement.querySelector(".nested").classList.toggle("active");
        toggler[i].classList.toggle("caret-down");
    }

    for (i = 0; i < toggler.length; i++) {
        toggler[i].addEventListener("click", function () {
            this.parentElement.querySelector(".nested").classList.toggle("active");
            this.classList.toggle("caret-down");
        });
    }

    updateBody(selectedTab);

    //getElementById('container__right').style.width='10px';
    //document.getElementById('container__right').setAttribute("style","width:500px");
})


function updateBody(tabId, page) {
  document.getElementById(selectedTab).setAttribute("class", "item");
  tab = document.getElementById(tabId)
  tab.setAttribute("class", "selected");
  selectedTab = tabId;
  
  iframe = document.getElementById("myframe");
  iframe.src = encodeURIComponent(tab.getAttribute("value")).replace(/%2F/g, '/');
}