function getDateNow() {
    var dayNames = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
    var now = new Date();
    var dateText = now.toISOString();
    tempArray = dateText.split("T");
    dateText = tempArray[0];
    var iDay = now.getDay();
    var dayName = dayNames[iDay];
    dateText = dateText + " " + dayName;
    return dateText;
}

function getTimeNow() {
    var now = new Date();
    var timeText = now.toLocaleTimeString();
    return trimSecDigits(timeText);
}

function checkIfElemExists(elemId) {
    var bExists = false;
    var elem = document.getElementById(elemId);

    if ((typeof (elem) != 'undefined') && (elem != null)) {
        bExists = true;
    }
    return bExists;
}
//
// --------------------------------------------
//  Input:
//      "7:44:34 PM" or "2021-05-14 7:44:34 PM"
//  Output:
//      "7:44 PM" or "2021-05-14 7:44 PM"
// --------------------------------------------
//
function trimSecDigits(dateTimeText) {
    var idxLastColon = dateTimeText.lastIndexOf(":");
    var timeTextWithoutSec = dateTimeText.substring(0, idxLastColon) + dateTimeText.substring(idxLastColon + 3);
    return timeTextWithoutSec;
}

if (checkIfElemExists("clientDate")) {
    document.getElementById("clientDate").innerHTML = getDateNow();
}

if (checkIfElemExists("clientTime")) {
    document.getElementById("clientTime").innerHTML = getTimeNow();
}
