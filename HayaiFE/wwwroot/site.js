window.setSavedExam = function (data) {
    sessionStorage.setItem("savedExams", data);
};

window.getSavedExam = function () {
    return sessionStorage.getItem("savedExams") || "[]";
};

// SummaryDetails storage
window.setSummaryDetails = function (data) {
    sessionStorage.setItem("summaryDetails", data);
};

window.getSummaryDetails = function () {
    return sessionStorage.getItem("summaryDetails") || null;
};

window.clearSummaryDetails = function () {
    sessionStorage.removeItem("summaryDetails");
};

