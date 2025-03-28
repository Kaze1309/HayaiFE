window.setSavedExam = function (data) {
    sessionStorage.setItem("savedExams", data);
};

window.getSavedExam = function () {
    return sessionStorage.getItem("savedExams") || "[]";
};
