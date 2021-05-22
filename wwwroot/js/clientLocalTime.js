function getDateNowAsStr() {
    var dayNames = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
    var now = new Date();
    var iDay = now.getDay();
    var dayName = dayNames[iDay];
    var iMonth = now.getMonth() + 1;
    var monthStr = addLeadingZeros(iMonth, 2);;
    var dateStr = addLeadingZeros(now.getDate(), 2);
    var finalStr = now.getFullYear().toString() + "-" + monthStr + "-" + dateStr + " " + dayName;
    return finalStr;
}

function getTimeNowAsStr() {
    var now = new Date();

    var hrVal = now.getHours();
    var ampm = hrVal >= 12 ? "PM" : "AM";
    hrVal = hrVal == 0 ? 12 : hrVal;
    hrVal = hrVal > 12 ? hrVal % 12 : hrVal;

    var hrStr = addLeadingZeros(hrVal, 2);
    var minStr = addLeadingZeros(now.getMinutes(), 2);

    var timeStr = hrStr + ":" + minStr + " " + ampm;
    return timeStr;
}

function addLeadingZeros(intVal, desiredDigitCnt) {
    var intValAsStr = Number(intVal).toString();
    while (intValAsStr.length < desiredDigitCnt) {
        intValAsStr = "0" + intValAsStr;
    }
    return intValAsStr;
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
// JavaScript's sign convention seems to be different for timezoneoffset
// That is why OI am having to invert it
//
function getClientTimeZoneOffsetSec() {
    var now = new Date();
    var timeZoneOffsetMin = now.getTimezoneOffset();
    return (timeZoneOffsetMin * (-60));
}

function getCityName() {
    var cityName = "";
    if (checkIfElemExists("cityName")) {
        cityName = document.getElementById("cityName").innerHTML;
    }
    return cityName;
}

if (checkIfElemExists("clientDate")) {
    document.getElementById("clientDate").innerHTML = getDateNowAsStr();
}

if (checkIfElemExists("clientTime")) {
    document.getElementById("clientTime").innerHTML = getTimeNowAsStr();
}

if (checkIfElemExists("CityTimeZoneOffset")) {
    var cityTimeZoneOffsetSecStr = $("#CityTimeZoneOffset").val();
    var cityTimeZoneOffsetSec = Number(cityTimeZoneOffsetSecStr);
    var clientTimeZoneOffsetSec = getClientTimeZoneOffsetSec();
    var diffHrs = (cityTimeZoneOffsetSec - clientTimeZoneOffsetSec) / (60 * 60);
    var msg = "";
    var cityName = getCityName();
    if (diffHrs > 0) {
        msg = cityName + " is ahead of you by " + Math.abs(diffHrs) + " hrs";
    }
    else if (diffHrs < 0) {
        msg = cityName + " is behind you by " + Math.abs(diffHrs) + " hrs";
    }
    else {
        msg = "Same Time Zone";
    }
    if (checkIfElemExists("timeDiff")) {
        document.getElementById("timeDiff").innerHTML = msg;
    }
}